using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TutorialPOIText : POIText
{
    [System.NonSerialized]
    public bool hasBeenExpanded = false, hasBeenPulled = false;

    private TutorialManager tutorialManager = null;

    public override void Start()
    {
        tutorialManager = FindObjectOfType<TutorialManager>();
        base.Start();
    }

    public override void PullPOIToPlayer(int index)
    {
        base.PullPOIToPlayer(index);
        if (tutorialManager.checkForPOIPull)
        {
            hasBeenPulled = true;
        }
    }

    public override void ExpandText(HoverEnterEventArgs args)
    {
        base.ExpandText(args);
        if (tutorialManager.checkForPOIOpen)
        {
            hasBeenExpanded = true;
        }
    }
}
