using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetSettingsButton : MonoBehaviour
{
    [SerializeField]
    private List<SettingSlider> settingSliders = new List<SettingSlider>();

    [SerializeField]
    private List<SettingsToggle> settingsToggles = new List<SettingsToggle>();

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(ResetSettings);
    }

    private void ResetSettings()
    {
        SettingsManager.ResetStats();

        for (int i = 0; i < settingSliders.Count; i++)
        {
            settingSliders[i].ReverseEngineerSetting();
        }

        for (int i = 0; i < settingsToggles.Count; i++)
        {
            settingsToggles[i].SwitchToggleSetting();
        }
    }
}
