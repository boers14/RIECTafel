using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerRIECTafel : NetworkManager
{
    [SerializeField]
    private List<Transform> spawnPosses = new List<Transform>();

    private Dictionary<NetworkConnection, Transform> clientSeatConnection = new Dictionary<NetworkConnection, Transform>();

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

        Transform start = null;

        for (int i = 0; i < spawnPosses.Count; i++)
        {
            if (!clientSeatConnection.ContainsValue(spawnPosses[i]))
            {
                start = spawnPosses[i];
                clientSeatConnection.Add(conn, spawnPosses[i]);
                break;
            }
        }

        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);

        if (GameObject.FindGameObjectWithTag("GameManager") == null)
        {
            gameManager = Instantiate(gameManagerPrefab);
            NetworkServer.Spawn(gameManager.gameObject);
            gameManager.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
            StartCoroutine(player.GetComponent<PlayerConnection>().StartConnectionWithGamemanager(cityName));
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        clientSeatConnection.Remove(conn);
        base.OnServerDisconnect(conn);
    }
}
