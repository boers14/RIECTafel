using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMapControlDropdown : DropdownSelection
{
    [SerializeField]
    private List<PlayerConnection> players = new List<PlayerConnection>();

    /// <summary>
    /// Initialize variables
    /// </summary>

    public override void Start()
    {
        base.Start();
        dropdown.ClearOptions();
        dropdown.interactable = false;

        dropdown.onValueChanged.AddListener(SetNewPlayerMapController);
    }

    /// <summary>
    /// Fill the option list with all the currently seated players, selectable options have names equal to the player names
    /// </summary>

    public void FillDropdownWithPlayers()
    {
        dropdown.interactable = true;
        dropdown.ClearOptions();
        players.Clear();

        List<string> newOptions = new List<string>();
        PlayerConnection[] possiblePlayers = FindObjectsOfType<PlayerConnection>();
        PlayerConnection ownPlayer = possiblePlayers[0].FetchOwnPlayer();

        for (int i = 0; i < possiblePlayers.Length; i++)
        {
            if (possiblePlayers[i].chosenSeat == -1) { continue; }

            players.Add(possiblePlayers[i]);
            string[] playerName = possiblePlayers[i].playerName.Split('\n');

            if (possiblePlayers[i] != ownPlayer)
            {
                if (playerName.Length > 1)
                {
                    newOptions.Add(playerName[1]);
                }
                else
                {
                    newOptions.Add("Geen naam gevonden");
                }
            } else
            {
                newOptions.Add("Zelf controle nemen");
            }
        }
        newOptions.Add("Geen begeleider");

        dropdown.AddOptions(newOptions);

        // Set the value of the dropdown equal to the current map owner
        PlayerConnection mapOwner = players.Find(player => player.playerIsInControlOfMap);

        if (mapOwner)
        {
            if (mapOwner == mapOwner.FetchOwnPlayer())
            {
                dropdown.value = newOptions.IndexOf("Zelf controle nemen");
            } else
            {
                string[] playerName = mapOwner.playerName.Split('\n');

                if (playerName.Length > 1)
                {
                    dropdown.value = newOptions.IndexOf(playerName[1]);
                } else
                {
                    dropdown.value = newOptions.IndexOf("Geen naam gevonden");
                }
            }
        } else
        {
            dropdown.value = newOptions.IndexOf("Geen begeleider");
        }
    }

    /// <summary>
    /// Give map control to the selected player, unless its the last option, then set the map to free for all
    /// </summary>

    private void SetNewPlayerMapController(int value)
    {
        if (value == players.Count)
        {
            players[0].FetchOwnPlayer().CmdSetMapToFreeForAll();
        }
        else
        {
            players[0].FetchOwnPlayer().CmdSetNewMapOwner(players[value].playerNumber);
        }
    }
}
