using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnAvatarCreation : MeetingButton
{
    [SerializeField]
    private GameObject avatar = null;

    /// <summary>
    /// Turn off register UI to turn on Avatar creation UI.
    /// </summary>

    public override void Start()
    {
        base.Start();
        button.onClick.AddListener(SwitchAvatarActiveState);
    }

    /// <summary>
    /// Turn on/ off example avatar
    /// </summary>

    public virtual void SwitchAvatarActiveState()
    {
        avatar.SetActive(!avatar.activeSelf);
    }
}
