using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;

public class POIManager : MonoBehaviour
{
    [System.NonSerialized]
    public List<Vector2d> locationCoordinates = new List<Vector2d>();

    [SerializeField]
    private AbstractMap map = null;

    private MoveMap moveMap = null;

    [SerializeField]
    private Transform table = null;

    [System.NonSerialized]
    public GameManager.DataType dataType = GameManager.DataType.Regular;

    [SerializeField]
    private GameObject poiPrefab = null, emptyTransform = null;

    [SerializeField]
    private Color32 regularPOIColor = Color.green, policePOIColor = Color.blue, taxPOIColor = Color.red, ppoPOIColor = Color.magenta, 
        bankPOIColor = Color.yellow;

    private Vector3 offset = Vector3.zero, extraOffset = Vector3.zero;

    private List<GameObject> allPOIs = new List<GameObject>();

    private List<Vector3> poiOffsets = new List<Vector3>();

    [SerializeField]
    private BackToStartPositionButton startPositionButton = null;

    [SerializeField]
    private POISelectionDropdown poiSelectionDropdown = null;

    [SerializeField]
    private POIEnableDropdown poiEnableDropdown = null;

    [SerializeField]
    private float poiScale = 0.2f;

    [System.NonSerialized]
    public List<string> conclusions = new List<string>(), indications = new List<string>(), featureAmounts = new List<string>(), 
        extraExplanations = new List<string>(), dutchNamesForRoles = new List<string>();

    private List<int> poiHits = new List<int>();

    private List<POIArrows> poiArrows = new List<POIArrows>();

    [System.NonSerialized]
    public List<bool> poiVisibility = new List<bool>();

    private void Start()
    {
        moveMap = map.GetComponent<MoveMap>();

        offset = table.transform.position;
        offset.y += table.transform.localScale.y / 2;

        POIArrows[] poiArrows = FindObjectsOfType<POIArrows>();
        this.poiArrows.AddRange(poiArrows);
    }

    public void SetLocationData(List<string> locationData, List<string> dataTypes, List<string> neededAmounts, List<string> neededExtraInfo, 
        List<string> conclusions, List<string> indications)
    {
        poiHits.Clear();
        dutchNamesForRoles.Clear();
        locationCoordinates.Clear();
        for (int i = 0; i < allPOIs.Count; i++)
        {
            Destroy(allPOIs[i]);
        }
        allPOIs.Clear();

        featureAmounts = neededAmounts;
        extraExplanations = neededExtraInfo;
        this.conclusions = conclusions;
        this.indications = indications;

        for (int i = 0; i < dataTypes.Count; i++)
        {
            GameManager.DataType dataType = (GameManager.DataType)System.Enum.Parse(typeof(GameManager.DataType), dataTypes[i]);
            Vector2d coordinate = map.ReturnCoordinateFromString(locationData[i]);
            GameObject POI = null;

            switch (dataType)
            {
                case GameManager.DataType.Regular:
                    POI = CreatePOI(POI, regularPOIColor, "Algemeen", i);
                    break;
                case GameManager.DataType.Tax:
                    POI = CreatePOI(POI, taxPOIColor, "Belasting", i);
                    break;
                case GameManager.DataType.Police:
                    POI = CreatePOI(POI, policePOIColor, "Politie", i);
                    break;
                case GameManager.DataType.PPO:
                    POI = CreatePOI(POI, ppoPOIColor, "OM", i);
                    break;
                case GameManager.DataType.Bank:
                    POI = CreatePOI(POI, bankPOIColor, "Bank", i);
                    break;
            }
            allPOIs.Add(POI);
            locationCoordinates.Add(coordinate);
            poiOffsets.Add(new Vector3(Random.Range(-poiScale / 2, poiScale / 2), 0, Random.Range(-poiScale / 2, poiScale / 2)));
            poiVisibility.Add(true);

            int amountOfHits = 3;
            string[] amountOfHitsString = neededAmounts[i].Split(new string[] { "Hoeveelheid hits:" }, System.StringSplitOptions.None);
            if (amountOfHitsString.Length > 1)
            {
                amountOfHits = int.Parse(amountOfHitsString[1]);
            }
            amountOfHits -= 2;
            poiHits.Add(amountOfHits);

            Vector3 newScale = POI.transform.localScale * (1 / poiScale);
            POI.GetComponent<POIText>().UpdateScaleOfPoi(newScale, amountOfHits, moveMap.minimumScale, moveMap.maximumScale, poiScale);
        }

        startPositionButton.startPosition = locationCoordinates[0];
        poiSelectionDropdown.FillAllCoordinatesList(locationCoordinates, dutchNamesForRoles, featureAmounts);
        poiEnableDropdown.FillDropDownList(allPOIs, dutchNamesForRoles, this);

        moveMap.SetNewMapCenter(locationCoordinates[0]);
    }

