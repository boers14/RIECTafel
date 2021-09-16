using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CitySelectionDropdown : MonoBehaviour
{
    [SerializeField]
    private NetworkManagerRIECTafel networkManager = null;

    private Dropdown dropdown = null;

    private void Start()
    {
        dropdown = GetComponent<Dropdown>();
        dropdown.onValueChanged.AddListener(SetCityForNetworkManager);
        SetCityForNetworkManager(0);
    }

    private void SetCityForNetworkManager(int selectedCity)
    {
        networkManager.cityName = dropdown.options[selectedCity].text;
    }
}
