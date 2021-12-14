using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsToggle : MonoBehaviour
{
    [SerializeField]
    private KeyBoard keyBoard = null;

    private PlayerGrab[] hands = null;

    private PlayerHandRays[] handRays = null;

    [SerializeField]
    private SettingsManager.AffectedSetting affectedSetting = 0;

    private void Start()
    {
        hands = FindObjectsOfType<PlayerGrab>();
        handRays = FindObjectsOfType<PlayerHandRays>();
        GetComponent<Toggle>().onValueChanged.AddListener(SwitchSettings);

        SwitchToggleSetting();
    }

    public void SwitchToggleSetting()
    {
        bool isOn = false;
        switch (affectedSetting)
        {
            case SettingsManager.AffectedSetting.HandControls:
                isOn = SettingsManager.oneHandControls;
                break;
            case SettingsManager.AffectedSetting.RayKeyBoard:
                isOn = SettingsManager.rayKeyBoard;
                break;
        }

        GetComponent<Toggle>().isOn = isOn;
    }

    private void SwitchSettings(bool isOn)
    {
        switch (affectedSetting)
        {
            case SettingsManager.AffectedSetting.HandControls:
                SettingsManager.ChangeHandControls(isOn, hands, handRays);
                break;
            case SettingsManager.AffectedSetting.RayKeyBoard:
                SettingsManager.ChangeKeyBoardState(isOn, keyBoard);
                break;
        }
    }
}
