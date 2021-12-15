using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StopConnectButton : SwitchSceneButton
{
    [SerializeField]
    private NetworkManagerRIECTafel networkManager = null;

    private void FixedUpdate()
    {
        if (!NetworkClient.active && networkManager.hasFoundDiscussion)
        {
            SwitchScene();
        }
    }

    public override void SwitchScene()
    {
        switch (ConnectionManager.connectFunction)
        {
            case ConnectionManager.ConnectFunction.ServerClient:
                if (NetworkServer.active && NetworkClient.isConnected)
                {
                    networkManager.StopHost();
                }
                break;
            case ConnectionManager.ConnectFunction.Server:
                if (NetworkServer.active)
                {
                    networkManager.StopServer();
                }
                break;
            case ConnectionManager.ConnectFunction.Client:
                if (NetworkClient.isConnected)
                {
                    networkManager.StopClient();
                }
                break;
        }

        base.SwitchScene();
    }
}
