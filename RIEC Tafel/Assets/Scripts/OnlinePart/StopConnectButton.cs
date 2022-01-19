using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StopConnectButton : SwitchSceneButton
{
    [SerializeField]
    private NetworkManagerRIECTafel networkManager = null;

    private void FixedUpdate()
    {
        //if (!NetworkClient.active && networkManager.hasFoundDiscussion)
        //{
        //    SwitchScene();
        //}
    }

    public override void SwitchScene()
    {
        switch (ConnectionManager.connectFunction)
        {
            case ConnectionManager.ConnectFunction.Host:

                break;
            case ConnectionManager.ConnectFunction.Join:

                break;
        }

        PhotonNetwork.Disconnect();
        NotesSaver.instance.SetTextToBeSaved();
        base.SwitchScene();
    }
}
