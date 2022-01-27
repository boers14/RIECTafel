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

    /// <summary>
    /// Set the slider to value gotten from the Setting manager
    /// </summary>

    private void Start()
    {
        slider = GetComponent<Slider>();
        baseValue = slider.maxValue / 2;
        slider.onValueChanged.AddListener(ChangeAffectedSetting);

        ReverseEngineerSetting();
    }

    /// <summary>
    /// Calculate the percentage progress on the bar of the slider based on the setting given in the setting manager
    /// </summary>

    public void ReverseEngineerSetting()
    {
        // Get value from setting manager based on which slider it is
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

        // Based on whether the value is lower then the standard value (1) or higher calculate the slider value
        bool isOverHundredPercent = false;

        // Calculate what the current value would be percentually to the max value it could be
        if (currentValue <= 1)
        {
            currentValue = (currentValue - (1 / maxExtraPercentage)) / (1 - (1 / maxExtraPercentage));
        }
        else
        {
            currentValue = (1 + ((currentValue - 1) - (maxExtraPercentage - 1) / (maxExtraPercentage - 1))) / (maxExtraPercentage - 1);
            isOverHundredPercent = true;
        }

        if (isOverHundredPercent)
        {
            // Calculate the value where the min value of the slider has to be over 50%
            slider.value = baseValue + (currentValue * baseValue);
        }
        else
        {
            // Calculate the value where the max value of the slider is 50%
            slider.value = baseValue * currentValue;
        }
    }

    /// <summary>
    /// Calculate value of the setting based on how far the value is on the bar
    /// </summary>

    private void ChangeAffectedSetting(float value)
    {
        float maxValue = 0;
        float minValue = 0;
        float actualValue = 0;
        bool isOverHunderdPercent = false;

        // Get min and max values. Actual value being the percentage from 50% of the slider.
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

        // Calculate the maximal added value
        float difference = maxValue - minValue;
        // Percentage based calculation of what the value would be
        float addedValue = difference * (actualValue / maxValue);
        if (isOverHunderdPercent)
        {
            addedValue *= maxExtraPercentage;
        }

        float newSettingValue = minValue + addedValue;

        // Set new value based on type
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
