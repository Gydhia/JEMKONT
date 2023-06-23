using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DownBelow.Managers;

public class UIRoomItem : MonoBehaviour
{
    public Button JoinButton;
    public TextMeshProUGUI RoomName;
    public string RawName = "";


    public void SetName(string roomName, int players)
    {
        this.RawName = roomName;
        this.RoomName.text = roomName + " (" + players + "/4)";
    }

    public void OnClickJoin()
    {
        NetworkManager.Instance.JoinRoom(RawName);
    }
}
