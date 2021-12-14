using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class KickPlayerButton : OpenKickPlayerSet
{
    public override void OnButtonClickAction()
    {
        player.FetchOwnPlayer().StartDisconnectPlayerFromDiscussion(player);
    }
}
