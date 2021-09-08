using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class StopConnectButton : MonoBehaviour
{
    private ConnectButton.ButtonFunction currentFunction = ConnectButton.ButtonFunction.None;

    [SerializeField]
    private List<ConnectButton> connectButtons = new List<ConnectButton>();

    [SerializeField]
    private Text clientConnectionStatus = null;

    [SerializeField]
    private NetworkManager networkManager = null;

    private Image buttonVisual = null;
    private Text buttonText = null;

    private void Start()
    {
        buttonVisual = GetComponent<Image>();
        buttonText = GetComponentInChildren<Text>();
        EnableConnectButtons(true, true);

        GetComponent<Button>().onClick.AddListener(StopOnlineConnection);
    }

    private void FixedUpdate()
    {
        if (buttonVisual.enabled)
        {
            switch (currentFunction)
            {
                case ConnectButton.ButtonFunction.ServerClient:
                    clientConnectionStatus.text = "You're the server and client";
                    if (!NetworkServer.active || !NetworkClient.isConnected)
                    {
                        clientConnectionStatus.text = "Click a button to start";
                        EnableConnectButtons(true, true);
                    }
                    break;
                case ConnectButton.ButtonFunction.Server:
                    clientConnectionStatus.text = "You're the server";
                    if (!NetworkServer.active)
                    {
                        clientConnectionStatus.text = "Click a button to start";
                        EnableConnectButtons(true, true);
                    }
                    break;
                case ConnectButton.ButtonFunction.Client:
                    clientConnectionStatus.text = "Connected to a server";
                    if (!NetworkClient.active)
                    {
                        clientConnectionStatus.text = "Click a button to start";
                        EnableConnectButtons(true, true);
                    }
                    break;
                case ConnectButton.ButtonFunction.None:
                    print("Forgot to set button function!");
                    break;
            }
        } else
        {
            if (NetworkServer.active || NetworkClient.active)
            {
                if (!NetworkClient.isConnecting)
                {
                    EnableConnectButtons(false, true);
                }
            } else if (!NetworkClient.active && !connectButtons[0].gameObject.activeSelf)
            {
                clientConnectionStatus.text = "Click a button to start";
                EnableConnectButtons(true, true);
            }
        }
    }

    public void SetButtonFunction(ConnectButton.ButtonFunction currentFunction, bool affectsButton)
    {
        this.currentFunction = currentFunction;
        EnableConnectButtons(false, affectsButton);

        if (currentFunction == ConnectButton.ButtonFunction.Client)
        {
            clientConnectionStatus.text = "Trying to connect to server";
        }
    }

    private void StopOnlineConnection()
    {
        switch (currentFunction)
        {
            case ConnectButton.ButtonFunction.ServerClient:
                if (NetworkServer.active && NetworkClient.isConnected)
                {
                    networkManager.StopHost();
                }
                break;
            case ConnectButton.ButtonFunction.Server:
                if (NetworkServer.active)
                {
                    networkManager.StopServer();
                }
                break;
            case ConnectButton.ButtonFunction.Client:
                if (NetworkClient.isConnected)
                {
                    networkManager.StopClient();
                }
                break;
            case ConnectButton.ButtonFunction.None:
                print("Forgot to set button function!");
                break;
        }
    }

    private void EnableConnectButtons(bool enabled, bool affectsButton)
    {
        if (affectsButton)
        {
            buttonVisual.enabled = !enabled;
            buttonText.enabled = !enabled;
        }

        for (int i = 0; i < connectButtons.Count; i++)
        {
            connectButtons[i].gameObject.SetActive(enabled);
        }
    }
}
