using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class POIEnableToggle : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Toggle>().onValueChanged.AddListener(OnToggleSwitch);
    }

    private void OnToggleSwitch(bool toggle)
    {
        if (!toggle) { return; }

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
