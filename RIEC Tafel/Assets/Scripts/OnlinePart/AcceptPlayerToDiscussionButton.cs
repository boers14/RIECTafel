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

    [System.NonSerialized]
    public bool playerIsRejected = false;

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

    public override void ActivateMeetingSet()
    {
        if (!playerIsRejected)
        {
            rejectPlayerButton.connections[0].FetchOwnPlayer().CmdAcceptPlayerToDiscussion(rejectPlayerButton.connections[0].playerNumber);
        }

        rejectPlayerButton.connections.RemoveAt(0);
        if (rejectPlayerButton.connections.Count > 0)
        {
            playerIsRejected = false;
            rejectPlayerButton.connections[0].FetchOwnPlayer().TurnOnAcceptPlayerToDiscussionMenu(rejectPlayerButton.connections[0].playerName, 
                true);
            return;
        }

        base.ActivateMeetingSet();
        for (int i = 0; i < objectsToSetActiveStateFor.Count; i++)
        {
            objectsToSetActiveStateFor[i].SetActive(activeStatesOfObjects[i]);
        }

        settingsButton.gameObject.SetActive(settingButtonActiveState);
    }

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
