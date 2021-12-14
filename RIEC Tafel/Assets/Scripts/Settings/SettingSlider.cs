using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingSlider : MonoBehaviour
{
    private Slider slider = null;

    [SerializeField]
    private SettingsManager.AffectedSetting affectedSetting = 0;

    [SerializeField]
    private float maxExtraPercentage = 2;

    [SerializeField]
    private TMP_Text valueText = null;

    private float baseValue = 0;

    private void Start()
    {
        slider = GetComponent<Slider>();
        baseValue = slider.maxValue / 2;
        slider.onValueChanged.AddListener(ChangeAffectedSetting);

        ReverseEngineerSetting();
    }

    public void ReverseEngineerSetting()
    {
        float currentValue = 0;
        switch (affectedSetting)
        {
            case SettingsManager.AffectedSetting.MoveMapFactor:
                currentValue = SettingsManager.moveMapSpeedFactor;
                break;
            case SettingsManager.AffectedSetting.RotateMapFactor:
                currentValue = SettingsManager.rotateMapFactor;
                break;
            case SettingsManager.AffectedSetting.ScaleMapFactor:
                currentValue = SettingsManager.scaleMapFactor;
                break;
            case SettingsManager.AffectedSetting.MoveGrabbedObjectFactor:
                currentValue = SettingsManager.moveGrabbedObjectSpeedFactor;
                break;
            case SettingsManager.AffectedSetting.ScaleGrabbedObjectFactor:
                currentValue = SettingsManager.scaleGrabbedObjectFactor;
                break;
        }

        float maxValue = 0;
        float minValue = 0;
        bool isOverHundredPercent = false;

        if (currentValue <= 1)
        {
            maxValue = baseValue;
            minValue = 0;
            currentValue = (currentValue - (1 / maxExtraPercentage)) / (1 / maxExtraPercentage);
        }
        else
        {
            maxValue = slider.maxValue;
            minValue = baseValue;
            currentValue = 1 + ((currentValue - 1) - (maxExtraPercentage - 1) / (maxExtraPercentage - 1));
            isOverHundredPercent = true;
        }

        float difference = maxValue - minValue;
        float currentSettingValue = minValue + difference * currentValue;
        if (isOverHundredPercent)
        {
            slider.value = baseValue + (currentValue * baseValue);
        }
        else
        {
            slider.value = currentSettingValue;
        }
    }

    private void ChangeAffectedSetting(float value)
    {
        float maxValue = 0;
        float minValue = 0;
        float actualValue = 0;
        bool isOverHunderdPercent = false;

        if (value <= baseValue)
        {
            maxValue = 1;
            minValue = 1 / maxExtraPercentage;
            actualValue = value / baseValue;
        } else
        {
            maxValue = maxExtraPercentage;
            minValue = 1;
            actualValue = (value - baseValue) / baseValue;
            isOverHunderdPercent = true;
        }

        float difference = maxValue - minValue;
        float addedValue = difference * (actualValue / maxValue);
        if (isOverHunderdPercent)
        {
            addedValue *= maxExtraPercentage;
        }

        float newSettingValue = minValue + addedValue;
        valueText.text = Mathf.Round(newSettingValue * 100) + "%";

        switch (affectedSetting)
        {
            case SettingsManager.AffectedSetting.MoveMapFactor:
                SettingsManager.moveMapSpeedFactor = newSettingValue;
                break;
            case SettingsManager.AffectedSetting.RotateMapFactor:
                SettingsManager.rotateMapFactor = newSettingValue;
                break;
            case SettingsManager.AffectedSetting.ScaleMapFactor:
                SettingsManager.scaleMapFactor = newSettingValue;
                break;
            case SettingsManager.AffectedSetting.MoveGrabbedObjectFactor:
                SettingsManager.moveGrabbedObjectSpeedFactor = newSettingValue;
                break;
            case SettingsManager.AffectedSetting.ScaleGrabbedObjectFactor:
                SettingsManager.scaleGrabbedObjectFactor = newSettingValue;
                break;
        }
    }
}
