using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialExitButton : SwitchSceneButton
{
    public override void Start()
    {
        base.Start();
        sceneToSwitchTo = TutorialSceneManager.sceneToSwitchTo;
    }
}
