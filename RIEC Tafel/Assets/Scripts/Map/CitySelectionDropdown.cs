using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CitySelectionDropdown : DropdownSelection
{
    [SerializeField]
    private NetworkManagerRIECTafel networkManager = null;

    public override void Start()
    {
        base.Start();
        dropdown.onValueChanged.AddListener(SetCityForNetworkManager);
        SetCityForNetworkManager(0);
    }

    private void SetCityForNetworkManager(int selectedCity)
    {
        networkManager.cityName = dropdown.options[selectedCity].text;
    }
}
