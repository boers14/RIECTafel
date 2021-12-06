using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMoveMap : MoveMap
{
    [SerializeField]
    private TutorialManager tutorialManager = null;

    public override void ChangeMapScale(float changedScale)
    {
        if (tutorialManager.cantZoomMap) { return; }
        base.ChangeMapScale(changedScale);
    }
}
