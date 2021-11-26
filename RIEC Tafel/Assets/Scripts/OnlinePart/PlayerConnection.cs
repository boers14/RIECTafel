using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public int playerNumber = -1, chosenSeat = -1;

    [SerializeField]
    private MeshFilter head = null, body = null;

    [SerializeField]
    private List<Mesh> allUsableBodyMeshes = new List<Mesh>();

    List<ChooseSeatButton> chooseSeatButtons = new List<ChooseSeatButton>();

    private void Start()
    {
        nameText.text = playerName;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        chooseSeatButtons.AddRange(FindObjectsOfType<ChooseSeatButton>());

        if (!isLocalPlayer) { return; }

        CmdSetPlayerNumber();
        tag = "PlayerConnection";

        FetchVRPlayer();
        FetchGameManager();
        StartCoroutine(StartRequestLocationData());

        MoveMap map = FindObjectOfType<MoveMap>();
        map.playerConnectionTransform = transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            print(chosenSeat);
        }
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

            currentConnections[i].ChangeBodyColorOfPlayer(currentConnections[i].avatarData, currentConnections[i].dataType, currentConnections[i]);
        }

        for (int i = 0; i < chooseSeatButtons.Count; i++)
        {
            chooseSeatButtons[i].CheckIfSeatIsOpen();
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

        for (int i = 0; i < chooseSeatButtons.Count; i++)
        {
            chooseSeatButtons[i].GetComponent<Image>().enabled = true;
            for (int j = 0; j < chooseSeatButtons[i].transform.childCount; j++)
            {
                chooseSeatButtons[i].transform.GetChild(j).gameObject.SetActive(true);
            }
            chooseSeatButtons[i].playerNumber = playerNumber;
        }
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

        PlayerConnection player = FetchPlayerConnectionBasedOnNumber(playerNumber);
        player.avatarData = avatarData;
        player.dataType = (GameManager.DataType)System.Enum.Parse(typeof(GameManager.DataType), dataType);
        ChangeBodyColorOfPlayer(avatarData, player.dataType, player);
    }

    private void ChangeBodyColorOfPlayer(string avatarData, GameManager.DataType dataType, PlayerConnection player)
    {
        string[] differentBodyParts = avatarData.Split(new string[] { "/*nextbodypart*/" }, System.StringSplitOptions.None);

        for (int i = 0; i < differentBodyParts.Length; i++)
        {
            string[] bodyPartData = differentBodyParts[i].Split('\n');
            AvatarCreationManager.TargetedBodyType bodyType = (AvatarCreationManager.TargetedBodyType)System.Enum.Parse(
                typeof(AvatarCreationManager.TargetedBodyType), bodyPartData[0]);

            switch (bodyType)
            {
                case AvatarCreationManager.TargetedBodyType.Body:
                    Transform ground = GameObject.FindGameObjectWithTag("Ground").transform;

                    Vector3 currentHeadScale = player.head.transform.localScale;
                    Vector3 oldBodyScale = player.body.transform.localScale;
                    player.head.transform.SetParent(player.body.transform);

                    SetCorrectModelAndScaleForMesh(player.body, bodyPartData);

                    Vector3 newBodyPos = player.body.transform.position;
                    string[] standardScale = bodyPartData[3].Split('.');
                    if (float.Parse(standardScale[1]) > float.Parse(standardScale[0]))
                    {
                        newBodyPos.y = ground.position.y + player.body.transform.localScale.y / 2;

                        Vector3 newHeadPos = player.head.transform.localPosition;
                        float diffInYPos = oldBodyScale.y / player.body.transform.localScale.y;
                        if (player.body.transform.localScale.y - float.Parse(standardScale[1]) > 0)
                        {
                            diffInYPos *= 1.25f;
                        }
                        else
                        {
                            diffInYPos *= 0.75f;
                        }
                        newHeadPos.y *= diffInYPos;
                        player.head.transform.localPosition = newHeadPos;
                    }
                    else
                    {
                        newBodyPos.y = ground.position.y + player.body.transform.localScale.y;
                    }
                    player.body.transform.position = newBodyPos;

                    player.head.transform.SetParent(player.body.transform.parent);
                    player.head.transform.localScale = currentHeadScale;
                    break;
                case AvatarCreationManager.TargetedBodyType.Head:
                    player.nameText.transform.SetParent(player.transform);
                    SetCorrectModelAndScaleForMesh(player.head, bodyPartData);
                    player.nameText.transform.SetParent(player.head.transform);
                    break;
            }
        }

        List<MeshRenderer> bodyPartsRenderers = new List<MeshRenderer>();
        bodyPartsRenderers.AddRange(new MeshRenderer[] { player.head.GetComponent<MeshRenderer>(), player.body.GetComponent<MeshRenderer>() });
        FetchVRPlayer();
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

    private void SetCorrectModelAndScaleForMesh(MeshFilter bodyPart, string[] bodyPartData)
    {
        bodyPart.mesh = allUsableBodyMeshes.Find(model => model.name == bodyPartData[1]);
        string[] bodyScale = bodyPartData[2].Split('.');
        Vector3 newScale = Vector3.one;
        newScale.x = float.Parse(bodyScale[0]);
        newScale.y = float.Parse(bodyScale[1]);
        newScale.z = float.Parse(bodyScale[2]);
        bodyPart.transform.localScale = newScale;
    }

    private void ChangeBodyColor(List<MeshRenderer> bodyParts, Color32 bodyColor)
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].material.color = bodyColor;
        }
    }

    [Command]
    public void CmdChangePlayerPos(int playerNumber, Vector3 newPos, Vector3 newRot, int seatIndex)
    {
        SetPlayerPosition(playerNumber, newPos, newRot, seatIndex);
        RpcChangePlayerPos(playerNumber, newPos, newRot, seatIndex);
    }

    [ClientRpc]
    private void RpcChangePlayerPos(int playerNumber, Vector3 newPos, Vector3 newRot, int seatIndex)
    {
        SetPlayerPosition(playerNumber, newPos, newRot, seatIndex);
    }

    private void SetPlayerPosition(int playerNumber, Vector3 newPos, Vector3 newRot, int seatIndex)
    {
        PlayerConnection player = FetchPlayerConnectionBasedOnNumber(playerNumber);
        player.transform.position = newPos;
        player.transform.eulerAngles = newRot;
        player.chosenSeat = seatIndex;

        for (int i = 0; i < chooseSeatButtons.Count; i++)
        {
            chooseSeatButtons[i].CheckIfSeatIsOpen();
        }

        if (playerNumber == this.playerNumber && isLocalPlayer)
        {
            FetchVRPlayer();
            poiManager.ChangePOIManagerTransform(player.transform);
            poiManager.GetComponent<SetCanvasPosition>().ChangeCanvasPosition();
            poiManager.RotatePOITextToPlayer();

            PlayerConnection[] currentConnections = FindObjectsOfType<PlayerConnection>();
            for (int i = 0; i < currentConnections.Length; i++)
            {
                if (currentConnections[i] == this) { continue; }

                for (int j = 0; j < currentConnections[i].transform.childCount; j++)
                {
                    currentConnections[i].transform.GetChild(j).gameObject.SetActive(true);
                }
            }

        }
        else
        {
            print(player.chosenSeat);
            if (chosenSeat != -1)
            {
                for (int i = 0; i < player.transform.childCount; i++)
                {
                    player.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
        }
    }

    private PlayerConnection FetchPlayerConnectionBasedOnNumber(int playerNumber)
    {
        PlayerConnection[] currentConnections = FindObjectsOfType<PlayerConnection>();
        List<PlayerConnection> playerConnections = new List<PlayerConnection>(currentConnections);
        return playerConnections.Find(i => i.playerNumber == playerNumber);
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
