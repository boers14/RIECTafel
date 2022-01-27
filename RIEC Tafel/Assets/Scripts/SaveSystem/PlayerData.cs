using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    /// <summary>
    /// Save all data that was set in the settings manager. Only basic data types can be saved.
    /// </summary>

    public float moveMapSpeedFactor = 1f, scaleMapFactor = 1f, rotateMapFactor = 1f, moveGrabbedObjectSpeedFactor = 1f,
        scaleGrabbedObjectFactor = 1f;

    public bool oneHandControls = true, rayKeyBoard = true;

    public PlayerData()
    {
        moveMapSpeedFactor = SettingsManager.moveMapSpeedFactor;
        scaleMapFactor = SettingsManager.scaleMapFactor;
        rotateMapFactor = SettingsManager.rotateMapFactor;
        moveGrabbedObjectSpeedFactor = SettingsManager.moveGrabbedObjectSpeedFactor;
        scaleGrabbedObjectFactor = SettingsManager.scaleGrabbedObjectFactor;

        oneHandControls = SettingsManager.oneHandControls;
        rayKeyBoard = SettingsManager.rayKeyBoard;
    }
}
