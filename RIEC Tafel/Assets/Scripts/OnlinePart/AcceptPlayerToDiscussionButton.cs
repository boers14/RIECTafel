using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcceptPlayerToDiscussionButton : MeetingButton
{
    [SerializeField]
    private List<GameObject> objectsToSetActiveStateFor = new List<GameObject>();

    private List<bool> activeStatesOfObjects = new List<bool>();

    [SerializeField]
    private GameSceneSettingsButton settingsButton = null;

    private RejectPlayerFromDicussionButton rejectPlayerButton = null;

    private bool settingButtonActiveState = false;

    public bool playerIsRejected { get; set; } = false;

    /// <summary>
    /// Initialize variables
    /// </summary>

    public override void Start()
    {
        if (activeStatesOfObjects.Count > 0) { return; }

        base.Start();
        for (int i = 0; i < objectsToSetActiveStateFor.Count; i++)
        {
            activeStatesOfObjects.Add(false);
        }

        rejectPlayerButton = FindObjectOfType<RejectPlayerFromDicussionButton>();
    }

    /// <summary>
    /// Accept player to discussion if the player isnt rejected, turn on the accept player to a discussion UI again if there are
    /// more players to accept to the discussion else turn it off and turn all other required UI on
    /// </summary>

    public override void ActivateMeetingSet()
    {
        if (!playerIsRejected)
        {
            rejectPlayerButton.connections[0].FetchOwnPlayer().CmdAcceptPlayerToDiscussion(
                rejectPlayerButton.connections[0].playerNumber);
        }

        rejectPlayerButton.connections.RemoveAt(0);
        if (rejectPlayerButton.connections.Count > 0)
        {
            // Reset player is rejected if the last player was rejected
            playerIsRejected = false;
            rejectPlayerButton.connections[0].FetchOwnPlayer().TurnOnAcceptPlayerToDiscussionMenu(
                rejectPlayerButton.connections[0].playerName, true);
            return;
        }

        base.ActivateMeetingSet();
        for (int i = 0; i < objectsToSetActiveStateFor.Count; i++)
        {
            objectsToSetActiveStateFor[i].SetActive(activeStatesOfObjects[i]);
        }

        settingsButton.gameObject.SetActive(settingButtonActiveState);
    }

    /// <summary>
    /// Set active state of objects that should deactivate/ activate
    /// </summary>

    private void OnEnable()
    {
        if (activeStatesOfObjects.Count == 0)
        {
            Start();
        }

        for (int i = 0; i < objectsToSetActiveStateFor.Count; i++)
        {
            activeStatesOfObjects[i] = objectsToSetActiveStateFor[i].activeSelf;
            objectsToSetActiveStateFor[i].SetActive(false);
        }

        settingButtonActiveState = settingsButton.gameObject.activeSelf;
        settingsButton.gameObject.SetActive(true);

        playerIsRejected = false;
    }
}
