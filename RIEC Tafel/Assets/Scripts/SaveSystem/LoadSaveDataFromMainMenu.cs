using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSaveDataFromMainMenu : MonoBehaviour
{
    private void Start()
    {
        SettingsManager.LoadData(SaveSytem.LoadGame(), FindObjectOfType<KeyBoard>());
    }
}
