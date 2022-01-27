using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneSettingsButton : MeetingButton
{
    [SerializeField]
    private MiniMap miniMap = null;

    [SerializeField]
    private bool checkMiniMapEnabledStateOnEnable = false;

    /// <summary>
    /// (de)Activates minimap when the button is pressed or enabled
    /// </summary>

    public override void ActivateMeetingSet()
    {
        base.ActivateMeetingSet();
        CheckMiniMapActiveState();
    }

    private void OnEnable()
    {
        if (checkMiniMapEnabledStateOnEnable && button)
        {
            CheckMiniMapActiveState();
        }
    }

    private void CheckMiniMapActiveState()
    {
        if (objectToAcivate)
        {
            miniMap.gameObject.SetActive(!objectToAcivate.activeSelf);
        }
        else
        {
            miniMap.gameObject.SetActive(!objectToDeactivate.activeSelf);
        }
    }
}
