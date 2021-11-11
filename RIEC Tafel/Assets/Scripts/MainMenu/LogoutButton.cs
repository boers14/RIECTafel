using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoutButton : SwitchSceneButton
{
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
