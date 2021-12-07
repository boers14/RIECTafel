using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneSettingsButton : MeetingButton
{
    [SerializeField]
    private MiniMap miniMap = null;

    public override void Start()
    {
        base.Start();
        button.onClick.AddListener(EnabledMiniMap);
    }
    
    private void EnabledMiniMap()
    {
        miniMap.gameObject.SetActive(!miniMap.gameObject.activeSelf);
    }
}
