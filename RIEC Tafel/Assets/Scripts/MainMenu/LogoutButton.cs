using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoutButton : SwitchSceneButton
{
    /// <summary>
    /// Log user out if this button is pressed or if the user quits the application in the main menu
    /// </summary>

    public override void SwitchScene()
    {
        base.SwitchScene();
        LogInManager.LogOut(this);
    }

    private void OnApplicationQuit()
    {
        LogInManager.LogOut(this);
    }
}
