using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;

public class NetworkManagerRIECTafel : NetworkManager
{
    [SerializeField]
    private Transform spawnPos = null;

    [SerializeField]
    private GameManager gameManagerPrefab = null;

    private GameManager gameManager = null;

    [System.NonSerialized]
    public string cityName = "Hilversum";

    public int numberOfPlayers { get; set; } = 0;

    public GameObject acceptPlayerToDiscussionUI = null;

    private Dictionary<NetworkConnection, int> playerConnectedToNumber = new Dictionary<NetworkConnection, int>();

    public override void Start()
    {
        base.Start();
        cityName = ConnectionManager.cityName;
        switch (ConnectionManager.connectFunction)
        {
            case ConnectionManager.ConnectFunction.ServerClient:
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    GetComponent<NetworkDiscovery>().AdvertiseServer();
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
                GetComponent<NetworkDiscovery>().enableActiveDiscovery = true;
                GetComponent<NetworkDiscovery>().OnServerFound.AddListener(DisableNetworkDiscoveryOnServerFound);
                break;
        }
    }

    private void DisableNetworkDiscoveryOnServerFound(ServerResponse response)
    {
        print("hi");
        GetComponent<NetworkDiscovery>().enabled = false;
        networkAddress = response.EndPoint.Address.ToString();
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        numberOfPlayers++;

        GameObject player = Instantiate(playerPrefab, spawnPos.position, spawnPos.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);
        playerConnectedToNumber.Add(conn, numberOfPlayers);

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
        base.OnServerDisconnect(conn);

        PlayerConnection connection = FindObjectOfType<PlayerConnection>();
        PlayerConnection ownConnection = connection.FetchOwnPlayer();
        if (!ownConnection || ownConnection.isServer || playerConnectedToNumber.Count == 1) { return; }

        ownConnection.CmdCheckIfSeatsOpenedUp(playerConnectedToNumber[conn]);
        playerConnectedToNumber.Remove(conn);
    }
}
