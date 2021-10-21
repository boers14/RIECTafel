using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitySelectionDropdown : DropdownSelection
{
    public override void Start()
    {
        base.Start();
        dropdown.onValueChanged.AddListener(SetCityForNetworkManager);
        SetCityForNetworkManager(0);
    }

    private void SetCityForNetworkManager(int selectedCity)
    {
        ConnectionManager.cityName = dropdown.options[selectedCity].text;
    }
}
