using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using DownBelow.Managers;

public class UIPlayerItem : MonoBehaviour
{
    public TextMeshProUGUI PlayerName;
    public string UserID;

    public ExitGames.Client.Photon.Hashtable PlayerProperties = new ExitGames.Client.Photon.Hashtable();

    public bool IsReady = false;
    public Toggle ReadyToggle;

    public void SetPlayerDatas(string playerName, string playerID)
    {
        this.PlayerName.text = playerName;
        this.UserID = playerID;
        this.ReadyToggle.SetIsOnWithoutNotify(false);
    }

    public void OnClickReady(bool value)
    {
        this.IsReady = value;
        this.PlayerProperties["isReady"] = this.IsReady;

        // Notify the other players that we changed our state
        PhotonNetwork.SetPlayerCustomProperties(this.PlayerProperties);
    }

    public void ChangeReadyState(bool isReady)
    {
        this.IsReady = isReady;
        this.ReadyToggle.SetIsOnWithoutNotify(isReady);
    }
}
