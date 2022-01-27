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

    private bool playerIsAcceptedToDiscussion = false, isSendingData = false, setUIStats = false;

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

    private PlayerConnection mapManager = null;

    private ObjectHolder objectHolder = null;

    /// <summary>
    /// Initialize variables. If the player owns this object, initialize player specific variables
    /// </summary>

    private void Start()
    {
        nameText.text = playerName;
        EnableChildsOfObject(transform, false);

        networkManager = FindObjectOfType<NetworkManagerRIECTafel>();
        hitbox = GetComponent<BoxCollider>();
        chooseSeatButtons.AddRange(FindObjectsOfType<ChooseSeatButton>());

        objectHolder = FindObjectOfType<ObjectHolder>();
        chooseSeatTitle = objectHolder.chooseSeatTitle;
        mapOwnerText = objectHolder.mapOwnerText;
        backToStartPositionButton = objectHolder.backToStartPositionButton;
        poiSelectionDropdown = objectHolder.poiSelectionDropdown;
        playerMapControlDropdown = objectHolder.playerMapControlDropdown;

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

        // Server sets some variables extra for itsself to start the discussion
        if (isServer)
        {
            mapManager = this;
            playerIsAcceptedToDiscussion = true;
            CmdSetServerAsMapOwner();
        }
    }

    /// <summary>
    /// The local players updates the hands position/ rotation, head rotation over the network
    /// If the player owns the map update map position/ scale/ rotation over the network
    /// </summary>

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

    /// <summary>
    /// Set the map on the same position for everyone on the server. Skip the person with the same player number.
    /// </summary>

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

    /// <summary>
    /// Switches around position based on difference between map owner rotation and player rotation, so its looks the same for
    /// all the players
    /// </summary>

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

    /// <summary>
    /// Set the map on the same rotation for everyone on the server. Skip the person with the same player number.
    /// </summary>

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

    /// <summary>
    /// Switches around rotation based on difference between map owner rotation and player rotation, so its looks the same for
    /// all the players
    /// </summary>

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

    /// <summary>
    /// Set the map on the same scale for everyone on the server. Skip the person with the same player number.
    /// </summary>

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

    /// <summary>
    /// Set the map on the same center lat/long value for everyone on the server. Skip the person with the same player number.
    /// </summary>

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

    /// <summary>
    /// Called at the start by the server to make the server the starting map owner.
    /// </summary>

    [Command]
    private void CmdSetServerAsMapOwner()
    {
        playerIsInControlOfMap = true;
    }

    /// <summary>
    /// Set line renderer visuals for given hand of the player with given player number. Skip if the own player has this playernumber. 
    /// </summary>

    [Command]
    public void CmdDrawHandLines(int handSide, int playerNumber, Vector3 tableLocalPos, float playerYRot)
    {
        RpcDrawHandLines(handSide, playerNumber, tableLocalPos, playerYRot);
    }

    [ClientRpc]
    private void RpcDrawHandLines(int handSide, int playerNumber, Vector3 tableLocalPos, float playerYRot)
    {
        if (playerNumber == FetchOwnPlayer().playerNumber) { return; }

        PlayerConnection mapOwner = FetchPlayerConnectionBasedOnNumber(playerNumber);
        switch((PlayerHandRays.Hand)handSide)
        {
            case PlayerHandRays.Hand.Left:
                DrawHandLines(mapOwner.leftLineRenderer, tableLocalPos, playerYRot);
                break;
            case PlayerHandRays.Hand.Right:
                DrawHandLines(mapOwner.rightLineRenderer, tableLocalPos, playerYRot);
                break;
        }
    }

    /// <summary>
    /// Set the linerenderer visuals based on the local position of the connection point that the ray touched the table.
    /// Switch around these position so they always point to the same point based on the rotational difference, then calculate them
    /// to real world positions.
    /// </summary>

    private void DrawHandLines(LineRenderer handSide, Vector3 tableLocalPos, float playerYRot)
    {
        FetchVRPlayer();
        if (map == null)
        {
            FindObjectOfType<MoveMap>();
        }

        handSide.enabled = true;

        float xPos = tableLocalPos.x;
        int rotationDiff = (int)(playerYRot - poiManager.transform.eulerAngles.y);

        switch (rotationDiff)
        {
            case 90: case -270:
                tableLocalPos.x = -tableLocalPos.z;
                tableLocalPos.z = xPos;
                break;
            case 180: case -180:
                tableLocalPos.x = -tableLocalPos.x;
                tableLocalPos.z = -tableLocalPos.z;
                break;
            case 270: case -90:
                tableLocalPos.x = tableLocalPos.z;
                tableLocalPos.z = -xPos;
                break;
        }

        Vector3 endPos = map.table.position;
        endPos.x += tableLocalPos.x * map.table.localScale.x;
        endPos.y += tableLocalPos.y * map.table.localScale.y;
        endPos.z += tableLocalPos.z * map.table.localScale.z;

        handSide.SetPositions(new Vector3[] { handSide.transform.position, endPos });
    }

    /// <summary>
    /// Turn off the line renderer for the player with the given player number. Skip the person with the same player number.
    /// </summary>

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

    /// <summary>
    /// Set the head of given playernumber on the same rotation for everyone on the server. 
    /// Skip the person with the same player number.
    /// </summary>

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

    /// <summary>
    /// Set the hands of given playernumber on the same position/ rotation for everyone on the server. 
    /// Skip the person with the same player number.
    /// </summary>

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

    /// <summary>
    /// Accept the given playernumber to the discussion and enable that player to choose a seat
    /// </summary>

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

    /// <summary>
    /// Disconnect the given player number from the discussion by stopping the network client (if server stops everyone gets 
    /// disconnected automatically) and send that player to main menu
    /// </summary>

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

    /// <summary>
    /// Called once in network manager when game manager is created. Game manager retrieves all required data from database.
    /// </summary>

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

    /// <summary>
    /// Sets the playernumber for the player. This is a unique ID for the player for the program to recognize the player.
    /// NumberOfPlayers isnt actually always equal to the amount of players in the discussion, it never counts down when a player
    /// leaves
    /// </summary>

    [Command]
    private void CmdSetPlayerNumber()
    {
        playerNumber = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerRIECTafel>().numberOfPlayers;
    }

    /// <summary>
    /// Asks for the location data on server side from the game manager
    /// Also begin setting player personal data for all other players
    /// </summary>

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

    /// <summary>
    /// (de)Activate all childeren of an object
    /// </summary>

    private void EnableChildsOfObject(Transform parent, bool enabled)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.SetActive(enabled);
        }
    }

    /// <summary>
    /// Based on if the player is accepted to the discussion and if the POI manager has all location data turn on the choose seat
    /// buttons that arent seated yet
    /// </summary>

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

    /// <summary>
    /// Start the request for location data from the game manager based on the given data type
    /// </summary>

    [Command]
    private void CmdRequestLocationData(string dataType)
    {
        FetchGameManager();
        gameManager.CmdGiveBackLocationData(dataType, playerNumber);
    }

    /// <summary>
    /// After the location data is gotten call this function to set location data in POI manager to initialize it there further.
    /// Only perform that function if the player is the given playernumber.
    /// </summary>

    [ClientRpc]
    public void RpcSetLocationDataForPlayer(List<string> locationData, List<string> dataTypes, List<string> neededAmounts,
        List<string> neededExtraInfo, List<string> conclusions, List<string> indications, int playerNumber, string cityName)
    {
        if (playerNumber != this.playerNumber || !isLocalPlayer) { return; }

        FetchVRPlayer();
        poiManager.SetLocationData(locationData, dataTypes, neededAmounts, neededExtraInfo, conclusions, indications, cityName);
    }

    /// <summary>
    /// Sets the playername and avatar data on server so syncvar will line data up for later connecting players
    /// </summary>

    [Command]
    public void CmdSetPlayerName(string name, string avatarData, string dataType)
    {
        playerName = name;
        this.avatarData = avatarData;

        RpcSetPlayerName(name, avatarData, dataType, playerNumber);
    }

    /// <summary>
    /// Also set personal data instantly on other connected players. If the newly connected player is not the server turn the accept
    /// player to discussion menu on for the server player.
    /// </summary>

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

    /// <summary>
    /// Turn on accept player to discussion menu with required text. Dont always change the text, because when a new player joins
    /// the old players could not be handled yet
    /// </summary>

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

    /// <summary>
    /// Sets given avatar to player, changes body color of player to body color of faction, sets hitbox of player, rotates name text
    /// to own player
    /// </summary>

    private void ChangeBodyColorOfPlayer(string avatarData, GameManager.DataType dataType, PlayerConnection player)
    {
        // Set body part data
        string[] differentBodyParts = avatarData.Split(new string[] { "/*nextbodypart*/" }, System.StringSplitOptions.None);

        for (int i = 0; i < differentBodyParts.Length; i++)
        {
            string[] bodyPartData = differentBodyParts[i].Split('\n');
            AvatarCreationManager.TargetedBodyType bodyType = (AvatarCreationManager.TargetedBodyType)System.Enum.Parse(
                typeof(AvatarCreationManager.TargetedBodyType), bodyPartData[0]);

            switch (bodyType)
            {
                case AvatarCreationManager.TargetedBodyType.Body:
                    // Perform same type of calculation for y position of body as in avatar manager for the body
                    Transform ground = GameObject.FindGameObjectWithTag("Ground").transform;

                    // Parent head to body while remember old variables
                    Vector3 currentHeadScale = player.head.transform.localScale;
                    Vector3 oldBodyScale = player.body.transform.localScale;
                    player.head.transform.SetParent(player.body.transform);

                    // Set new model and scale
                    SetCorrectModelAndScaleForMesh(player.body, bodyPartData);

                    // Set body on correct y position on the floor
                    Vector3 newBodyPos = player.body.transform.position;
                    string[] standardScale = bodyPartData[3].Split('.');
                    if (float.Parse(standardScale[1]) > float.Parse(standardScale[0]))
                    {
                        // Cubical bodies worked differently due to their y scale being twice as large
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

                    // Unparent head and set its scale correct again
                    player.head.transform.SetParent(player.body.transform.parent);
                    player.head.transform.localScale = currentHeadScale;
                    break;
                case AvatarCreationManager.TargetedBodyType.Head:
                    // Make sure name text doesnt get a weird scale by parenting it to player
                    player.nameText.transform.SetParent(player.transform);
                    SetCorrectModelAndScaleForMesh(player.head, bodyPartData);
                    player.nameText.transform.SetParent(player.head.transform);
                    break;
            }
        }

        // Change avatar body color
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

        // Change avatar hitbox
        Vector3 newHitboxSize = Vector3.zero;
        Vector3 newHitboxCenter = Vector3.zero;

        // The star has a scale of 35 instead of 1
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

        // Rotate name text to own player
        player.nameText.transform.SetParent(player.transform);
        player.nameText.transform.LookAt(FindObjectOfType<POIManager>().transform);
        Vector3 lookRotation = player.nameText.transform.eulerAngles;
        lookRotation.x = 0;
        lookRotation.z = 0;
        lookRotation.y += 180;
        player.nameText.transform.eulerAngles = lookRotation;
    }

    /// <summary>
    /// Set mesh to the given mesh name (all meshes listed in allUsableBodyMeshes), set scale to given body scale
    /// </summary>

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

    /// <summary>
    /// Change the given body parts and hands material color to the given color
    /// </summary>

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

    /// <summary>
    /// Set given position/ rotation/ seatindex to the given player on all players in the discussion when a choose seat button
    /// is pressed.
    /// </summary>

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

    /// <summary>
    /// Set given rotation/ position/ seatindex to given player.
    /// If a player is still choosing a seat disable all chosen seats.
    /// If its the own player initialize the new position and initialize all other players in the discussion.
    /// If it another player and a seat is chosen, initialize the new player.
    /// </summary>

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
        
        // Disable chosen seats
        for (int i = 0; i < chooseSeatButtons.Count; i++)
        {
            chooseSeatButtons[i].CheckIfSeatIsOpen();
        }

        PlayerConnection ownPlayer = FetchOwnPlayer();
        if (playerNumber == ownPlayer.playerNumber && isLocalPlayer)
        {
            // Set room object correctly visible for new position/ rotation of player
            FetchVRPlayer();
            poiManager.ChangePOIManagerTransform(player.transform);
            poiManager.GetComponent<SetCanvasPosition>().ChangeCanvasPosition();

            // Enable all other players and set their variables
            PlayerConnection[] currentConnections = FindObjectsOfType<PlayerConnection>();
            for (int i = 0; i < currentConnections.Length; i++)
            {
                if (currentConnections[i] == this || currentConnections[i].chosenSeat == -1) { continue; }

                EnableChildsOfObject(currentConnections[i].transform, true);
                ChangeBodyColorOfPlayer(currentConnections[i].avatarData, currentConnections[i].dataType, currentConnections[i]);
            }

            // Initialize the map control UI on the canvas
            StartCoroutine(SetPOIUIObjectAndStats());
        }
        else
        {
            // If the player is seated enable the new player and set their variables
            if (ownPlayer.chosenSeat != -1)
            {
                EnableChildsOfObject(player.transform, true);
                ChangeBodyColorOfPlayer(player.avatarData, player.dataType, player);

                // Refill the player map owner dropdown for the server
                StartCoroutine(SetPOIUIObjectAndStats());
            }
        }
    }

    /// <summary>
    /// If its the server disable the mapowner text the first time and always update map owner dropdown
    /// If its client disable all map control UI and map owner dropdown. Check for a map owner and if there is one,
    /// set the current map variables to the variables of the map owner
    /// </summary>

    private IEnumerator SetPOIUIObjectAndStats()
    {
        yield return new WaitForEndOfFrame();

        if (!setUIStats)
        {
            setUIStats = true;

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

                List<PlayerConnection> players = new List<PlayerConnection>(FindObjectsOfType<PlayerConnection>());
                PlayerConnection mapOwner = players.Find(player => player.playerIsInControlOfMap);

                // If its a free for all map, there is no map owner
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

    /// <summary>
    /// Starts the process of retrieving the current map data from the map owner
    /// </summary>

    [Command]
    private void CmdRetrieveCurrentMapStats(int playerNumber, bool targetPlayerNumber)
    {
        List<PlayerConnection> players = new List<PlayerConnection>(FindObjectsOfType<PlayerConnection>());
        PlayerConnection mapOwner = players.Find(player => player.playerIsInControlOfMap);

        RpcRetrieveCurrentMapStats(mapOwner.playerNumber, playerNumber, targetPlayerNumber);
    }

    /// <summary>
    /// Go down on client side to retrieve the latest map data and the player rotation of the map owner
    /// </summary>

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

            ownPlayer.CmdSetCurrentMapStats(playerNumber, targetPlayerNumber, map.RetrieveMapCenter(), map.offset, 
                map.transform.eulerAngles, poiManager.transform.eulerAngles.y, map.transform.localScale);
        }
    }

    /// <summary>
    /// Based on if the map variables are set for all players in the server or a player in specific perform a different RPC
    /// </summary>

    [Command]
    private void CmdSetCurrentMapStats(int playerNumber, bool targetPlayerNumber, Vector2d mapCenter, Vector3 mapOffset, 
        Vector3 mapRotation, float yRotationMapOwner, Vector3 mapScale)
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

    /// <summary>
    /// Only if the own player has the playernumber perform the function for setting map variables
    /// </summary>

    [ClientRpc]
    private void RpcSetCurrentMapStatsForTargetedPlayer(int playerNumber, Vector2d mapCenter, Vector3 mapOffset, Vector3 mapRotation, 
        float yRotationMapOwner, Vector3 mapScale)
    {
        if (FetchOwnPlayer().playerNumber == playerNumber)
        {
            SetCurrentMapStats(mapCenter, mapOffset, mapRotation, yRotationMapOwner, mapScale);
        }
    }

    /// <summary>
    /// Perform the map setting function for all players except the player with the given number
    /// </summary>

    [ClientRpc]
    private void RpcSetCurrentMapStatsForAllPlayers(int playerNumber, Vector2d mapCenter, Vector3 mapOffset, Vector3 mapRotation,
        float yRotationMapOwner, Vector3 mapScale)
    {
        if (FetchOwnPlayer().playerNumber != playerNumber)
        {
            SetCurrentMapStats(mapCenter, mapOffset, mapRotation, yRotationMapOwner, mapScale);
        }
    }

    /// <summary>
    /// First set the new map center and then rotate the map
    /// </summary>

    private void SetCurrentMapStats(Vector2d mapCenter, Vector3 mapOffset, Vector3 mapRotation, float yRotationMapOwner, Vector3 mapScale)
    {
        map.SetNewMapCenter(mapCenter, false);
        RotateMapOnCorrectAngle(mapRotation, yRotationMapOwner, true);
        StartCoroutine(SetCurrentMapStatsAfterRotation(mapOffset, mapScale, yRotationMapOwner));
    }

    /// <summary>
    /// After a small interval where the rotation is correctly being set, set the new position and scale
    /// </summary>

    private IEnumerator SetCurrentMapStatsAfterRotation(Vector3 mapOffset, Vector3 mapScale, float yRotationMapOwner)
    {
        yield return new WaitForSeconds(0.175f);
        map.ChangeMapScaleToChosenScale(mapScale, true);
        MoveMapOnCorrectAngle(mapOffset, yRotationMapOwner, true);
    }

    /// <summary>
    /// Called in the network manager when a player leaves. If a player is still choosing a seat, the seat of the leaving player 
    /// will open up.
    /// </summary>

    [Command]
    public void CmdCheckIfSeatsOpenedUp(int playerNumber)
    {
        RpcCheckIfSeatsOpenedUp(playerNumber);
    }

    [ClientRpc]
    private void RpcCheckIfSeatsOpenedUp(int playerNumber)
    {
        PlayerConnection ownPlayer = FetchOwnPlayer();

        // Disable leaving player so it cant be found anymore when searching for all players
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

    /// <summary>
    /// Gives the player with the given playernumber map control, removes control from the old player in control if there is one
    /// </summary>

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

        // Remove control from the old player in control
        PlayerConnection oldPlayerInControl = players.Find(player => player.playerIsInControlOfMap);
        if (oldPlayerInControl)
        {
            oldPlayerInControl.playerIsInControlOfMap = false;
            oldPlayerInControl.leftLineRenderer.enabled = false;
            oldPlayerInControl.rightLineRenderer.enabled = false;
        }

        // Find the new player to give control
        PlayerConnection newPlayerInControl = players.Find(i => i.playerNumber == playerNumber);

        newPlayerInControl.playerIsInControlOfMap = true;

        // Enable map UI functions if the player has control else turn them off
        if (newPlayerInControl.hasAuthority)
        {
            EnablePOIUI(true);
        } else
        {
            EnablePOIUI(false);
        }

        // Change map owner text if the player isnt the server
        PlayerConnection ownPlayer = FetchOwnPlayer();
        if (!ownPlayer.isServer)
        {
            SetMapOwnerText(newPlayerInControl, ownPlayer);
        }

        ownPlayer.mapManager = newPlayerInControl;

        // Set new map variables to this player map variables for all players in the server
        if (newPlayerInControl.playerNumber == ownPlayer.playerNumber && newPlayerInControl.hasAuthority)
        {
            ownPlayer.isSendingData = true;
            ownPlayer.CmdRetrieveCurrentMapStats(ownPlayer.playerNumber, false);
        }
    }

    /// <summary>
    /// Set the map to free for all, remove control from the player that is in control. Enable map control UI.
    /// </summary>

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

        FetchOwnPlayer().mapManager = null;
        EnablePOIUI(true);

        if (mapOwnerText)
        {
            mapOwnerText.text = "Iedereen bestuurd de kaart zelfstandig";
        }
    }

    /// <summary>
    /// Set the map owner text to show who owns the map
    /// </summary>

    private void SetMapOwnerText(PlayerConnection mapOwner, PlayerConnection ownPlayer)
    {
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

    /// <summary>
    /// Whenever a player leaves call in the network manager a refill for the player map owner dropdown with all current players.
    /// Set the map owner to be the server if the map owner left.
    /// </summary>

    [Command]
    public void CmdRefillPlayerMapControlDropdown()
    {
        RpcRefillPlayerMapControlDropdown();
    }

    [ClientRpc]
    private void RpcRefillPlayerMapControlDropdown()
    {
        PlayerConnection ownPlayer = FetchOwnPlayer();
        if (!ownPlayer.isServer) { return; }

        if (!ownPlayer.map.ffaMap && ownPlayer.mapManager == null)
        {
            ownPlayer.CmdSetNewMapOwner(ownPlayer.playerNumber);
        }

        ownPlayer.playerMapControlDropdown.FillDropdownWithPlayers();
    }

    /// <summary>
    /// (de)Activates the map control UI
    /// </summary>

    private void EnablePOIUI(bool enabled)
    {
        if (backToStartPositionButton)
        {
            backToStartPositionButton.button.interactable = enabled;
            poiSelectionDropdown.dropdown.interactable = enabled;
        }
    }

    /// <summary>
    /// Grabs the player with the given playernumber
    /// </summary>

    private PlayerConnection FetchPlayerConnectionBasedOnNumber(int playerNumber)
    {
        List<PlayerConnection> playerConnections = new List<PlayerConnection>(FindObjectsOfType<PlayerConnection>());
        return playerConnections.Find(i => i.playerNumber == playerNumber);
    }

    /// <summary>
    /// Grabs the player that the player owns
    /// </summary>

    public PlayerConnection FetchOwnPlayer()
    {
        List<PlayerConnection> playerConnections = new List<PlayerConnection>(FindObjectsOfType<PlayerConnection>());
        return playerConnections.Find(i => i.hasAuthority);
    }

    /// <summary>
    /// Fethes the POI manager if its still null
    /// </summary>

    private void FetchVRPlayer()
    {
        if (poiManager == null)
        {
            poiManager = GameObject.FindGameObjectWithTag("Player").GetComponent<POIManager>();
        }
    }

    /// <summary>
    /// Fethes the gameManager if its still null
    /// </summary>

    private void FetchGameManager()
    {
        if (gameManager == null)
        {
            gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        }
    }
}
