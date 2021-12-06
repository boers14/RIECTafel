using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneSettingsButton : MeetingButton
{
    private MiniMap miniMap = null;

    public override void Start()
    {
        base.Start();
        miniMap = FindObjectOfType<MiniMap>();
        button.onClick.AddListener(EnabledMiniMap);
    }
    
    private void EnabledMiniMap()
    {
        miniMap.gameObject.SetActive(!miniMap.gameObject.activeSelf);
    }
}
