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

    [System.NonSerialized]
    public bool hasFoundDiscussion = false;

    /// <summary>
    /// Start hosting if the connection manager is a server type else start searching for a discussion
    /// </summary>

    public override void Start()
    {
        base.Start();
        cityName = ConnectionManager.cityName;

        NetworkDiscovery networkDiscovery = GetComponent<NetworkDiscovery>();

        switch (ConnectionManager.connectFunction)
        {
            case ConnectionManager.ConnectFunction.ServerClient:
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    StartHost();
                    // Make the server visible for other clients by advertising it on the network
                    GetComponent<NetworkDiscovery>().AdvertiseServer();
                    hasFoundDiscussion = true;
                }
                break;
            case ConnectionManager.ConnectFunction.Server:
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    StartServer();
                    hasFoundDiscussion = true;
                }
                break;
            case ConnectionManager.ConnectFunction.Client:
                networkDiscovery.enableActiveDiscovery = true;
                networkDiscovery.OnServerFound.AddListener(DisableNetworkDiscoveryOnServerFound);
                networkDiscovery.StartDiscovery();
                break;
        }
    }

    /// <summary>
    /// Start the client if the player found a server and stop searching for a server
    /// </summary>

    private void DisableNetworkDiscoveryOnServerFound(ServerResponse response)
    {
        GetComponent<NetworkDiscovery>().StopDiscovery();
        StartClient(response.uri);
        hasFoundDiscussion = true;
    }

    /// <summary>
    /// Spawn a player on the network and if there is no game manager yet als spawn a game manager
    /// </summary>

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // This isnt actually always equal to the amount of players in the discussion, it is used to give each player connection
        // a unique player ID
        numberOfPlayers++;

        GameObject player = Instantiate(playerPrefab, spawnPos.position, spawnPos.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);
        playerConnectedToNumber.Add(conn, numberOfPlayers);

        if (GameObject.FindGameObjectWithTag("GameManager") == null)
        {
            gameManager = Instantiate(gameManagerPrefab);
            NetworkServer.Spawn(gameManager.gameObject);
            gameManager.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
            // The start of retrieving city data for the game manager
            StartCoroutine(player.GetComponent<PlayerConnection>().StartConnectionWithGamemanager(cityName));
        }
    }

    /// <summary>
    /// Refill the map owner dropdown with all actual players in the discussion and if there a players still selecting a seat
    /// Open up the seat of the player that left for those players
    /// </summary>

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);

        PlayerConnection connection = FindObjectOfType<PlayerConnection>();
        PlayerConnection ownConnection = connection.FetchOwnPlayer();

        if (ownConnection)
        {
            if (ownConnection.isServer)
            {
                ownConnection.CmdRefillPlayerMapControlDropdown();
            }
        }

        if (!ownConnection || ownConnection.isServer || playerConnectedToNumber.Count == 1) { return; }

        ownConnection.CmdCheckIfSeatsOpenedUp(playerConnectedToNumber[conn]);
        playerConnectedToNumber.Remove(conn);
    }
}
