using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Utils;
using TMPro;

public class POISelectionDropdown : DropdownSelection
{
    [SerializeField]
    private MoveMap map = null;

    private List<Vector2d> allPOICoordinates = new List<Vector2d>(), currentSelectionPOICoordinates = new List<Vector2d>();

    private List<string> optionNames = new List<string>();

    /// <summary>
    /// Initialize variables
    /// </summary>

    public override void Start()
    {
        base.Start();
        dropdown.ClearOptions();
        dropdown.onValueChanged.AddListener(SetPOIAsCenterCoordinate);
    }

    /// <summary>
    /// Uses the current selection of poi coordinates to grab the selected option as the new map center
    /// </summary>

    private void SetPOIAsCenterCoordinate(int selectedPOI)
    {
        map.SetNewMapCenter(currentSelectionPOICoordinates[selectedPOI]);
    }

    /// <summary>
    /// Creates a list of all coordinates and options names to remember. Then it loops throught them and adds them to dropdown
    /// list and current use of coordinates.
    /// </summary>

    public void FillAllCoordinatesList(List<Vector2d> allCoordinates, List<string> featureTypes, List<string> locationNames)
    {
        allPOICoordinates = allCoordinates;
        dropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < allPOICoordinates.Count; i++)
        {
            currentSelectionPOICoordinates.Add(allPOICoordinates[i]);
            optionNames.Add(locationNames[i] + ": " + featureTypes[i]);
            options.Add(locationNames[i] + ": " + featureTypes[i]);
        }

        dropdown.AddOptions(options);
    }

    /// <summary>
    /// Based on what options are availeble adjusts the current selection of POI coordinates
    /// </summary>

    public void EnableOptionsCoordinatesList(List<bool> enabledOptions)
    {
        dropdown.ClearOptions();
        currentSelectionPOICoordinates.Clear();

        List<string> options = new List<string>();
        for (int i = 0; i < optionNames.Count; i++)
        {
            if (enabledOptions[i])
            {
                options.Add(optionNames[i]);
                currentSelectionPOICoordinates.Add(allPOICoordinates[i]);
            }
        }

        dropdown.AddOptions(options);
    }
}
