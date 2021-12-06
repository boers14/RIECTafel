using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SettingsManager
{
    public enum AffectedSetting
    {
        HandControls,
        RayKeyBoard,
        MoveMapFactor,
        RotateMapFactor,
        ScaleMapFactor,
        MoveGrabbedObjectFactor,
        ScaleGrabbedObjectFactor
    }

    public static bool oneHandControls = true, rayKeyBoard = true;

    public static float moveMapSpeedFactor = 1f, scaleMapFactor = 1f, rotateMapFactor = 1f, moveGrabbedObjectSpeedFactor = 1f, 
        scaleGrabbedObjectFactor = 1f;

    public static void ChangeHandControls(bool oneHandsControl, PlayerGrab[] hands)
    {
        oneHandControls = oneHandsControl;
        for (int i = 0; i < hands.Length; i++)
        {
            hands[i].oneButtonControl = oneHandControls;
        }
    }
}
