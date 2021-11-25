using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseSeatButton : MeetingButton
{
    [SerializeField]
    private Transform seatPosition = null;

    public int playerNumber { get; set; } = 0;

    private MiniMap miniMap = null;
    private MoveMap map = null;
    private POIManager poiManager = null;

    public override void Start()
    {
        base.Start();

        foreach(PlayerConnection player in FindObjectsOfType<PlayerConnection>())
        {
            if (player.transform.position == seatPosition.position)
            {
                gameObject.SetActive(false);
            }
        }

        button.onClick.AddListener(SeatPlayer);
        miniMap = FindObjectOfType<MiniMap>();
        map = FindObjectOfType<MoveMap>();
        poiManager = FindObjectOfType<POIManager>();
        StartCoroutine(TurnOffDiscussionUI());
    }

    private IEnumerator TurnOffDiscussionUI()
    {
        yield return new WaitForEndOfFrame();
        EnableMapRendererPieces(false);
        miniMap.gameObject.SetActive(false);
        objectToAcivate.gameObject.SetActive(false);
    }

    private void SeatPlayer()
    {
        List<PlayerConnection> playerConnections = new List<PlayerConnection>();
        playerConnections.AddRange(FindObjectsOfType<PlayerConnection>());
        PlayerConnection player = playerConnections.Find(x => x.playerNumber == playerNumber);
        player.CmdChangePlayerPos(playerNumber, seatPosition.position, seatPosition.eulerAngles);

        miniMap.gameObject.SetActive(true);
        EnableMapRendererPieces(true);
    }

    private void OnDisable()
    {
        poiManager.CheckPOIVisibility();
    }

    private void EnableMapRendererPieces(bool enabled)
    {
        foreach(MeshRenderer renderer in map.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.enabled = enabled;
        }
    }
}
