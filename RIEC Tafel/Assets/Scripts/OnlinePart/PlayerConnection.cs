using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;
using Mapbox.Utils;

public class PlayerConnection : NetworkBehaviour
{
    [SyncVar]
    private string avatarData = "";

    [System.NonSerialized, SyncVar]
    public string playerName = "";

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

    private List<ChooseSeatButton> chooseSeatButtons = new List<ChooseSeatButton>();

    [SerializeField]
    private string mainMenuScene = "";

    [SerializeField]
    private List<Transform> hands = new List<Transform>();

    private List<Transform> actualHands = new List<Transform>();

    private Transform cameraTransform = null;

    private List<Vector3> lastKnownHandPosses = new List<Vector3>(), lastKnownHandRots = new List<Vector3>(), 
        handRotationOffsets = new List<Vector3>();

    private Vector3 lastKnownCameraRot = Vector3.zero, lastMapPos = Vector3.zero, lastMapRot = Vector3.zero, lastMapScale = Vector3.zero;

    private List<PlayerConnection> serverConnectedPlayersList = new List<PlayerConnection>();

    private NetworkManagerRIECTafel networkManager = null;

    private bool playerIsAcceptedToDiscussion = false, isSendingData = false;

    private BoxCollider hitbox = null;

    [System.NonSerialized, SyncVar]
    public bool playerIsInControlOfMap = false;

    private BackToStartPositionButton backToStartPositionButton = null;

    private POISelectionDropdown poiSelectionDropdown = null;

    private PlayerMapControlDropdown playerMapControlDropdown = null;

    private TMP_Text mapOwnerText = null, chooseSeatTitle = null;

    private MoveMap map = null;

    [SerializeField]
    private LineRenderer leftLineRenderer = null, rightLineRenderer = null;

    private void Start()
    {
        nameText.text = playerName;
        EnableChildsOfObject(transform, false);

        chooseSeatButtons.AddRange(FindObjectsOfType<ChooseSeatButton>());
        chooseSeatTitle = GameObject.FindGameObjectWithTag("ChooseSeatPlacementTitle").GetComponent<TMP_Text>();
        networkManager = FindObjectOfType<NetworkManagerRIECTafel>();
        hitbox = GetComponent<BoxCollider>();

        map = FindObjectOfType<MoveMap>();
        lastMapPos = map.transform.position;
        lastMapRot = map.transform.eulerAngles;
        lastMapScale = map.transform.localScale;

        if (!isLocalPlayer) { return; }

        CmdSetPlayerNumber();
        tag = "PlayerConnection";

        FetchVRPlayer();
        FetchGameManager();
        StartCoroutine(StartRequestLocationData());

        map.ownPlayer = this;
        map.playerConnectionTransform = transform;
        for (int i = 0; i < hands.Count; i++)
        {
            lastKnownHandPosses.Add(hands[i].localPosition);
            lastKnownHandRots.Add(hands[i].localEulerAngles);
            handRotationOffsets.Add(hands[i].localEulerAngles);
        }

        cameraTransform = Camera.main.transform;
        lastKnownCameraRot = cameraTransform.eulerAngles;

        List<PlayerHandRays> handRays = new List<PlayerHandRays>();
        handRays.AddRange(FindObjectsOfType<PlayerHandRays>());
        actualHands.Add(handRays.Find(hand => hand.hand == PlayerHandRays.Hand.Left).transform);
        actualHands.Add(handRays.Find(hand => hand.hand == PlayerHandRays.Hand.Right).transform);

        if (isServer)
        {
            playerIsAcceptedToDiscussion = true;
            CmdSetServerAsMapOwner();
        }
    }

