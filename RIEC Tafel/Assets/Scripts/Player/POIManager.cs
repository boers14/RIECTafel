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

    [SerializeField]
    private Transform table = null;

    [System.NonSerialized]
    public GameManager.DataType dataType = GameManager.DataType.Regular;

    [SerializeField]
    private GameObject regularPOI = null, policePOI = null, taxPOI = null, ppoPOI = null, bankPOI = null, emptyTransform = null;

    private Vector3 offset = Vector3.zero, extraOffset = Vector3.zero;

    private List<GameObject> allPOIs = new List<GameObject>();

    [SerializeField]
    private BackToStartPositionButton startPositionButton = null;

    [SerializeField]
    private POISelectionDropdown poiSelectionDropdown = null;

    [System.NonSerialized]
    public List<string> conclusions = new List<string>(), indications = new List<string>(), featureTypes = new List<string>(), 
        extraExplanations = new List<string>(), dutchNamesForRoles = new List<string>();

    private List<int> poiHits = new List<int>();

    private void Start()
    {
        offset = table.transform.position;
        offset.y += table.transform.localScale.y / 2;
    }

    public void SetLocationData(List<string> locationData, List<string> dataTypes, List<string> neededFeatureTypes, List<string> neededExtraInfo, 
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

        featureTypes = neededFeatureTypes;
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
                    POI = CreatePOI(POI, regularPOI, "Algemeen", i);
                    break;
                case GameManager.DataType.Tax:
                    POI = CreatePOI(POI, taxPOI, "Belasting", i);
                    break;
                case GameManager.DataType.Police:
                    POI = CreatePOI(POI, policePOI, "Politie", i);
                    break;
                case GameManager.DataType.PPO:
                    POI = CreatePOI(POI, ppoPOI, "OM", i);
                    break;
                case GameManager.DataType.Bank:
                    POI = CreatePOI(POI, bankPOI, "Bank", i);
                    break;
            }
            allPOIs.Add(POI);
            locationCoordinates.Add(coordinate);

            int amountOfHits = 3;
            string[] amountOfHitsString = extraExplanations[i].Split(new string[] { "Hoeveelheid hits:" }, System.StringSplitOptions.None);
            if (amountOfHitsString.Length > 1)
            {
                amountOfHits = int.Parse(amountOfHitsString[1]);
            }
            amountOfHits -= 2;
            poiHits.Add(amountOfHits);

            Vector3 newScale = POI.transform.localScale * 10;
            POI.GetComponent<POIText>().UpdateScaleOfPoi(newScale, amountOfHits);
        }

        startPositionButton.startPosition = locationCoordinates[0];
        poiSelectionDropdown.FillAllCoordinatesList(locationCoordinates, dutchNamesForRoles, featureTypes);

        map.GetComponent<MoveMap>().SetNewMapCenter(locationCoordinates[0]);
    }

    private GameObject CreatePOI(GameObject POI, GameObject prefabPOI, string roleText, int index)
    {
        dutchNamesForRoles.Add(roleText);
        POI = Instantiate(prefabPOI, Vector3.zero, Quaternion.identity);
        POI.GetComponent<POIText>().SetText(roleText + ": " + featureTypes[index], extraExplanations[index]);

        return POI;
    }

    public void MovePOIs(Vector3 movement)
    {
        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].transform.position += movement;
        }

        CheckPOIVisibility();
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
            allPOIs[i].GetComponent<POIText>().UpdateScaleOfPoi(newScale, poiHits[i]);
        }
        SetPOIMapPosition();
    }

    public void SetPOIMapPosition()
    {
        GameObject rotationObject = Instantiate(emptyTransform, map.transform.position, Quaternion.identity);
        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].transform.SetParent(null);
            Vector3 position = Conversions.GeoToWorldPosition(locationCoordinates[i], map.CenterMercator, map.WorldRelativeScale).ToVector3xz();
            // * the scale of the building
            position = offset + (position * 0.02f * map.transform.localScale.x) + extraOffset;
            allPOIs[i].transform.position = position;
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
}
