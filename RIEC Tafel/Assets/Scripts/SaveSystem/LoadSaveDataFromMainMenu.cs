using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSaveDataFromMainMenu : MonoBehaviour
{
    /// <summary>
    /// Load saved data from main menu
    /// </summary>

    private void Start()
    {
        SettingsManager.LoadData(SaveSytem.LoadGame(), FindObjectOfType<KeyBoard>());
    }
}
