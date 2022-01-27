using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class POIEnableToggle : MonoBehaviour
{
    /// <summary>
    /// Perform the function of the POI enable dropdown via the toggle so it doesnt show which option was selected.
    /// </summary>

    private void Start()
    {
        GetComponent<Toggle>().onValueChanged.AddListener(OnToggleSwitch);
    }

    private void OnToggleSwitch(bool toggle)
    {
        if (!toggle) { return; }

        // The name of the object contains which option number this object is
        string nameOfObject = name;
        nameOfObject = nameOfObject.Split(':')[0];
        nameOfObject = nameOfObject.Split(' ')[1];
        if (int.TryParse(nameOfObject, out int value)) {
            GetComponentInParent<POIEnableDropdown>().EnablePOIs(value);
        } else
        {
            print(nameOfObject);
        }
    }
}
