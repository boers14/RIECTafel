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
        dataType = GameManager.DataType.Police.ToString();
        dropdown.onValueChanged.AddListener(ChangePlayerDataType);

        dropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < System.Enum.GetNames(typeof(GameManager.DataType)).Length; i++)
        {
            GameManager.DataType option = (GameManager.DataType)i;
            string optionInDutch = "";

            switch (option)
            {
                case GameManager.DataType.Regular:
                    continue;
                case GameManager.DataType.Police:
                    optionInDutch = "Politie";
                    break;
                case GameManager.DataType.Tax:
                    optionInDutch = "Belasting";
                    break;
                case GameManager.DataType.PPO:
                    optionInDutch = "OM";
                    break;
                case GameManager.DataType.Bank:
                    optionInDutch = "Bank";
                    break;
            }

            options.Add(optionInDutch);
        }
        dropdown.AddOptions(options);
    }

    private void ChangePlayerDataType(int value)
    {
        dataType = ((GameManager.DataType)(value + 1)).ToString();
    }
}
