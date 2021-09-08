using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerConnection : NetworkBehaviour
{
    [SyncVar]
    private string playerName = "";

    [SerializeField]
    private TextMesh nameText = null;

    private void Start()
    {
        nameText.text = playerName;

        if (isLocalPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = transform.position;
            player.transform.rotation = transform.rotation;
            player.GetComponent<SetCanvasPosition>().ChangeCanvasPosition();
            CmdSetPlayerName("bob" + Random.Range(0, 100));
            for (int i = 0; i  < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    [Command]
    public void CmdSetPlayerName(string name)
    {
        playerName = name;
        RpcSetPlayerName(name);
    }

    [ClientRpc]
    private void RpcSetPlayerName(string name)
    {
        playerName = name;
        nameText.text = playerName;
    }
}
