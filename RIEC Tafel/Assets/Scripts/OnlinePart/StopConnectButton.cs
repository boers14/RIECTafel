using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StopConnectButton : SwitchSceneButton
{
    [SerializeField]
    private NetworkManagerRIECTafel networkManager = null;

    /// <summary>
    /// Disconnetct the player from the discussion scene if the player is in one and the clint isnt active anymore
    /// </summary>

    private void FixedUpdate()
    {
        if (!NetworkClient.active && networkManager.hasFoundDiscussion)
        {
            SwitchScene();
        }
    }

    /// <summary>
    /// Stop the required function and save the notes to the database
    /// </summary>

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

        NotesSaver.instance.SetTextToBeSaved();
        base.SwitchScene();
    }
}
