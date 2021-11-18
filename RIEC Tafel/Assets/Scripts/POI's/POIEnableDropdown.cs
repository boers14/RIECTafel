using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class POIEnableDropdown : DropdownSelection
{
    private Dictionary<string, List<GameObject>> poisByName = new Dictionary<string, List<GameObject>>();

    private List<GameObject> allPOIs = new List<GameObject>();

    private POIManager poiManager = null;

    public override void Start()
    {
        base.Start();
        dropdown.ClearOptions();
    }

    public void FillDropDownList(List<GameObject> allPOIs, List<string> dutchNames, POIManager poiManager)
    {
        poisByName.Clear();
        this.poiManager = poiManager;
        this.allPOIs = allPOIs;

        dropdown.ClearOptions();
        List<string> allOptions = new List<string>();
        allOptions.Add("Alle POI's");

        for (int i = 0; i < allPOIs.Count; i++)
        {
            if (poisByName.ContainsKey(dutchNames[i]))
            {
                poisByName[dutchNames[i]].Add(allPOIs[i]);
            } else
            {
                allOptions.Add(dutchNames[i]);

                List<GameObject> uniquePOIsList = new List<GameObject>();
                uniquePOIsList.Add(allPOIs[i]);
                poisByName.Add(dutchNames[i], uniquePOIsList);
            }
        }

        dropdown.AddOptions(allOptions);
        dropdown.transform.GetChild(0).GetComponent<TMP_Text>().text = "Selecteer POI's om te (de)activeren";
    }

    public void EnablePOIs(int value)
    {
        bool enabled = true;
        if (poisByName.TryGetValue(dropdown.options[value].text, out List<GameObject> selectedPOIs))
        {
            enabled = !poiManager.poiVisibility[allPOIs.IndexOf(selectedPOIs[0])];
            SetActivePOIs(selectedPOIs, enabled);
        } else
        {
            int activeCounter = 0;
            for (int i = 0; i < allPOIs.Count; i++)
            {
                if (poiManager.poiVisibility[i])
                {
                    activeCounter++;
                }
            }

            if (activeCounter == allPOIs.Count)
            {
                enabled = false;
            }

            SetActivePOIs(allPOIs, enabled);
        }

        dropdown.transform.GetChild(0).GetComponent<TMP_Text>().text = "Selecteer POI's om te (de)activeren";
        poiManager.CheckPOIVisibility();
    }

    private void SetActivePOIs(List<GameObject> pois, bool enabled)
    {
        for (int i = 0; i < pois.Count; i++)
        {
            pois[i].SetActive(enabled);
            poiManager.poiVisibility[allPOIs.IndexOf(pois[i])] = enabled;
        }
    }
}
