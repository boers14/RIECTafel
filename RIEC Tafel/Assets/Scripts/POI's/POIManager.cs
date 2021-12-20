using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using Mapbox.Unity;
using Mapbox.Geocoding;
using System.Text.RegularExpressions;
using UnityEngine.Events;

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

    public Color32 regularPOIColor = Color.green, policePOIColor = Color.blue, taxPOIColor = Color.red, ppoPOIColor = Color.magenta, 
        bankPOIColor = Color.yellow;

    private Vector3 offset = Vector3.zero, extraOffset = Vector3.zero;

    [System.NonSerialized]
    public List<GameObject> allPOIs = new List<GameObject>();

    [SerializeField]
    private BackToStartPositionButton startPositionButton = null;

    [SerializeField]
    private POISelectionDropdown poiSelectionDropdown = null;

    [SerializeField]
    private POIEnableDropdown poiEnableDropdown = null;

    public float poiScale = 0.2f;

    [System.NonSerialized]
    public List<string> conclusions = new List<string>(), indications = new List<string>(), featureAmounts = new List<string>(), 
        extraExplanations = new List<string>(), locationNames = new List<string>();

    [System.NonSerialized]
    public List<int> poiHits = new List<int>();

    [System.NonSerialized]
    public List<bool> poiVisibility = new List<bool>();

    private List<DataExplanations> dataExplanations = new List<DataExplanations>();

    private Transform chooseSeatButtonsParent = null;

    private ForwardGeocodeResource resource = null;

    private Vector2d coordinateGotFromLocationData = Vector2d.zero, lastLocation = Vector2d.zero;

    private event System.Action<ForwardGeocodeResponse> onGeocoderResponse = delegate { };

    private Regex regex = new Regex("[0-9]{4} *[A-Z]{2} *");

    [System.NonSerialized]
    public bool allLocationDataIsInitialized = false;

    private void Start()
    {
        if (FindObjectOfType<ChooseSeatButton>())
        {
            chooseSeatButtonsParent = FindObjectOfType<ChooseSeatButton>().transform.parent;
        }

        dataType = (GameManager.DataType)System.Enum.Parse(typeof(GameManager.DataType), LogInManager.datatype);

        moveMap = map.GetComponent<MoveMap>();

        offset = table.transform.position;
        offset.y += table.transform.localScale.y / 2;

        dataExplanations.AddRange(FindObjectsOfType<DataExplanations>());
        resource = new ForwardGeocodeResource("");
    }

    public void SetLocationData(List<string> locationData, List<string> dataTypes, List<string> neededAmounts, List<string> neededExtraInfo, 
        List<string> conclusions, List<string> indications, string cityName)
    {
        poiHits.Clear();
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

        allLocationDataIsInitialized = true;
        FindObjectOfType<PlayerConnection>().FetchOwnPlayer().EnableChooseSeatButtons();
        //StartCoroutine(GetLocationData(locationData, 0, true, () => CreateAllPOIsConnectedToLocationData(dataTypes, cityName), 0));
    }

    private IEnumerator GetLocationData(List<string> locationData, int currentLocation, bool addLocationData,
        UnityAction functionAfterLocationGettingAllLocationData, int amountOfTriesForCurrentLocation)
    {
        yield return new WaitForSeconds(0.15f);

        string locationCityName = "";
        locationCityName = locationData[currentLocation].Split(',')[1];
        locationCityName = regex.Replace(locationCityName, "");
        if (locationCityName.StartsWith(" "))
        {
            locationCityName = locationCityName.Remove(0, 1);
        }

        resource.Query = locationCityName;
        MapboxAccess.Instance.Geocoder.Geocode(resource, HandleGeocoderResponse);
        amountOfTriesForCurrentLocation++;

        if (coordinateGotFromLocationData.x > lastLocation.x - 0.0001 && coordinateGotFromLocationData.x < lastLocation.x + 0.0001 &&
            coordinateGotFromLocationData.y > lastLocation.y - 0.0001 && coordinateGotFromLocationData.y < lastLocation.y + 0.0001 && 
            amountOfTriesForCurrentLocation <= 3 ||coordinateGotFromLocationData.x <= 0.0001 && coordinateGotFromLocationData.y <= 0.0001 &&
            amountOfTriesForCurrentLocation <= 3)
        {
            StartCoroutine(GetLocationData(locationData, currentLocation, addLocationData, functionAfterLocationGettingAllLocationData,
                amountOfTriesForCurrentLocation));
        }
        else
        {
            if (addLocationData)
            {
                locationNames.Add(locationCityName);
                locationCoordinates.Add(coordinateGotFromLocationData);
            }
            amountOfTriesForCurrentLocation = 0;
            lastLocation = coordinateGotFromLocationData;
            currentLocation++;
            if (currentLocation >= locationData.Count)
            {
                functionAfterLocationGettingAllLocationData.Invoke();
            }
            else
            {
                StartCoroutine(GetLocationData(locationData, currentLocation, addLocationData, functionAfterLocationGettingAllLocationData,
                    amountOfTriesForCurrentLocation));
            }
        }
    }

    private void HandleGeocoderResponse(ForwardGeocodeResponse res)
    {
        if (res == null)
        {
            print("no geocode response");
        }
        else if (null != res.Features && res.Features.Count > 0)
        {
            coordinateGotFromLocationData = res.Features[0].Center;
        }
        onGeocoderResponse(res);
    }

    private void CreateAllPOIsConnectedToLocationData(List<string> dataTypes, string cityName)
    {
        for (int i = 0; i < dataTypes.Count; i++)
        {
            GameManager.DataType dataType = (GameManager.DataType)System.Enum.Parse(typeof(GameManager.DataType), dataTypes[i]);
            GameObject POI = null;

            switch (dataType)
            {
                case GameManager.DataType.Regular:
                    POI = CreatePOI(POI, regularPOIColor, i);
                    break;
                case GameManager.DataType.Tax:
                    POI = CreatePOI(POI, taxPOIColor, i);
                    break;
                case GameManager.DataType.Police:
                    POI = CreatePOI(POI, policePOIColor, i);
                    break;
                case GameManager.DataType.PPO:
                    POI = CreatePOI(POI, ppoPOIColor, i);
                    break;
                case GameManager.DataType.Bank:
                    POI = CreatePOI(POI, bankPOIColor, i);
                    break;
            }
            allPOIs.Add(POI);
            poiVisibility.Add(true);

            int amountOfHits = 3;
            string[] amountOfHitsString = featureAmounts[i].Split(new string[] { "Hoeveelheid hits:" }, System.StringSplitOptions.None);
            if (amountOfHitsString.Length > 1)
            {
                amountOfHits = int.Parse(amountOfHitsString[1]);
            }
            amountOfHits -= 2;
            poiHits.Add(amountOfHits);

            Vector3 newScale = POI.transform.localScale * (1 / poiScale);
            POI.GetComponent<POIText>().UpdateScaleOfPoi(newScale, amountOfHits, moveMap.minimumScale, moveMap.maximumScale, poiScale);
            POI.SetActive(false);
        }

        lastLocation = Vector2d.zero;
        cityName = ",1111 AA " + cityName; 
        List<string> locationData = new List<string>();
        locationData.AddRange(new string[] { cityName, cityName });

        StartCoroutine(GetLocationData(locationData, 0, false, () => FinishMapInitialisation(), 0));
    }

    private GameObject CreatePOI(GameObject POI, Color32 color, int index)
    {
        POI = Instantiate(poiPrefab, Vector3.zero, Quaternion.identity);
        Color32 selectedColor = color;
        selectedColor.a = 150;
        POI.GetComponent<MeshRenderer>().material.color = selectedColor;
        POI.GetComponent<POIText>().SetText(locationNames[index] + ": " + featureAmounts[index], extraExplanations[index], transform, moveMap, 
            moveMap.inputDevices);

        return POI;
    }

    private void FinishMapInitialisation()
    {
        startPositionButton.startPosition = coordinateGotFromLocationData;
        poiSelectionDropdown.FillAllCoordinatesList(locationCoordinates, featureAmounts, locationNames);
        poiEnableDropdown.FillDropDownList(allPOIs, extraExplanations, this);

        for (int i = 0; i < dataExplanations.Count; i++)
        {
            dataExplanations[i].FillOptionsList();
        }

        moveMap.SetNewMapCenter(coordinateGotFromLocationData);
        allLocationDataIsInitialized = true;
        FindObjectOfType<PlayerConnection>().FetchOwnPlayer().EnableChooseSeatButtons();
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
            pos = offset + (pos * 0.02f * map.transform.localScale.x) + extraOffset;
            allPOIs[i].transform.position = pos;

            allPOIs[i].transform.SetParent(rotationObject.transform);
        }

        Vector3 newRot = map.transform.eulerAngles;
        rotationObject.transform.eulerAngles = newRot;

        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].transform.SetParent(null);
            allPOIs[i].GetComponent<POIText>().SetTextRotation(transform);
        }
        Destroy(rotationObject);
        CheckPOIVisibility();
    }

    public void RotatePOITextToPlayer()
    {
        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].GetComponent<POIText>().SetTextRotation(transform);
        }
    }

    public void SetExtraOffset(Vector3 mapOffset)
    {
        extraOffset.x = mapOffset.x;
        extraOffset.z = mapOffset.z;
    }

    public void CheckPOIVisibility()
    {
        if (chooseSeatButtonsParent)
        {
            if (chooseSeatButtonsParent.gameObject.activeSelf) { return; }
        }

        for (int i = 0; i < allPOIs.Count; i++)
        {
            if (!poiVisibility[i])
            {
                allPOIs[i].SetActive(false);
                continue;
            }

            if (allPOIs[i].transform.position.x < table.position.x - table.localScale.x / 2 ||
                allPOIs[i].transform.position.x > table.position.x + table.localScale.x / 2 ||
                allPOIs[i].transform.position.z < table.position.z - table.localScale.z / 2 ||
                allPOIs[i].transform.position.z > table.position.z + table.localScale.z / 2)
            {
                allPOIs[i].SetActive(false);
            }
            else
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