    private void FixedUpdate()
    {
        if (!hasAuthority || playerNumber == -1 || chosenSeat == -1) { return; }

        for (int i = 0; i < hands.Count; i++)
        {
            hands[i].position = actualHands[i].position;
            hands[i].eulerAngles = actualHands[i].eulerAngles;

            if (lastKnownHandPosses[i] != hands[i].localPosition || lastKnownHandRots[i] != hands[i].localEulerAngles)
            {
                lastKnownHandPosses[i] = hands[i].localPosition;
                lastKnownHandRots[i] = hands[i].localEulerAngles;
                CmdSetHandPosAndRot(playerNumber, lastKnownHandPosses[i], lastKnownHandRots[i], handRotationOffsets[i], i);
            }
        }

        if (cameraTransform.eulerAngles != lastKnownCameraRot)
        {
            lastKnownCameraRot = cameraTransform.eulerAngles;
            CmdSetHeadRotation(playerNumber, lastKnownCameraRot);
        }

        if (!playerIsInControlOfMap || isSendingData) { return; }

        if (map.transform.position != lastMapPos && map.transform.parent == map.originalParent)
        {
            lastMapPos = map.transform.position;
            CmdMoveMapOnServer(playerNumber, map.offset, poiManager.transform.eulerAngles.y);
        }
        
        if (map.transform.localScale != lastMapScale && map.transform.parent == map.originalParent)
        {
            lastMapScale = map.transform.localScale;
            CmdScaleMapOnServer(playerNumber, lastMapScale);
        }
        
        if (map.transform.eulerAngles != lastMapRot && map.transform.parent == map.originalParent)
        {
            lastMapRot = map.transform.eulerAngles;
            CmdRotateMapOnServer(playerNumber, lastMapRot, poiManager.transform.eulerAngles.y);
        }
    }

    [Command]
    private void CmdMoveMapOnServer(int playerNumber, Vector3 newPos, float yRotationMapOwner)
    {
        RpcMoveMapOnClients(playerNumber, newPos, yRotationMapOwner);
    }

    [ClientRpc]
    private void RpcMoveMapOnClients(int playerNumber, Vector3 newPos, float yRotationMapOwner)
    {
        if (FetchOwnPlayer().playerNumber == playerNumber) { return; }

        MoveMapOnCorrectAngle(newPos, yRotationMapOwner, false);
    }

    private void MoveMapOnCorrectAngle(Vector3 newPos, float yRotationMapOwner, bool ignoreIsTweeningCheck)
    {
        FetchVRPlayer();

        float xPos = newPos.x;
        int rotationDiff = (int)(yRotationMapOwner - poiManager.transform.eulerAngles.y);

        switch (rotationDiff)
        {
            case 90: case -270:
                newPos.x = -newPos.z;
                newPos.z = xPos;
                break;
            case 180: case -180:
                newPos.x = -newPos.x;
                newPos.z = -newPos.z;
                break;
            case 270: case -90:
                newPos.x = newPos.z;
                newPos.z = -xPos;
                break;
        }

        map.SetMapToNewPos(newPos, true, ignoreIsTweeningCheck);
    }

    [Command]
    private void CmdRotateMapOnServer(int playerNumber, Vector3 nextRotation, float yRotationMapOwner)
    {
        RpcRotateMapOnClients(playerNumber, nextRotation, yRotationMapOwner);
    }

    [ClientRpc]
    private void RpcRotateMapOnClients(int playerNumber, Vector3 nextRotation, float yRotationMapOwner)
    {
        if (FetchOwnPlayer().playerNumber == playerNumber) { return; }

        RotateMapOnCorrectAngle(nextRotation, yRotationMapOwner, false);
    }

    private void RotateMapOnCorrectAngle(Vector3 nextRotation, float yRotationMapOwner, bool ignoreIsTweeningCheck)
    {
        FetchVRPlayer();

        int rotationDiff = (int)(yRotationMapOwner - poiManager.transform.eulerAngles.y);
        switch (rotationDiff)
        {
            case 90: case -270:
                nextRotation.y = nextRotation.y - 90;
                break;
            case 180: case -180:
                nextRotation.y = nextRotation.y + 180;
                break;
            case 270: case -90:
                nextRotation.y = nextRotation.y + 90;
                break;
        }

        map.RotateTowardsAngle(nextRotation, ignoreIsTweeningCheck);
    }

