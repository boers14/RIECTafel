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

    public static void ResetStats()
    {
        moveMapSpeedFactor = 1f;
        scaleMapFactor = 1f;
        rotateMapFactor = 1f;
        moveGrabbedObjectSpeedFactor = 1f;
        scaleGrabbedObjectFactor = 1f;

        oneHandControls = true;
        rayKeyBoard = true;
    }

    public static void LoadData(PlayerData data, KeyBoard keyBoard)
    {
        moveMapSpeedFactor = data.moveMapSpeedFactor;
        scaleMapFactor = data.scaleMapFactor;
        rotateMapFactor = data.rotateMapFactor;
        moveGrabbedObjectSpeedFactor = data.moveGrabbedObjectSpeedFactor;
        scaleGrabbedObjectFactor = data.scaleGrabbedObjectFactor;

        oneHandControls = data.oneHandControls;
        rayKeyBoard = data.rayKeyBoard;
        ChangeKeyBoardState(rayKeyBoard, keyBoard);
    }

    public static void ChangeHandControls(bool oneHandsControl, PlayerGrab[] hands, PlayerHandRays[] handRays)
    {
        oneHandControls = oneHandsControl;
        for (int i = 0; i < hands.Length; i++)
        {
            hands[i].oneButtonControl = oneHandControls;
        }

        if (oneHandControls)
        {
            for (int i = 0; i < handRays.Length; i++)
            {
                handRays[i].ChangeColorGradientOfRayIfHandRayIsHittingRegisteredObject(1);
            }
        } else
        {
            for (int i = 0; i < handRays.Length; i++)
            {
                handRays[i].ChangeColorGradientOfRayIfHandRayIsHittingRegisteredObject(0);
            }
        }
    }

    public static void ChangeKeyBoardState(bool rayKeyBoardIsOn, KeyBoard keyBoard)
    {
        rayKeyBoard = rayKeyBoardIsOn;
        keyBoard.SetSelectedKeyBoardPos();
    }
}
