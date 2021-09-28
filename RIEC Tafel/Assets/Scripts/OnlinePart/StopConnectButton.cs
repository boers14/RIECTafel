using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class StopConnectButton : MonoBehaviour
{
    private ConnectButton.ButtonFunction currentFunction = ConnectButton.ButtonFunction.None;

    [SerializeField]
    private List<GameObject> connectMenuButtonsButtons = new List<GameObject>(), discussionButtons = new List<GameObject>();

    [SerializeField]
    private TMP_Text clientConnectionStatus = null;

    [SerializeField]
    private NetworkManager networkManager = null;

    private Image buttonVisual = null;
    private TMP_Text buttonText = null;

    private void Start()
    {
        buttonVisual = GetComponent<Image>();
        buttonText = GetComponentInChildren<TMP_Text>();
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
                    clientConnectionStatus.text = "Je bent de server en een client";
                    if (!NetworkServer.active || !NetworkClient.isConnected)
                    {
                        clientConnectionStatus.text = "Druk op een knop om te starten";
                        EnableConnectButtons(true, true);
                    }
                    break;
                case ConnectButton.ButtonFunction.Server:
                    clientConnectionStatus.text = "Je bent de server";
                    if (!NetworkServer.active)
                    {
                        clientConnectionStatus.text = "Druk op een knop om te starten";
                        EnableConnectButtons(true, true);
                    }
                    break;
                case ConnectButton.ButtonFunction.Client:
                    clientConnectionStatus.text = "Je hebt connectie met een server";
                    if (!NetworkClient.active)
                    {
                        clientConnectionStatus.text = "Druk op een knop om te starten";
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
            } else if (!NetworkClient.active && !connectMenuButtonsButtons[0].activeSelf)
            {
                clientConnectionStatus.text = "Druk op een knop om te starten";
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
            clientConnectionStatus.text = "Probeer connectie te krijgen met een server";
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

        for (int i = 0; i < discussionButtons.Count; i++)
        {
            discussionButtons[i].SetActive(!enabled);
        }

        for (int i = 0; i < connectMenuButtonsButtons.Count; i++)
        {
            connectMenuButtonsButtons[i].SetActive(enabled);
        }
    }
}