    [Command]
    private void CmdScaleMapOnServer(int playerNumber, Vector3 newScale)
    {
        RpcScaleMapOnClients(playerNumber, newScale);
    }

    [ClientRpc]
    private void RpcScaleMapOnClients(int playerNumber, Vector3 newScale)
    {
        if (FetchOwnPlayer().playerNumber == playerNumber) { return; }

        map.ChangeMapScaleToChosenScale(newScale);
    }

    [Command]
    public void CmdSetNewMapCenter(int playerNumber, Vector2d newCenter)
    {
        RpcSetNewMapCenter(playerNumber, newCenter);
    }

    [ClientRpc]
    private void RpcSetNewMapCenter(int playerNumber, Vector2d newCenter)
    {
        if (FetchOwnPlayer().playerNumber == playerNumber) { return; }

        map.SetNewMapCenter(newCenter, false);
    }

    [Command]
    private void CmdSetServerAsMapOwner()
    {
        playerIsInControlOfMap = true;
    }

    [Command]
    public void CmdDrawHandLines(int handSide, int playerNumber)
    {
        RpcDrawHandLines(handSide, playerNumber);
    }

    [ClientRpc]
    private void RpcDrawHandLines(int handSide, int playerNumber)
    {
        if (playerNumber == FetchOwnPlayer().playerNumber) { return; }

        PlayerConnection mapOwner = FetchPlayerConnectionBasedOnNumber(playerNumber);
        switch((PlayerHandRays.Hand)handSide)
        {
            case PlayerHandRays.Hand.Left:
                DrawHandLines(mapOwner.leftLineRenderer);
                break;
            case PlayerHandRays.Hand.Right:
                DrawHandLines(mapOwner.rightLineRenderer);
                break;
        }
    }

    private void DrawHandLines(LineRenderer handSide)
    {
        handSide.enabled = true;

        Vector3 endPos = handSide.transform.position + handSide.transform.forward * 10;
        handSide.SetPositions(new Vector3[] { handSide.transform.position, endPos });
    }

    [Command]
    public void CmdTurnOffhandLine(int handSide, int playerNumber)
    {
        RpcTurnOffhandLine(handSide, playerNumber);
    }

    [ClientRpc]
    private void RpcTurnOffhandLine(int handSide, int playerNumber)
    {
        if (playerNumber == FetchOwnPlayer().playerNumber) { return; }

        PlayerConnection mapOwner = FetchPlayerConnectionBasedOnNumber(playerNumber);
        PlayerHandRays.Hand hand = (PlayerHandRays.Hand)handSide;
        switch (hand)
        {
            case PlayerHandRays.Hand.Left:
                mapOwner.leftLineRenderer.enabled = false;
                break;
            case PlayerHandRays.Hand.Right:
                mapOwner.rightLineRenderer.enabled = false;
                break;
        }
    }

    [Command]
    private void CmdSetHeadRotation(int playerNumber, Vector3 newRot)
    {
        SetHeadRotation(playerNumber, newRot);
        RpcSetHeadRotation(playerNumber, newRot);
    }

    [ClientRpc]
    private void RpcSetHeadRotation(int playerNumber, Vector3 newRot)
    {
        SetHeadRotation(playerNumber, newRot);
    }

    private void SetHeadRotation(int playerNumber, Vector3 newRot)
    {
        if (playerNumber != FetchOwnPlayer().playerNumber)
        {
            FetchPlayerConnectionBasedOnNumber(playerNumber).head.transform.eulerAngles = newRot;
        }
    }

