using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectButton : SwitchSceneButton
{
    [SerializeField]
    private ConnectionManager.ConnectFunction buttonFunction = 0;

    /// <summary>
    /// Sets the selected connect function equal to the connect function in the connection manager
    /// </summary>

    public override void Start()
    {
        base.Start();
        button.onClick.AddListener(SetConnectionFunction);
    }

    public virtual void SetConnectionFunction()
    {
        ConnectionManager.connectFunction = buttonFunction;
    }
}
