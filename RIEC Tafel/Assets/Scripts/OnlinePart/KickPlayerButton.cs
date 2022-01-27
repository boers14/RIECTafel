using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class KickPlayerButton : OpenKickPlayerSet
{
    /// <summary>
    /// When the button is clicked kick the player (this script is on the selected player)
    /// </summary>

    public override void OnButtonClickAction()
    {
        player.FetchOwnPlayer().StartDisconnectPlayerFromDiscussion(player);
    }
}
