using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RejectPlayerFromDicussionButton : MonoBehaviour
{
    public List<PlayerConnection> connections { get; set; } = new List<PlayerConnection>();

    private AcceptPlayerToDiscussionButton acceptPlayerToDiscussionButton = null;

    /// <summary>
    /// Keep track of all players that just joined the discussion with the connections list
    /// If the player is rejected, start disconnecting the player and deactivate the meeting set if there is no next player
    /// else show this UI for that player
    /// </summary>

    private void Start()
    {
        acceptPlayerToDiscussionButton = FindObjectOfType<AcceptPlayerToDiscussionButton>();
        GetComponent<Button>().onClick.AddListener(RejectPlayerFromDiscussion);
    }

    private void RejectPlayerFromDiscussion()
    {
        connections[0].FetchOwnPlayer().StartDisconnectPlayerFromDiscussion(connections[0]);
        acceptPlayerToDiscussionButton.playerIsRejected = true;
        acceptPlayerToDiscussionButton.ActivateMeetingSet();
    }
}
