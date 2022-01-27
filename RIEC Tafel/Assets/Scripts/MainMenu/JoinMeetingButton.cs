using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinMeetingButton : MeetingButton
{
    [SerializeField]
    private RectTransform startMeetingButton = null;

    [SerializeField]
    private List<RectTransform> otherButtons = new List<RectTransform>();

    /// <summary>
    /// Move the button to start meeting button pos and move al the other buttons up with it
    /// </summary>

    public override void Start()
    {
        base.Start();
        if (!LogInManager.iCOVWorker)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            float difInYPos = startMeetingButton.localPosition.y - rectTransform.localPosition.y;
            rectTransform.localPosition = startMeetingButton.localPosition;

            for (int i = 0; i < otherButtons.Count; i++)
            {
                Vector3 newPos = otherButtons[i].localPosition;
                newPos.y += difInYPos;
                otherButtons[i].localPosition = newPos;
            }
        }
    }
}
