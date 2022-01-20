using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManagerRIECTafel : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Transform spawnPos = null, chooseDiscussionUI = null, choosSeatUI = null;

    [SerializeField]
    private GameManager gameManagerPrefab = null;

    private GameManager gameManager = null;

    [System.NonSerialized]
    public string cityName = "Hilversum";

    [SerializeField]
    private int maxAmountOfPlayers = 6;

    public int numberOfPlayers { get; set; } = 0;

    public GameObject acceptPlayerToDiscussionUI = null;

    //private Dictionary<NetworkConnection, int> playerConnectedToNumber = new Dictionary<NetworkConnection, int>();

    [System.NonSerialized]
    public bool hasFoundDiscussion = false;

    public void Start()
    {
        cityName = ConnectionManager.cityName;
        print("start searching");
        PhotonNetwork.NickName = LogInManager.username;
        PhotonNetwork.GameVersion = "0.0.1";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print("is connected!");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        print("joined lobby");
        switch (ConnectionManager.connectFunction)
        {
            case ConnectionManager.ConnectFunction.Host:
                RoomOptions options = new RoomOptions();
                options.MaxPlayers = (byte)maxAmountOfPlayers;
                PhotonNetwork.CreateRoom(ConnectionManager.roomName, options, TypedLobby.Default);
                break;
            case ConnectionManager.ConnectFunction.Join:
                chooseDiscussionUI.gameObject.SetActive(true);
                choosSeatUI.gameObject.SetActive(false);
                break;
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        hasFoundDiscussion = true;
        switch (ConnectionManager.connectFunction)
        {
            case ConnectionManager.ConnectFunction.Host:
                break;
            case ConnectionManager.ConnectFunction.Join:
                chooseDiscussionUI.gameObject.SetActive(false);
                choosSeatUI.gameObject.SetActive(true);
                break;
        }

        // Set playernumber, playerIsInControlOfMap, seatIndex, dataType, playerName, avatarData, controlledplayer of all players
        // for joined player

        print("Player joined room");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        print("disconnected! Reason: " + cause);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        print("Room created");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        print("Room created failed. Reason: " + message);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        print("player entered room");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        print("player left room");
    }



    //public override void OnServerAddPlayer(NetworkConnection conn)
    //{
    //    numberOfPlayers++;

    //    GameObject player = Instantiate(playerPrefab, spawnPos.position, spawnPos.rotation);
    //    NetworkServer.AddPlayerForConnection(conn, player);
    //    playerConnectedToNumber.Add(conn, numberOfPlayers);

    //    if (GameObject.FindGameObjectWithTag("GameManager") == null)
    //    {
    //        gameManager = Instantiate(gameManagerPrefab);
    //        NetworkServer.Spawn(gameManager.gameObject);
    //        gameManager.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
    //        StartCoroutine(player.GetComponent<PlayerConnection>().StartConnectionWithGamemanager(cityName));
    //    }
    //}

    //public override void OnServerDisconnect(NetworkConnection conn)
    //{
    //    base.OnServerDisconnect(conn);

    //    PlayerConnection connection = FindObjectOfType<PlayerConnection>();
    //    PlayerConnection ownConnection = connection.FetchOwnPlayer();
    //    if (!ownConnection || ownConnection.isServer || playerConnectedToNumber.Count == 1) { return; }

    //    ownConnection.CmdCheckIfSeatsOpenedUp(playerConnectedToNumber[conn]);
    //    playerConnectedToNumber.Remove(conn);
    //}
}
