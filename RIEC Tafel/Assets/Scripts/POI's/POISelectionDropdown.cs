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

    private List<Vector2d> allPOICoordinates = new List<Vector2d>(), currenSelectionPOICoordinates = new List<Vector2d>();

    private List<string> optionNames = new List<string>();

    public override void Start()
    {
        base.Start();
        dropdown.ClearOptions();
        dropdown.onValueChanged.AddListener(SetPOIAsCenterCoordinate);
    }

    private void SetPOIAsCenterCoordinate(int selectedPOI)
    {
        map.SetNewMapCenter(currenSelectionPOICoordinates[selectedPOI]);
    }

    public void FillAllCoordinatesList(List<Vector2d> allCoordinates, List<string> featureTypes, List<string> locationNames)
    {
        allPOICoordinates = allCoordinates;
        dropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < allPOICoordinates.Count; i++)
        {
            currenSelectionPOICoordinates.Add(allPOICoordinates[i]);
            optionNames.Add(locationNames[i] + ": " + featureTypes[i]);
            options.Add(locationNames[i] + ": " + featureTypes[i]);
        }

        dropdown.AddOptions(options);
    }

    public void EnableOptionsCoordinatesList(List<bool> enabledOptions)
    {
        dropdown.ClearOptions();
        currenSelectionPOICoordinates.Clear();

        List<string> options = new List<string>();
        for (int i = 0; i < optionNames.Count; i++)
        {
            if (enabledOptions[i])
            {
                options.Add(optionNames[i]);
                currenSelectionPOICoordinates.Add(allPOICoordinates[i]);
            }
        }

        dropdown.AddOptions(options);
    }
}
