using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelAvatarCreationButton : TurnOnAvatarCreation
{
    private List<AvatarChangebleBodypart> bodyparts = new List<AvatarChangebleBodypart>();

    /// <summary>
    /// Turn on register UI, turn off avatar creation UI
    /// </summary>

    public override void Start()
    {
        base.Start();
        bodyparts.AddRange(FindObjectsOfType<AvatarChangebleBodypart>());
    }

    /// <summary>
    /// Reset avatar to state in which it was before opening avtar creation UI
    /// </summary>

    public override void SwitchAvatarActiveState()
    {
        base.SwitchAvatarActiveState();
        for (int i = 0; i < bodyparts.Count; i++)
        {
            bodyparts[i].CancelAvatarCreation();
        }
    }
}
