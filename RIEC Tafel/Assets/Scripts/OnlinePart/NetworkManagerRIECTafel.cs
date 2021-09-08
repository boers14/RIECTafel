using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerRIECTafel : NetworkManager
{
    [SerializeField]
    private List<Transform> spawnPosses = new List<Transform>();

    private Dictionary<NetworkConnection, Transform> clientSeatConnection = new Dictionary<NetworkConnection, Transform>();

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Transform start = null;

        for (int i = 0; i < spawnPosses.Count; i++)
        {
            if (!clientSeatConnection.ContainsValue(spawnPosses[i]))
            {
                start = spawnPosses[i];
                clientSeatConnection.Add(conn, spawnPosses[i]);
                break;
            }
        }

        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        clientSeatConnection.Remove(conn);
        base.OnServerDisconnect(conn);
    }
}
