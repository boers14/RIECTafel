using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseSeatButton : MeetingButton
{
    [SerializeField]
    private Transform seatPosition = null;

    [SerializeField]
    private int seatIndex = 0;
    
    public int playerNumber { get; set; } = 0;

    private MiniMap miniMap = null;
    private MoveMap map = null;
    private POIManager poiManager = null;

    /// <summary>
    /// Initialize variables
    /// </summary>

    public override void Start()
    {
        base.Start();

        button.onClick.AddListener(SeatPlayer);
        miniMap = FindObjectOfType<MiniMap>();
        map = FindObjectOfType<MoveMap>();
        poiManager = FindObjectOfType<POIManager>();
        StartCoroutine(TurnOffDiscussionUI());
    }

    /// <summary>
    /// Turn off objects that arent required to be turned on at start of scene
    /// </summary>

    private IEnumerator TurnOffDiscussionUI()
    {
        yield return new WaitForEndOfFrame();
        EnableMapRendererPieces(false);
        miniMap.gameObject.SetActive(false);
        objectToAcivate.gameObject.SetActive(false);
    }

    /// <summary>
    /// Seat the player with the playernumber of the owned player in the scene
    /// Also turn on the minimap and the actual map
    /// </summary>

    private void SeatPlayer()
    {
        List<PlayerConnection> playerConnections = new List<PlayerConnection>(FindObjectsOfType<PlayerConnection>());
        PlayerConnection player = playerConnections.Find(x => x.playerNumber == playerNumber);
        player.CmdChangePlayerPos(playerNumber, seatPosition.position, seatPosition.eulerAngles, seatIndex);

        miniMap.gameObject.SetActive(true);
        EnableMapRendererPieces(true);
    }

    /// <summary>
    /// Turn on all currently visible POI's when the seat button UI deactivates (all POI's are set inactive at start so that they are 
    /// not visible when selecting a seat)
    /// </summary>

    private void OnDisable()
    {
        poiManager.CheckPOIVisibility();
    }

    /// <summary>
    /// (de)Activate the visibility of the map
    /// </summary>

    private void EnableMapRendererPieces(bool enabled)
    {
        foreach (MeshRenderer renderer in map.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.enabled = enabled;
        }
    }

    /// <summary>
    /// Show the current seat button
    /// </summary>

    public void ActivateSeat()
    {
        EnableButton(true);
    }

    /// <summary>
    /// Check if a current player has the seatindex of the seat. If so deactivate this button.
    /// </summary>

    public void CheckIfSeatIsOpen()
    {
        foreach (PlayerConnection player in FindObjectsOfType<PlayerConnection>())
        {
            if (player.chosenSeat == seatIndex)
            {
                EnableButton(false);
                break;
            }
        }
    }

    /// <summary>
    /// (de)Activate the button visibility
    /// </summary>

    private void EnableButton(bool enabled)
    {
        GetComponent<Image>().enabled = enabled;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(enabled);
        }
    }
}
