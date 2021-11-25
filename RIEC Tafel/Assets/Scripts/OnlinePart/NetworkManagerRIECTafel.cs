using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerRIECTafel : NetworkManager
{
    [SerializeField]
    private Transform spawnPos = null;

    [SerializeField]
    private GameManager gameManagerPrefab = null;

    private GameManager gameManager = null;

    [System.NonSerialized]
    public string cityName = "Hilversum";

    [System.NonSerialized]
    public int numberOfPlayers = 0;

    public override void Start()
    {
        base.Start();
        cityName = ConnectionManager.cityName;
        switch (ConnectionManager.connectFunction)
        {
            case ConnectionManager.ConnectFunction.ServerClient:
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    StartHost();
                }
                break;
            case ConnectionManager.ConnectFunction.Server:
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    StartServer();
                }
                break;
            case ConnectionManager.ConnectFunction.Client:
                StartClient();
                break;
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        numberOfPlayers++;

        GameObject player = Instantiate(playerPrefab, spawnPos.position, spawnPos.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);

        if (GameObject.FindGameObjectWithTag("GameManager") == null)
        {
            gameManager = Instantiate(gameManagerPrefab);
            NetworkServer.Spawn(gameManager.gameObject);
            gameManager.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
            StartCoroutine(player.GetComponent<PlayerConnection>().StartConnectionWithGamemanager(cityName));
        }
    }
}
