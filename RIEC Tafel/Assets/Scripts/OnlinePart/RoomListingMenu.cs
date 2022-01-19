using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomListingMenu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Transform content = null;

    [SerializeField]
    private RoomListing roomListingPrefab = null;

    private List<RoomListing> roomListings = new List<RoomListing>();

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        roomListings.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
            {
                int index = roomListings.FindIndex(listing => listing.info.Name == roomList[i].Name);
                if (index != -1)
                {
                    Destroy(roomListings[index].gameObject);
                    roomListings.RemoveAt(index);
                }
            }
            else
            {
                int index = roomListings.FindIndex(listing => listing.info.Name == roomList[i].Name);
                if (index == -1)
                {
                    RoomListing roomListing = Instantiate(roomListingPrefab, content);
                    roomListing.SetText(roomList[i]);
                    roomListings.Add(roomListing);
                }
            }
        }
    }
}
