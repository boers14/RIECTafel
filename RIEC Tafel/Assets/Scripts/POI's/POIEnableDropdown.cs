using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class POIEnableDropdown : DropdownSelection
{
    private Dictionary<string, List<POIText>> poisByFeatureType = new Dictionary<string, List<POIText>>();

    private List<GameObject> allPOIs = new List<GameObject>();

    private List<bool> poiActiveStates = new List<bool>();

    private POIManager poiManager = null;

    private MiniMap miniMap = null;

    private POISelectionDropdown poiSelectionDropdown = null;

    /// <summary>
    /// Initialize variables
    /// </summary>

    public override void Start()
    {
        base.Start();
        miniMap = FindObjectOfType<MiniMap>();
        poiSelectionDropdown = FindObjectOfType<POISelectionDropdown>();
        dropdown.ClearOptions();
    }

    /// <summary>
    /// Fill dropdown with all different feature types and add them to the dictionary. Then check for all POI's which feature types
    /// they have. For each feature type they have add them to the dictionary.
    /// </summary>

    public void FillDropDownList(List<GameObject> allPOIs, List<string> featureTypes, POIManager poiManager)
    {
        poisByFeatureType.Clear();
        this.poiManager = poiManager;
        this.allPOIs = allPOIs;

        dropdown.ClearOptions();
        List<string> allOptions = new List<string>();
        allOptions.Add("Alle POI's");

        for (int i = 0; i < featureTypes.Count; i++)
        {
            string[] allFeaturesInPOI = featureTypes[i].Split(',');
            for (int j = 0; j < allFeaturesInPOI.Length; j++)
            {
                // if feature type starts with an empty space, remove it
                if (char.IsWhiteSpace(allFeaturesInPOI[j][0]))
                {
                    allFeaturesInPOI[j] = allFeaturesInPOI[j].Remove(0, 1);
                }

                // Add new feature type to dictionary
                if (!poisByFeatureType.ContainsKey(allFeaturesInPOI[j]))
                {
                    allOptions.Add(allFeaturesInPOI[j]);
                    poisByFeatureType.Add(allFeaturesInPOI[j], new List<POIText>());
                }
            }
        }

        for (int i = 0; i < allPOIs.Count; i++)
        {
            poiActiveStates.Add(true);
            POIText poiText = allPOIs[i].GetComponent<POIText>();
            for (int j = 1; j < allOptions.Count; j++)
            {
                // Add POI to connected feature part of dictionary
                if (poiText.textExtend.Contains(allOptions[j]))
                {
                    poisByFeatureType[allOptions[j]].Add(poiText);
                }
            }
        }

        dropdown.AddOptions(allOptions);
        // Shows given text here on the dropdown itself instead of an option
        dropdown.transform.GetChild(0).GetComponent<TMP_Text>().text = "Selecteer POI's om te zien";
    }

    /// <summary>
    /// Unless the value is 0, disable all POI active states first. Then check which POI's have the selected feature and enable 
    /// those poi active states.
    /// </summary>

    public void EnablePOIs(int value)
    {
        if (value != 0)
        {
            for (int i = 0; i < poiActiveStates.Count; i++)
            {
                poiActiveStates[i] = false;
            }

            string selectedValue = dropdown.options[value].text;
            for (int i = 0; i < poisByFeatureType[selectedValue].Count; i++)
            {
                poiActiveStates[allPOIs.IndexOf(poisByFeatureType[selectedValue][i].gameObject)] = true;
            }
        } else
        {
            // Option 0 is enable all POI's
            for (int i = 0; i < poiActiveStates.Count; i++)
            {
                poiActiveStates[i] = true;
            }
        }

        dropdown.transform.GetChild(0).GetComponent<TMP_Text>().text = "Selecteer POI's om te zien";

        // Update all affected components of the program
        miniMap.SetActiveStateOfPOIs(poiActiveStates);
        poiSelectionDropdown.EnableOptionsCoordinatesList(poiActiveStates);
        poiManager.poiVisibility = poiActiveStates;
        poiManager.CheckPOIVisibility();
    }
}
