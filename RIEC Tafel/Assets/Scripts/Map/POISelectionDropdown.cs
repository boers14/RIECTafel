using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Utils;

public class POISelectionDropdown : MonoBehaviour
{
    [SerializeField]
    private MoveMap map = null;

    private Dropdown dropdown = null;

    private List<Vector2d> allPOICoordinates = new List<Vector2d>();

    private void Start()
    {
        dropdown = GetComponent<Dropdown>();
        dropdown.ClearOptions();
        dropdown.onValueChanged.AddListener(SetPOIAsCenterCoordinate);
    }

    private void SetPOIAsCenterCoordinate(int selectedPOI)
    {
        map.SetNewMapCenter(allPOICoordinates[selectedPOI]);
    }

    public void FillAllCoordinatesList(List<Vector2d> allCoordinates)
    {
        allPOICoordinates = allCoordinates;
        dropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < allPOICoordinates.Count; i++)
        {
            options.Add(allPOICoordinates[i].ToString());
        }

        dropdown.AddOptions(options);
    }
}
