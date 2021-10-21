using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataTypeDropdown : DropdownSelection
{
    [System.NonSerialized]
    public string dataType = "";

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

        dataType = options[0];
    }

    private void ChangePlayerDataType(int value)
    {
        dataType = dropdown.options[value].text;
    }
}