    private GameObject CreatePOI(GameObject POI, Color32 color, string roleText, int index)
    {
        dutchNamesForRoles.Add(roleText);
        POI = Instantiate(poiPrefab, Vector3.zero, Quaternion.identity);
        POI.GetComponent<MeshRenderer>().material.color = color;
        POI.GetComponent<POIText>().SetText(roleText + ": " + featureAmounts[index], extraExplanations[index]);

        return POI;
    }

    public void MovePOIs(Vector3 movement)
    {
        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].transform.position += movement;
        }

        CheckPOIVisibility();
        SetPOIArrowText();
    }

    public void ParentPOIs(Transform parentObject, bool rotateText)
    {
        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].transform.SetParent(parentObject);

            if (rotateText)
            {
                allPOIs[i].GetComponent<POIText>().SetTextRotation(transform);
            }
        }
    }

    public void SetPOIsScale(Vector3 newScale)
    {
        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].GetComponent<POIText>().UpdateScaleOfPoi(newScale, poiHits[i], moveMap.minimumScale, moveMap.maximumScale, poiScale);
        }
        SetPOIMapPosition();
    }

    public void SetPOIMapPosition()
    {
        GameObject rotationObject = Instantiate(emptyTransform, map.transform.position, Quaternion.identity);
        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].transform.SetParent(null);
            Vector3 pos = Conversions.GeoToWorldPosition(locationCoordinates[i], map.CenterMercator, map.WorldRelativeScale).ToVector3xz();
            // * the scale of the building
            pos = offset + (pos * 0.02f * map.transform.localScale.x) + extraOffset + (poiOffsets[i] * map.transform.localScale.x);
            allPOIs[i].transform.position = pos;
            allPOIs[i].GetComponent<POIText>().SetTextRotation(transform);

            allPOIs[i].transform.SetParent(rotationObject.transform);
        }

        Vector3 newRot = map.transform.eulerAngles;
        rotationObject.transform.eulerAngles = newRot;

        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].transform.SetParent(null);
        }
        Destroy(rotationObject);

        CheckPOIVisibility();
        SetPOIArrowText();
    }

    public void SetPOIArrowText()
    {
        for (int i = 0; i < poiArrows.Count; i++)
        {
            poiArrows[i].SetArrowText(allPOIs, dutchNamesForRoles, poiVisibility);
        }
    }

    public void SetExtraOffset(Vector3 mapOffset)
    {
        extraOffset.x = mapOffset.x;
        extraOffset.z = mapOffset.z;
    }

    public void CheckPOIVisibility()
    {
        for (int i = 0; i < allPOIs.Count; i++)
        {
            if (!poiVisibility[i]) { continue; }

            if (allPOIs[i].transform.position.x < table.position.x - table.localScale.x / 2 ||
                allPOIs[i].transform.position.x > table.position.x + table.localScale.x / 2 ||
                allPOIs[i].transform.position.z < table.position.z - table.localScale.z / 2 ||
                allPOIs[i].transform.position.z > table.position.z + table.localScale.z / 2)
            {
                allPOIs[i].SetActive(false);
            } else
            {
                allPOIs[i].SetActive(true);
            }
        }
    }

    public Transform ReturnClosestPOIToTransform(Vector3 position)
    {
        Transform closestPOI = null;
        float closestDist = -1;
        for (int i = 0; i < allPOIs.Count; i++)
        {
            float distanceBetweenTransformAndPOI = Vector3.Distance(position, allPOIs[i].transform.position);
            if (distanceBetweenTransformAndPOI < closestDist || closestDist == -1)
            {
                if (allPOIs[i].activeSelf)
                {
                    closestDist = distanceBetweenTransformAndPOI;
                    closestPOI = allPOIs[i].transform;
                }
            }
        }

        return closestPOI;
    }

    public void ChangePOIManagerTransform(Transform transform)
    {
        this.transform.position = transform.position;
        this.transform.rotation = transform.rotation;
        table.rotation = transform.rotation;
    }
}
