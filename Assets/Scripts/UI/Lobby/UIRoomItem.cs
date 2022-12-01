using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIRoomItem : MonoBehaviour
{
    public LobbyNetworkManager LobbyNetworkParent;
    public TextMeshProUGUI RoomName;
    public string RawName = "";


    public void SetName(string roomName, int players)
    {
        this.RawName = roomName;
        this.RoomName.text = roomName + " (" + players + "/4)";
    }

    public void OnClickJoin()
    {
        LobbyNetworkParent.JoinRoom(RawName);
    }
}