using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMoveMap : MoveMap
{
    [SerializeField]
    private TutorialManager tutorialManager = null;

    /// <summary>
    /// For when user has to hold a primary button to go to the next step and is doing the controller controls tutorial
    /// </summary>

    public override void ChangeMapScale(float changedScale)
    {
        if (tutorialManager.cantZoomMap) { return; }
        base.ChangeMapScale(changedScale);
    }
}
