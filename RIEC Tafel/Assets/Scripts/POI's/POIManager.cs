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

    /// <summary>
    /// Initialize variables
    /// </summary>

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

    /// <summary>
    /// Save the location data given from the game manager into the POI manager. Start transforming location names into 
    /// unity positions
    /// </summary>

    public void SetLocationData(List<string> locationData, List<string> dataTypes, List<string> neededAmounts, 
        List<string> neededExtraInfo, List<string> conclusions, List<string> indications, string cityName)
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

        StartCoroutine(GetLocationData(locationData, 0, true, () => CreateAllPOIsConnectedToLocationData(dataTypes, cityName), 0));
    }

    /// <summary>
    /// Loop throught all location data and transform them in to world coordinates.
    /// </summary>

    private IEnumerator GetLocationData(List<string> locationData, int currentLocation, bool addLocationData,
        UnityAction functionAfterLocationGettingAllLocationData, int amountOfTriesForCurrentLocation)
    {
        yield return new WaitForSeconds(0.15f);

        // Transform location to a city name
        string locationCityName = "";
        locationCityName = locationData[currentLocation].Split(',')[1];
        locationCityName = regex.Replace(locationCityName, "");
        if (locationCityName.StartsWith(" "))
        {
            locationCityName = locationCityName.Remove(0, 1);
        }

        // Queue the cityname for geocoding
        resource.Query = locationCityName;
        MapboxAccess.Instance.Geocoder.Geocode(resource, HandleGeocoderResponse);
        amountOfTriesForCurrentLocation++;

        // If last location is almost equal to current gotten location, try again
        if (coordinateGotFromLocationData.x > lastLocation.x - 0.0001 && coordinateGotFromLocationData.x < lastLocation.x + 0.0001 &&
            coordinateGotFromLocationData.y > lastLocation.y - 0.0001 && coordinateGotFromLocationData.y < lastLocation.y + 0.0001 && 
            amountOfTriesForCurrentLocation <= 3 || coordinateGotFromLocationData.x <= 0.0001 
            && coordinateGotFromLocationData.y <= 0.0001 && amountOfTriesForCurrentLocation <= 3)
        {
            StartCoroutine(GetLocationData(locationData, currentLocation, addLocationData, functionAfterLocationGettingAllLocationData,
                amountOfTriesForCurrentLocation));
        }
        else
        {
            // if the location was succesfully found, add the location to the list of currently found location coordinates
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
                // If all locations are found, stop looking for more locations and continue with the next function
                functionAfterLocationGettingAllLocationData.Invoke();
            }
            else
            {
                // If there are still more locations to handle perform this function again
                StartCoroutine(GetLocationData(locationData, currentLocation, addLocationData, functionAfterLocationGettingAllLocationData,
                    amountOfTriesForCurrentLocation));
            }
        }
    }

    /// <summary>
    /// Function from mapbox to find the location data
    /// </summary>

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

    /// <summary>
    /// Create all required POI's, set their color, text and scale
    /// </summary>

    private void CreateAllPOIsConnectedToLocationData(List<string> dataTypes, string cityName)
    {
        for (int i = 0; i < dataTypes.Count; i++)
        {
            GameManager.DataType dataType = (GameManager.DataType)System.Enum.Parse(typeof(GameManager.DataType), dataTypes[i]);
            GameObject POI = null;

            // Based on datatype create a POI
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

            // Based on amount of hits, decide scale of POI
            int amountOfHits = 3;
            string[] amountOfHitsString = featureAmounts[i].Split(new string[] { "Hoeveelheid hits:" }, System.StringSplitOptions.None);
            if (amountOfHitsString.Length > 1)
            {
                amountOfHits = int.Parse(amountOfHitsString[1]);
            }
            // has a minimum of 3 hits, and thus this needs to be a starting length of 1 when 3
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

        // Finalize the map initialisation process by starting the player of at the selected starting city
        StartCoroutine(GetLocationData(locationData, 0, false, () => FinishMapInitialisation(), 0));
    }

    /// <summary>
    /// Sets the POI color to the given color and sets other POI variables based on the index of the POI
    /// </summary>

    private GameObject CreatePOI(GameObject POI, Color32 color, int index)
    {
        POI = Instantiate(poiPrefab, Vector3.zero, Quaternion.identity);
        Color32 selectedColor = color;
        selectedColor.a = 150;
        POI.GetComponent<MeshRenderer>().material.color = selectedColor;
        POI.GetComponent<POIText>().SetText(locationNames[index] + ": " + featureAmounts[index], extraExplanations[index], transform, 
            moveMap, moveMap.inputDevices);

        return POI;
    }

    /// <summary>
    /// Initialize the last variables, set the map center to the starting city and enable the player to choose a seat
    /// </summary>

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
        PlayerConnection player = FindObjectOfType<PlayerConnection>();
        if (player)
        {
            player.FetchOwnPlayer().EnableChooseSeatButtons();
        }
    }

    /// <summary>
    /// Mapbox didnt work with parenting and thus scaling, rotating and moving need to be done manually
    /// This moves the POI's based on the given movement and checks whether they should be on/ off
    /// </summary>

    public void MovePOIs(Vector3 movement)
    {
        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].transform.position += movement;
        }

        CheckPOIVisibility();
    }

    /// <summary>
    /// Parents the POI's to the rotation object before its starts rotating, then also unparents them
    /// </summary>

    public void ParentPOIs(Transform parentObject)
    {
        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].transform.SetParent(parentObject);
        }
    }

    /// <summary>
    /// Updates the scale of the current POI's based on the new scale
    /// </summary>

    public void SetPOIsScale(Vector3 newScale)
    {
        for (int i = 0; i < allPOIs.Count; i++)
        {
            allPOIs[i].GetComponent<POIText>().UpdateScaleOfPoi(newScale, poiHits[i], moveMap.minimumScale, moveMap.maximumScale, poiScale);
        }
        SetPOIMapPosition();
    }

    /// <summary>
    /// Calculate the position of all POI's on the map depending on the location coordinates.
    /// Create a empty object for all POI's to be parented and rotate the object for all objects to have the correct position.
    /// </summary>

    private void SetPOIMapPosition()
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
        }
        Destroy(rotationObject);
        CheckPOIVisibility();
    }

    /// <summary>
    /// Set offset of POI's equal to mapoffset for when they are scaled they still have the map offset calculated in for their 
    /// position
    /// </summary>

    public void SetExtraOffset(Vector3 mapOffset)
    {
        extraOffset.x = mapOffset.x;
        extraOffset.z = mapOffset.z;
    }

    /// <summary>
    /// Cant be made visible when the choose seat menu is active.
    /// Checks whether the current postion of the POI is on the table or outside of it. When on the tabel the POI is visible else not.
    /// </summary>

    public void CheckPOIVisibility()
    {
        if (chooseSeatButtonsParent)
        {
            if (chooseSeatButtonsParent.gameObject.activeSelf) { return; }
        }

        for (int i = 0; i < allPOIs.Count; i++)
        {
            // POI's can be defualt invisble with the POI enable dropdown.
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

    /// <summary>
    /// Returns the closest active POI to a given position
    /// </summary>

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

    /// <summary>
    /// Change the rotation and position to one of a given transform (for when the player seats)
    /// </summary>

    public void ChangePOIManagerTransform(Transform transform)
    {
        this.transform.position = transform.position;
        this.transform.rotation = transform.rotation;
        table.rotation = transform.rotation;
    }
}
