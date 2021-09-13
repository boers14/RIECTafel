using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataTypeDropdown : MonoBehaviour
{
    [SerializeField]
    private VRPlayer player = null;

    private Dropdown dropdown = null;

    private void Start()
    {
        dropdown = GetComponent<Dropdown>();
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
        player.dataType = (GameManager.DataType)value;
    }
}
