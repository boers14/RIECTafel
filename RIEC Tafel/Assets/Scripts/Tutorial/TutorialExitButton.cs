using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialExitButton : SwitchSceneButton
{
    /// <summary>
    /// Scene to switch to changes to what the TutorialSceneManager holds
    /// </summary>

    public override void Start()
    {
        base.Start();
        sceneToSwitchTo = TutorialSceneManager.sceneToSwitchTo;
    }
}
