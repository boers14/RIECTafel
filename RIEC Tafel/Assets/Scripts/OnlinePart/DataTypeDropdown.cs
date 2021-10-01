using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataTypeDropdown : DropdownSelection
{
    [SerializeField]
    private POIManager poiManager = null;

    public override void Start()
    {
        base.Start();
        dropdown.onValueChanged.AddListener(ChangePlayerDataType);

        dropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < System.Enum.GetNames(typeof(GameManager.DataType)).Length; i++)
        {
            GameManager.DataType option = (GameManager.DataType)i;
            options.Add(option.ToString());
        }
        dropdown.AddOptions(options);
    }

    private void ChangePlayerDataType(int value)
    {
        poiManager.dataType = (GameManager.DataType)value;
    }
}
