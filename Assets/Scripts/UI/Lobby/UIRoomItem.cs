using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIRoomItem : MonoBehaviour
{
    public LobbyNetworkManager LobbyNetworkParent;
    public TextMeshProUGUI RoomName;
    
    public void SetName(string roomName)
    {
        this.RoomName.text = roomName;
    }

    public void OnClickJoin()
    {
        LobbyNetworkParent.JoinRoom(RoomName.text);
    }
}
