using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class PlayerConnection : NetworkBehaviour
{
    [SyncVar]
    private string playerName = "", avatarData = "";

    [SerializeField]
    private TMP_Text nameText = null;

    private GameManager gameManager = null;

    private POIManager poiManager = null;

    [SyncVar]
    private GameManager.DataType dataType = GameManager.DataType.Regular;

    [SyncVar, System.NonSerialized]
    public int playerNumber = -1;

    [SerializeField]
    private MeshFilter head = null, body = null; 

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

            currentConnections[i].ChangeBodyColorOfPlayer();
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
        CmdSetPlayerName(nameString, LogInManager.avatarData, poiManager.dataType.ToString());
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
    public void CmdSetPlayerName(string name, string avatarData, string dataType)
    {
        playerName = name;
        this.avatarData = avatarData;

        RpcSetPlayerName(name, avatarData, dataType, playerNumber);
    }

    [ClientRpc]
    private void RpcSetPlayerName(string name, string avatarData, string dataType, int playerNumber)
    {
        FetchVRPlayer();
        playerName = name;
        nameText.text = playerName;
        this.avatarData = avatarData;
        this.dataType = (GameManager.DataType)System.Enum.Parse(typeof(GameManager.DataType), dataType);

        PlayerConnection[] currentConnections = FindObjectsOfType<PlayerConnection>();
        List<PlayerConnection> playerConnections = new List<PlayerConnection>(currentConnections);
        PlayerConnection player = playerConnections.Find(i => i.playerNumber == playerNumber);
        player.ChangeBodyColorOfPlayer();
    }

    private void ChangeBodyColorOfPlayer()
    {
        string[] differentBodyParts = avatarData.Split(new string[] { "/*nextbodypart*/" }, System.StringSplitOptions.None);
        List<Mesh> allModels = new List<Mesh>();
        allModels.AddRange(Resources.FindObjectsOfTypeAll<Mesh>());

        for (int i = 0; i < differentBodyParts.Length; i++)
        {
            string[] bodyPartData = differentBodyParts[i].Split('\n');
            AvatarCreationManager.TargetedBodyType bodyType = (AvatarCreationManager.TargetedBodyType)System.Enum.Parse(
                typeof(AvatarCreationManager.TargetedBodyType), bodyPartData[0]);


            switch (bodyType)
            {
                case AvatarCreationManager.TargetedBodyType.Body:
                    SetCorrectModelAndScaleForMesh(body, allModels, bodyPartData);
                    break;
                case AvatarCreationManager.TargetedBodyType.Head:
                    SetCorrectModelAndScaleForMesh(head, allModels, bodyPartData);
                    break;
            }
        }

        MeshRenderer[] bodyPartsRenderers = GetComponentsInChildren<MeshRenderer>();
        switch (dataType)
        {
            case GameManager.DataType.Regular:
                ChangeBodyColor(bodyPartsRenderers, poiManager.regularPOIColor);
                break;
            case GameManager.DataType.Police:
                ChangeBodyColor(bodyPartsRenderers, poiManager.policePOIColor);
                break;
            case GameManager.DataType.Tax:
                ChangeBodyColor(bodyPartsRenderers, poiManager.taxPOIColor);
                break;
            case GameManager.DataType.PPO:
                ChangeBodyColor(bodyPartsRenderers, poiManager.ppoPOIColor);
                break;
            case GameManager.DataType.Bank:
                ChangeBodyColor(bodyPartsRenderers, poiManager.bankPOIColor);
                break;
        }
    }

    private void SetCorrectModelAndScaleForMesh(MeshFilter bodyPart, List<Mesh> allModels, string[] bodyPartData)
    {
        body.mesh = allModels.Find(model => model.name == bodyPartData[1]);
        string[] bodyScale = bodyPartData[2].Split('.');
        Vector3 newScale = Vector3.one;
        newScale.x = float.Parse(bodyScale[0]);
        newScale.y = float.Parse(bodyScale[1]);
        newScale.z = float.Parse(bodyScale[2]);
        body.transform.localScale = newScale;
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
