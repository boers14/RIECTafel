using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelAvatarCreationButton : TurnOnAvatarCreation
{
    private List<AvatarChangebleBodypart> bodyparts = new List<AvatarChangebleBodypart>();

    public override void Start()
    {
        base.Start();
        bodyparts.AddRange(FindObjectsOfType<AvatarChangebleBodypart>());
    }

    public override void SwitchAvatarActiveState()
    {
        base.SwitchAvatarActiveState();
        for (int i = 0; i < bodyparts.Count; i++)
        {
            bodyparts[i].CancelAvatarCreation();
        }
    }
}
