using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMapControlDropdown : DropdownSelection
{
    [SerializeField]
    private List<PlayerConnection> players = new List<PlayerConnection>();

    public override void Start()
    {
        base.Start();
        dropdown.ClearOptions();
        dropdown.interactable = false;

        dropdown.onValueChanged.AddListener(SetNewPlayerMapController);
    }

    public void FillDropdownWithPlayers()
    {
        dropdown.interactable = true;
        dropdown.ClearOptions();
        players.Clear();

        List<string> newOptions = new List<string>();
        players.AddRange(FindObjectsOfType<PlayerConnection>());
        PlayerConnection ownPlayer = players[0].FetchOwnPlayer();

        for (int i = 0; i < players.Count; i++)
        {
            string[] playerName = players[i].playerName.Split('\n');

            if (players[i] != ownPlayer)
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
