using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerConnection : NetworkBehaviour
{
    [SyncVar]
    private string playerName = "";

    [SerializeField]
    private TextMesh nameText = null;

    private GameManager gameManager = null;

    private VRPlayer player = null;

    [SyncVar, System.NonSerialized]
    public int playerNumber = -1;

    private void Start()
    {
        nameText.text = playerName;

        if (!isLocalPlayer) { return; }

        CmdSetPlayerNumber();

        tag = "PlayerConnection";
        FetchVRPlayer();
        player.transform.position = transform.position;
        player.transform.rotation = transform.rotation;
        player.GetComponent<SetCanvasPosition>().ChangeCanvasPosition();
        CmdSetPlayerName("bob" + Random.Range(0, 100));
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        FetchGameManager();
        StartCoroutine(StartRequestLocationData());

        MoveMap[] map = FindObjectsOfType<MoveMap>();
        map[0].playerConnectionTransform = transform;
    }

    public IEnumerator StartConnectionWithGamemanager(string cityName)
    {
        yield return new WaitForEndOfFrame();
        CmdStartFillingLocationData(cityName);
    }

    [Command]
    private void CmdStartFillingLocationData(string cityName)
    {
        FetchGameManager();
        gameManager.CmdStartRetrieveCityData(cityName);
    }

    [Command]
    private void CmdSetPlayerNumber()
    {
        playerNumber = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerRIECTafel>().numberOfPlayers;
    }

    private IEnumerator StartRequestLocationData()
    {
        yield return new WaitForSeconds(3f);
        CmdRequestLocationData(player.dataType.ToString());
    }

    [Command]
    private void CmdRequestLocationData(string dataType)
    {
        FetchGameManager();
        gameManager.CmdGiveBackLocationData(dataType, playerNumber);
    }

    [ClientRpc]
    public void RpcSetLocationDataForPlayer(List<List<string>> locationData, List<string> dataTypes, int playerNumber)
    {
        if (playerNumber != this.playerNumber || !isLocalPlayer) { return; }

        FetchVRPlayer();
        player.SetLocationData(locationData, dataTypes);
    }

    [Command]
    public void CmdSetPlayerName(string name)
    {
        playerName = name;
        RpcSetPlayerName(name);
    }

    [ClientRpc]
    private void RpcSetPlayerName(string name)
    {
        playerName = name;
        nameText.text = playerName;
    }

    private void FetchVRPlayer()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<VRPlayer>();
        }
    }

    private void FetchGameManager()
    {
        if (gameManager == null)
        {
            gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        }
    }
}
