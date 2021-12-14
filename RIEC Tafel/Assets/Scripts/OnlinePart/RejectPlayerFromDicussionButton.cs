using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RejectPlayerFromDicussionButton : MonoBehaviour
{
    public List<PlayerConnection> connections { get; set; } = new List<PlayerConnection>();

    private AcceptPlayerToDiscussionButton acceptPlayerToDiscussionButton = null;

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
