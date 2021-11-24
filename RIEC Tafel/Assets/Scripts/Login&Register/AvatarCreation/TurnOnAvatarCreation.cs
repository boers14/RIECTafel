using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnAvatarCreation : MeetingButton
{
    [SerializeField]
    private GameObject avatar = null;

    public override void Start()
    {
        base.Start();
        button.onClick.AddListener(SwitchAvatarActiveState);
    }

    public virtual void SwitchAvatarActiveState()
    {
        avatar.SetActive(!avatar.activeSelf);
    }
}
