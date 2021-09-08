using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ConnectButton : MonoBehaviour
{
    [SerializeField]
    private NetworkManager networkManager = null;

    public enum ButtonFunction
    {
        ServerClient,
        Server,
        Client,
        None
    }

    [SerializeField]
    private ButtonFunction buttonFunction = ButtonFunction.None;

    [SerializeField]
    private StopConnectButton stopConnectButton = null;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(PerformConnectFunction);
    }

    private void PerformConnectFunction()
    {
        switch (buttonFunction)
        {
            case ButtonFunction.ServerClient:
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    networkManager.StartHost();
                }
                break;
            case ButtonFunction.Server:
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    networkManager.StartServer();
                }
                break;
            case ButtonFunction.Client:
                networkManager.StartClient();
                break;
            case ButtonFunction.None:
                print("Forgot to set button function!");
                break;
        }

        stopConnectButton.SetButtonFunction(buttonFunction, false);
    }
}
