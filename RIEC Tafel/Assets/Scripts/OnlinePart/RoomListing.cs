using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class RoomListing : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text = null;

    [System.NonSerialized]
    public RoomInfo info = null;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(JoinRoom);
    }

    public void SetText(RoomInfo info)
    {
        this.info = info;
        text.text = info.Name;
    }

    private void JoinRoom()
    {
        PhotonNetwork.JoinRoom(info.Name);
    }
}
