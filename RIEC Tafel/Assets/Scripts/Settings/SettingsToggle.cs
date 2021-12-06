using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsToggle : MonoBehaviour
{
    private PlayerGrab[] hands = null;

    [SerializeField]
    private SettingsManager.AffectedSetting affectedSetting = 0;

    private void Start()
    {
        bool isOn = false;
        switch(affectedSetting)
        {
            case SettingsManager.AffectedSetting.HandControls:
                isOn = SettingsManager.oneHandControls;
                break;
            case SettingsManager.AffectedSetting.RayKeyBoard:
                isOn = SettingsManager.rayKeyBoard;
                break;
        }

        GetComponent<Toggle>().isOn = isOn;
        GetComponent<Toggle>().onValueChanged.AddListener(SwitchSettings);
        hands = FindObjectsOfType<PlayerGrab>();
    }

    private void SwitchSettings(bool isOn)
    {
        switch (affectedSetting)
        {
            case SettingsManager.AffectedSetting.HandControls:
                SettingsManager.ChangeHandControls(isOn, hands);
                break;
            case SettingsManager.AffectedSetting.RayKeyBoard:
                SettingsManager.ChangeHandControls(isOn, hands);
                break;
        }
    }
}