    [Command]
    private void CmdSetHandPosAndRot(int playerNumber, Vector3 newPos, Vector3 newRot, Vector3 rotOffset, int handIndex)
    {
        SetHandPosAndRot(playerNumber, newPos, newRot, rotOffset, handIndex);
        RpcSetHandPosAndRot(playerNumber, newPos, newRot, rotOffset, handIndex);
    }

    [ClientRpc]
    private void RpcSetHandPosAndRot(int playerNumber, Vector3 newPos, Vector3 newRot, Vector3 rotOffset, int handIndex)
    {
        SetHandPosAndRot(playerNumber, newPos, newRot, rotOffset, handIndex);
    }

    private void SetHandPosAndRot(int playerNumber, Vector3 newPos, Vector3 newRot, Vector3 rotOffset, int handIndex)
    {
        if (playerNumber != FetchOwnPlayer().playerNumber)
        {
            PlayerConnection player = FetchPlayerConnectionBasedOnNumber(playerNumber);
            player.hands[handIndex].localPosition = newPos;
            player.hands[handIndex].localEulerAngles = newRot + rotOffset;
        }
    }

    [Command]
    public void CmdAcceptPlayerToDiscussion(int playerNumber)
    {
        RpcAcceptPlayerToDiscussion(playerNumber);
    }

    [ClientRpc]
    private void RpcAcceptPlayerToDiscussion(int playerNumber)
    {
        PlayerConnection ownPlayer = FetchOwnPlayer();
        if (playerNumber != ownPlayer.playerNumber) { return; }

        ownPlayer.playerIsAcceptedToDiscussion = true;
        ownPlayer.EnableChooseSeatButtons();
    }

    public void StartDisconnectPlayerFromDiscussion(PlayerConnection connection)
    {
        serverConnectedPlayersList.Remove(connection);
        CmdDisconnectPlayerFromDiscussion(connection.playerNumber);
    }

    [Command]
    private void CmdDisconnectPlayerFromDiscussion(int playerNumber)
    {
        RpcDisconnectPlayerFromDiscussion(playerNumber);
    }

