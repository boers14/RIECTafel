using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class PlayerConnection : NetworkBehaviour
{
    [SyncVar]
    private string playerName = "";

    [SerializeField]
    private TMP_Text nameText = null;

    private GameManager gameManager = null;

    private POIManager poiManager = null;

    [SyncVar]
    private GameManager.DataType dataType = GameManager.DataType.Regular;

    [SyncVar, System.NonSerialized]
    public int playerNumber = -1;

    private void Start()
    {
        nameText.text = playerName;

        if (!isLocalPlayer) { return; }

        CmdSetPlayerNumber();

        tag = "PlayerConnection";
        FetchVRPlayer();
        poiManager.ChangePOIManagerTransform(transform);
        poiManager.GetComponent<SetCanvasPosition>().ChangeCanvasPosition();

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
        CmdRequestLocationData(poiManager.dataType.ToString());

        PlayerConnection[] currentConnections = FindObjectsOfType<PlayerConnection>();
        for (int i = 0; i < currentConnections.Length; i++)
        {
            if (currentConnections[i] == this) { continue; }

            MeshRenderer[] bodyParts = currentConnections[i].GetComponentsInChildren<MeshRenderer>();
            ChangeBodyColorOfPlayer(currentConnections[i].dataType, bodyParts);
        }

        string nameString = "";
        switch (poiManager.dataType)
        {
            case GameManager.DataType.Regular:
                nameString += "Algemeen";
                break;
            case GameManager.DataType.Police:
                nameString += "Politie";
                break;
            case GameManager.DataType.Tax:
                nameString += "Belasting";
                break;
            case GameManager.DataType.PPO:
                nameString += "OM";
                break;
            case GameManager.DataType.Bank:
                nameString += "Bank";
                break;
        }
        nameString += ":\n" + LogInManager.username;
        CmdSetPlayerName(nameString, poiManager.dataType.ToString());
    }

    [Command]
    private void CmdRequestLocationData(string dataType)
    {
        FetchGameManager();
        gameManager.CmdGiveBackLocationData(dataType, playerNumber);
    }

    [ClientRpc]
    public void RpcSetLocationDataForPlayer(List<string> locationData, List<string> dataTypes, List<string> neededAmounts,
        List<string> neededExtraInfo, List<string> conclusions, List<string> indications, int playerNumber)
    {
        if (playerNumber != this.playerNumber || !isLocalPlayer) { return; }

        FetchVRPlayer();
        poiManager.SetLocationData(locationData, dataTypes, neededAmounts, neededExtraInfo, conclusions, indications);
    }

    [Command]
    public void CmdSetPlayerName(string name, string dataType)
    {
        playerName = name;
        RpcSetPlayerName(name, dataType, playerNumber);
    }

    [ClientRpc]
    private void RpcSetPlayerName(string name, string dataType, int playerNumber)
    {
        FetchVRPlayer();
        playerName = name;
        nameText.text = playerName;

        PlayerConnection[] currentConnections = FindObjectsOfType<PlayerConnection>();
        List<PlayerConnection> playerConnections = new List<PlayerConnection>(currentConnections);
        PlayerConnection player = playerConnections.Find(i => i.playerNumber == playerNumber);
        MeshRenderer[] bodyParts = player.GetComponentsInChildren<MeshRenderer>();
        player.dataType = (GameManager.DataType)System.Enum.Parse(typeof(GameManager.DataType), dataType);
        ChangeBodyColorOfPlayer(player.dataType, bodyParts);
    }

    private void ChangeBodyColorOfPlayer(GameManager.DataType dataType, MeshRenderer[] bodyParts)
    {
        switch (dataType)
        {
            case GameManager.DataType.Regular:
                ChangeBodyColor(bodyParts, poiManager.regularPOIColor);
                break;
            case GameManager.DataType.Police:
                ChangeBodyColor(bodyParts, poiManager.policePOIColor);
                break;
            case GameManager.DataType.Tax:
                ChangeBodyColor(bodyParts, poiManager.taxPOIColor);
                break;
            case GameManager.DataType.PPO:
                ChangeBodyColor(bodyParts, poiManager.ppoPOIColor);
                break;
            case GameManager.DataType.Bank:
                ChangeBodyColor(bodyParts, poiManager.bankPOIColor);
                break;
        }
    }

    private void ChangeBodyColor(MeshRenderer[] bodyParts, Color32 bodyColor)
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            bodyParts[i].material.color = bodyColor;
        }
    }

    private void FetchVRPlayer()
    {
        if (poiManager == null)
        {
            poiManager = GameObject.FindGameObjectWithTag("Player").GetComponent<POIManager>();
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
