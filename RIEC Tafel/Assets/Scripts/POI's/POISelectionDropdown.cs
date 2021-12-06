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

    private List<Vector2d> allPOICoordinates = new List<Vector2d>();

    public override void Start()
    {
        base.Start();
        dropdown.ClearOptions();
        dropdown.onValueChanged.AddListener(SetPOIAsCenterCoordinate);
    }

    private void SetPOIAsCenterCoordinate(int selectedPOI)
    {
        map.SetNewMapCenter(allPOICoordinates[selectedPOI]);
    }

    public void FillAllCoordinatesList(List<Vector2d> allCoordinates, List<string> featureTypes, List<string> locationNames)
    {
        allPOICoordinates = allCoordinates;
        dropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < allPOICoordinates.Count; i++)
        {
            options.Add(locationNames[i] + ": " + featureTypes[i]);
        }

        dropdown.AddOptions(options);
    }
}
