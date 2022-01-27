using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMeetingButton : MeetingButton
{
    /// <summary>
    /// Is only active when user is known to work at iCOV
    /// </summary>

    public override void Start()
    {
        if (!LogInManager.iCOVWorker)
        {
            gameObject.SetActive(false);
            return;
        }

        base.Start();
    }
}