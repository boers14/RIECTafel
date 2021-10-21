using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectButton : SwitchSceneButton
{
    [SerializeField]
    private ConnectionManager.ConnectFunction buttonFunction = 0;

    public override void SwitchScene()
    {
        ConnectionManager.connectFunction = buttonFunction;
        base.SwitchScene();
    }
}