    [ClientRpc]
    private void RpcDisconnectPlayerFromDiscussion(int playerNumber)
    {
        if (playerNumber != FetchOwnPlayer().playerNumber) { return; }

        NetworkManager.singleton.StopClient();
        SceneManager.LoadScene(mainMenuScene);
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

    private void EnableChildsOfObject(Transform parent, bool enabled)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.SetActive(enabled);
        }
    }

    public void EnableChooseSeatButtons()
    {
        if (playerIsAcceptedToDiscussion && poiManager.allLocationDataIsInitialized)
        {
            chooseSeatTitle.text = "Kies een plek om te zitten:";
            for (int i = 0; i < chooseSeatButtons.Count; i++)
            {
                chooseSeatButtons[i].GetComponent<Image>().enabled = true;
                EnableChildsOfObject(chooseSeatButtons[i].transform, true);
                chooseSeatButtons[i].playerNumber = playerNumber;
                chooseSeatButtons[i].CheckIfSeatIsOpen();
            }
        } else if (!playerIsAcceptedToDiscussion && poiManager.allLocationDataIsInitialized)
        {
            chooseSeatTitle.text = "Wacht tot u wordt toegelaten tot de discussie...";
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
        List<string> neededExtraInfo, List<string> conclusions, List<string> indications, int playerNumber, string cityName)
    {
        if (playerNumber != this.playerNumber || !isLocalPlayer) { return; }

        FetchVRPlayer();
        poiManager.SetLocationData(locationData, dataTypes, neededAmounts, neededExtraInfo, conclusions, indications, cityName);
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

        PlayerConnection ownPlayer = FetchOwnPlayer();
        if (isServer && playerNumber != ownPlayer.playerNumber)
        {
            TurnOnAcceptPlayerToDiscussionMenu(name, false);
            ownPlayer.serverConnectedPlayersList.Add(player);
            networkManager.acceptPlayerToDiscussionUI.GetComponentInChildren<RejectPlayerFromDicussionButton>().connections.Add(player);
        }
    }

    public void TurnOnAcceptPlayerToDiscussionMenu(string name, bool alwaysChangeText)
    {
        TMP_Text text = networkManager.acceptPlayerToDiscussionUI.GetComponentInChildren<TMP_Text>();
        string question = "Wilt u " + name.Split('\n')[1] + " toelaten aan deze discussie?";

        if (alwaysChangeText)
        {
            text.text = question;
        }
        else if (!networkManager.acceptPlayerToDiscussionUI.activeSelf)
        {
            text.text = question;
        }
        
        networkManager.acceptPlayerToDiscussionUI.SetActive(true);
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
                ChangeBodyColor(bodyPartsRenderers, poiManager.regularPOIColor, player.hands);
                break;
            case GameManager.DataType.Police:
                ChangeBodyColor(bodyPartsRenderers, poiManager.policePOIColor, player.hands);
                break;
            case GameManager.DataType.Tax:
                ChangeBodyColor(bodyPartsRenderers, poiManager.taxPOIColor, player.hands);
                break;
            case GameManager.DataType.PPO:
                ChangeBodyColor(bodyPartsRenderers, poiManager.ppoPOIColor, player.hands);
                break;
            case GameManager.DataType.Bank:
                ChangeBodyColor(bodyPartsRenderers, poiManager.bankPOIColor, player.hands);
                break;
        }

        Vector3 newHitboxSize = Vector3.zero;
        Vector3 newHitboxCenter = Vector3.zero;

        if (player.head.sharedMesh.name == "star1")
        {
            newHitboxSize.x = player.head.transform.localScale.x / 35;
            newHitboxSize.y = player.body.transform.localScale.y + (player.head.transform.localScale.y / 35);
            newHitboxSize.z = player.head.transform.localScale.z / 35;

            newHitboxCenter.y = (player.body.transform.localScale.y + (player.head.transform.localScale.y / 35)) / 1.5f;
        }
        else
        {
            newHitboxSize.x = player.head.transform.localScale.x;
            newHitboxSize.y = player.body.transform.localScale.y + player.head.transform.localScale.y;
            newHitboxSize.z = player.head.transform.localScale.z;

            newHitboxCenter.y = (player.body.transform.localScale.y + player.head.transform.localScale.y) / 1.5f;
        }

        player.hitbox.size = newHitboxSize;
        player.hitbox.center = newHitboxCenter;

        player.nameText.transform.SetParent(player.transform);
        player.nameText.transform.LookAt(FindObjectOfType<POIManager>().transform);
        Vector3 lookRotation = player.nameText.transform.eulerAngles;
        lookRotation.x = 0;
        lookRotation.z = 0;
        lookRotation.y += 180;
        player.nameText.transform.eulerAngles = lookRotation;
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

    private void ChangeBodyColor(List<MeshRenderer> bodyParts, Color32 bodyColor, List<Transform> hands)
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].material.color = bodyColor;
        }
       
        for (int i = 0; i < hands.Count; i++)
        {
            hands[i].GetComponentInChildren<SkinnedMeshRenderer>().material.color = bodyColor;
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

        if (chooseSeatButtons.Count == 0)
        {
            chooseSeatButtons.AddRange(FindObjectsOfType<ChooseSeatButton>());
        }
        
        for (int i = 0; i < chooseSeatButtons.Count; i++)
        {
            chooseSeatButtons[i].CheckIfSeatIsOpen();
        }

        PlayerConnection ownPlayer = FetchOwnPlayer();
        if (playerNumber == ownPlayer.playerNumber && isLocalPlayer)
        {
            FetchVRPlayer();
            poiManager.ChangePOIManagerTransform(player.transform);
            poiManager.GetComponent<SetCanvasPosition>().ChangeCanvasPosition();

            PlayerConnection[] currentConnections = FindObjectsOfType<PlayerConnection>();
            for (int i = 0; i < currentConnections.Length; i++)
            {
                if (currentConnections[i] == this || currentConnections[i].chosenSeat == -1) { continue; }

                EnableChildsOfObject(currentConnections[i].transform, true);
                ChangeBodyColorOfPlayer(currentConnections[i].avatarData, currentConnections[i].dataType, currentConnections[i]);
            }

            StartCoroutine(SetPOIUIObjectAndStats());
        }
        else
        {
            if (ownPlayer.chosenSeat != -1)
            {
                EnableChildsOfObject(player.transform, true);
                ChangeBodyColorOfPlayer(player.avatarData, player.dataType, player);

                StartCoroutine(SetPOIUIObjectAndStats());
            }
        }
    }

    private IEnumerator SetPOIUIObjectAndStats()
    {
        yield return new WaitForEndOfFrame();

        if (!playerMapControlDropdown)
        {
            playerMapControlDropdown = FindObjectOfType<PlayerMapControlDropdown>();
            backToStartPositionButton = FindObjectOfType<BackToStartPositionButton>();
            poiSelectionDropdown = FindObjectOfType<POISelectionDropdown>();
            mapOwnerText = GameObject.FindGameObjectWithTag("MapOwnerText").GetComponent<TMP_Text>();

            PlayerConnection ownPlayer = FetchOwnPlayer();

            if (ownPlayer.isServer)
            {
                mapOwnerText.enabled = false;
            }
            else
            {
                backToStartPositionButton.button.interactable = false;
                poiSelectionDropdown.dropdown.interactable = false;
                playerMapControlDropdown.gameObject.SetActive(false);

                List<PlayerConnection> players = new List<PlayerConnection>();
                players.AddRange(FindObjectsOfType<PlayerConnection>());

                PlayerConnection mapOwner = players.Find(player => player.playerIsInControlOfMap);

                if (mapOwner)
                {
                    SetMapOwnerText(mapOwner, ownPlayer);

                    if (mapOwner != ownPlayer)
                    {
                        CmdRetrieveCurrentMapStats(ownPlayer.playerNumber, true);
                    }
                }
                else
                {
                    mapOwnerText.text = "Iedereen bestuurd de kaart zelfstandig";
                }
            }
        }

        if (isServer)
        {
            playerMapControlDropdown.FillDropdownWithPlayers();
        }
    }

    [Command]
    private void CmdRetrieveCurrentMapStats(int playerNumber, bool targetPlayerNumber)
    {
        List<PlayerConnection> players = new List<PlayerConnection>(FindObjectsOfType<PlayerConnection>());
        PlayerConnection mapOwner = players.Find(player => player.playerIsInControlOfMap);

        RpcRetrieveCurrentMapStats(mapOwner.playerNumber, playerNumber, targetPlayerNumber);
    }

    [ClientRpc]
    private void RpcRetrieveCurrentMapStats(int playerNumberMapOwner, int playerNumber, bool targetPlayerNumber)
    {
        PlayerConnection ownPlayer = FetchOwnPlayer();

        if (isLocalPlayer && ownPlayer.playerNumber == playerNumberMapOwner)
        {
            MoveMap map = FindObjectOfType<MoveMap>();
            POIManager poiManager = FindObjectOfType<POIManager>();

            lastMapPos = map.offset;
            lastMapRot = map.transform.eulerAngles;
            lastMapScale = map.transform.localScale;
            isSendingData = false;

            ownPlayer.CmdSetCurrentMapStats(playerNumber, targetPlayerNumber, map.RetrieveMapCenter(), map.offset, map.transform.eulerAngles, 
                poiManager.transform.eulerAngles.y, map.transform.localScale);
        }
    }

    [Command]
    private void CmdSetCurrentMapStats(int playerNumber, bool targetPlayerNumber, Vector2d mapCenter, Vector3 mapOffset, Vector3 mapRotation,
        float yRotationMapOwner, Vector3 mapScale)
    {
        if (targetPlayerNumber)
        {
            RpcSetCurrentMapStatsForTargetedPlayer(playerNumber, mapCenter, mapOffset, mapRotation, yRotationMapOwner, mapScale);
        }
        else
        {
            RpcSetCurrentMapStatsForAllPlayers(playerNumber, mapCenter, mapOffset, mapRotation, yRotationMapOwner, mapScale);
        }
    }

    [ClientRpc]
    private void RpcSetCurrentMapStatsForTargetedPlayer(int playerNumber, Vector2d mapCenter, Vector3 mapOffset, Vector3 mapRotation, 
        float yRotationMapOwner, Vector3 mapScale)
    {
        if (FetchOwnPlayer().playerNumber == playerNumber)
        {
            SetCurrentMapStats(mapCenter, mapOffset, mapRotation, yRotationMapOwner, mapScale);
        }
    }

    [ClientRpc]
    private void RpcSetCurrentMapStatsForAllPlayers(int playerNumber, Vector2d mapCenter, Vector3 mapOffset, Vector3 mapRotation,
        float yRotationMapOwner, Vector3 mapScale)
    {
        if (FetchOwnPlayer().playerNumber != playerNumber)
        {
            SetCurrentMapStats(mapCenter, mapOffset, mapRotation, yRotationMapOwner, mapScale);
        }
    }

    private void SetCurrentMapStats(Vector2d mapCenter, Vector3 mapOffset, Vector3 mapRotation, float yRotationMapOwner, Vector3 mapScale)
    {
        map.SetNewMapCenter(mapCenter, false);
        RotateMapOnCorrectAngle(mapRotation, yRotationMapOwner, true);
        StartCoroutine(SetCurrentMapStatsAfterRotation(mapOffset, mapScale, yRotationMapOwner));
    }

    private IEnumerator SetCurrentMapStatsAfterRotation(Vector3 mapOffset, Vector3 mapScale, float yRotationMapOwner)
    {
        yield return new WaitForSeconds(0.75f);
        map.ChangeMapScaleToChosenScale(mapScale, true);
        MoveMapOnCorrectAngle(mapOffset, yRotationMapOwner, true);
    }

    [Command]
    public void CmdCheckIfSeatsOpenedUp(int playerNumber)
    {
        RpcCheckIfSeatsOpenedUp(playerNumber);
    }

    [ClientRpc]
    private void RpcCheckIfSeatsOpenedUp(int playerNumber)
    {
        PlayerConnection ownPlayer = FetchOwnPlayer();
        PlayerConnection leavingPlayer = FetchPlayerConnectionBasedOnNumber(playerNumber);
        if (leavingPlayer != null)
        {
            if (leavingPlayer.playerNumber != ownPlayer.playerNumber)
            {
                leavingPlayer.gameObject.SetActive(false);
            }
        }

        if (ownPlayer.chosenSeat == -1)
        {
            if (chooseSeatButtons.Count != 6)
            {
                chooseSeatButtons.Clear();
                chooseSeatButtons.AddRange(FindObjectsOfType<ChooseSeatButton>());
            }

            for (int i = 0; i < chooseSeatButtons.Count; i++)
            {
                chooseSeatButtons[i].ActivateSeat();
                chooseSeatButtons[i].CheckIfSeatIsOpen();
            }
        }
    }

    [Command]
    public void CmdSetNewMapOwner(int playerNumber)
    {
        SetNewMapOwner(playerNumber);
        RpcSetNewMapOwner(playerNumber);
    }

    [ClientRpc]
    private void RpcSetNewMapOwner(int playerNumber)
    {
        SetNewMapOwner(playerNumber);
    }

    private void SetNewMapOwner(int playerNumber)
    {
        map.ffaMap = false;

        List<PlayerConnection> players = new List<PlayerConnection>(FindObjectsOfType<PlayerConnection>());

        PlayerConnection oldPlayerInControl = players.Find(player => player.playerIsInControlOfMap);
        if (oldPlayerInControl)
        {
            oldPlayerInControl.playerIsInControlOfMap = false;
            oldPlayerInControl.leftLineRenderer.enabled = false;
            oldPlayerInControl.rightLineRenderer.enabled = false;
        }

        PlayerConnection newPlayerInControl = players.Find(i => i.playerNumber == playerNumber);

        newPlayerInControl.playerIsInControlOfMap = true;

        if (newPlayerInControl.hasAuthority)
        {
            EnablePOIUI(true);
        } else
        {
            EnablePOIUI(false);
        }

        PlayerConnection ownPlayer = FetchOwnPlayer();
        if (!ownPlayer.isServer)
        {
            SetMapOwnerText(newPlayerInControl, ownPlayer);
        }

        if (newPlayerInControl.playerNumber == ownPlayer.playerNumber && newPlayerInControl.hasAuthority)
        {
            ownPlayer.isSendingData = true;
            ownPlayer.CmdRetrieveCurrentMapStats(ownPlayer.playerNumber, false);
        }
    }

    [Command]
    public void CmdSetMapToFreeForAll()
    {
        SetMapToFreeForAll();
        RpcSetMapToFreeForAll();
    }

    [ClientRpc]
    private void RpcSetMapToFreeForAll()
    {
        SetMapToFreeForAll();
    }

    private void SetMapToFreeForAll()
    {
        map.ffaMap = true;

        List<PlayerConnection> players = new List<PlayerConnection>(FindObjectsOfType<PlayerConnection>());

        PlayerConnection oldPlayerInControl = players.Find(player => player.playerIsInControlOfMap);
        if (oldPlayerInControl)
        {
            oldPlayerInControl.playerIsInControlOfMap = false;
        }

        EnablePOIUI(true);

        FetchMapOwnerText();

        if (mapOwnerText)
        {
            mapOwnerText.text = "Iedereen bestuurd de kaart zelfstandig";
        }
    }

    private void SetMapOwnerText(PlayerConnection mapOwner, PlayerConnection ownPlayer)
    {
        FetchMapOwnerText();

        if (mapOwnerText)
        {
            if (mapOwner.playerNumber == ownPlayer.playerNumber)
            {
                mapOwnerText.text = "U bestuurd de kaart";
            }
            else
            {
                mapOwnerText.text = mapOwner.playerName.Split('\n')[1] + " bestuurd de kaart";
            }
        }
    }

    private void EnablePOIUI(bool enabled)
    {
        if (!backToStartPositionButton)
        {
            backToStartPositionButton = FindObjectOfType<BackToStartPositionButton>();
            poiSelectionDropdown = FindObjectOfType<POISelectionDropdown>();
        }

        if (backToStartPositionButton)
        {
            backToStartPositionButton.button.interactable = enabled;
            poiSelectionDropdown.dropdown.interactable = enabled;
        }
    }

    private PlayerConnection FetchPlayerConnectionBasedOnNumber(int playerNumber)
    {
        PlayerConnection[] currentConnections = FindObjectsOfType<PlayerConnection>();
        List<PlayerConnection> playerConnections = new List<PlayerConnection>(currentConnections);
        return playerConnections.Find(i => i.playerNumber == playerNumber);
    }

    public PlayerConnection FetchOwnPlayer()
    {
        PlayerConnection[] currentConnections = FindObjectsOfType<PlayerConnection>();
        List<PlayerConnection> playerConnections = new List<PlayerConnection>(currentConnections);
        return playerConnections.Find(i => i.hasAuthority);
    }

    private void FetchMapOwnerText()
    {
        if (!mapOwnerText)
        {
            GameObject text = GameObject.FindGameObjectWithTag("MapOwnerText");
            if (text)
            {
                mapOwnerText = text.GetComponent<TMP_Text>();
            }
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
