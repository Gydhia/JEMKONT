using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIPlayerItem : MonoBehaviour
{
    public LobbyNetworkManager LobbyNetworkParent;
    public TextMeshProUGUI PlayerName;

    public bool IsReady = false;

    public void SetName(string playerName)
    {
        this.PlayerName.text = playerName;
    }

    public void OnClickReady()
    {
        this.IsReady = !this.IsReady;
        this.LobbyNetworkParent.UpdatePlayersState();
    }
}
