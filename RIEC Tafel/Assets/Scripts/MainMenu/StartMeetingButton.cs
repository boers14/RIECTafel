using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMeetingButton : MeetingButton
{
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