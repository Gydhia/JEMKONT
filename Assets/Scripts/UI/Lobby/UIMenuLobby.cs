using Jemkont.Managers;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuLobby : MonoBehaviour
{
    private List<UIRoomItem> _roomList = new List<UIRoomItem>();
    private List<UIPlayerItem> _playerList = new List<UIPlayerItem>();

    public Button LeaveRoomBtn;
    public Button StartBtn;
    public TextMeshProUGUI LobbyName;
    public UIPlayerItem PlayerPrefab;
    public Transform PlayersHolder;

    public UIRoomItem RoomPrefab;
    public Transform RoomsHolder;

    public TMP_InputField RoomInput;

    public TMP_InputField PlayerNameInput;

    private void Start()
    {
        this.PlayerNameInput.onValueChanged.AddListener(x => NetworkManager.Instance.UpdateOwnerName(this.PlayerNameInput.text));
        this.LeaveRoomBtn.interactable = false;
    }

    public void OnJoinedRoom()
    {
        Debug.Log("Joined room :" + PhotonNetwork.CurrentRoom.Name);
        this.UpdatePlayersList();
        this.UpdatePlayersState();

        this.LeaveRoomBtn.interactable = true;
        this.PlayerNameInput.interactable = false;
        this.LobbyName.text = PhotonNetwork.CurrentRoom.Name;

        this.StartBtn.interactable = PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 1;
    }

    public void OnSelfLeftRoom()
    {
        this._clearPlayers();

        this.LeaveRoomBtn.interactable = false;
        this.PlayerNameInput.interactable = true;
    }

    public void OnPlayerLeftRoom()
    {
        this.UpdatePlayersList();
        this.UpdatePlayersState();
    }

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        // Destroy existing rooms
        for (int i = 0; i < this._roomList.Count; i++)
            Destroy(this._roomList[i].gameObject);

        this._roomList.Clear();

        // Create the updated ones
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount == 0)
                continue;

            this._roomList.Add(Instantiate(this.RoomPrefab, this.RoomsHolder));
            this._roomList[i].LobbyNetworkParent = NetworkManager.Instance;
            this._roomList[i].SetName(roomList[i].Name, roomList[i].PlayerCount);
        }
    }

    public void UpdatePlayersList()
    {
        this._clearPlayers();

        if (PhotonNetwork.CurrentRoom == null)
            return;

        // Create the updated ones
        int count = 0;
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            this._playerList.Add(Instantiate(this.PlayerPrefab, this.PlayersHolder));
            this._playerList[count].LobbyNetworkParent = NetworkManager.Instance;
            this._playerList[count].SetPlayerDatas(player.Value.NickName, player.Value.UserId);

            // Do not make the toggle interactable if it's not ourself
            if (PhotonNetwork.LocalPlayer.UserId != player.Value.UserId)
                this._playerList[count].ReadyToggle.interactable = false;

            count++;
        }
    }

    private void _clearPlayers()
    {
        // Destroy existing players
        for (int i = 0; i < this._playerList.Count; i++)
            Destroy(this._playerList[i].gameObject);

        this._playerList.Clear();
    }

    public void UpdatePlayersFromProperties(Player targetPlayer)
    {
        if (targetPlayer.CustomProperties.ContainsKey("isReady"))
        {
            foreach (UIPlayerItem item in this._playerList)
                if (item.UserID == targetPlayer.UserId)
                    item.ChangeReadyState((bool)targetPlayer.CustomProperties["isReady"]);
        }

        this.UpdatePlayersState();
    }

    public void UpdatePlayersState()
    {
        bool allReady = true;
        for (int i = 0; i < _playerList.Count; i++)
            if (!this._playerList[i].IsReady)
                allReady = false;

        this.StartBtn.interactable = (PhotonNetwork.IsMasterClient && allReady);
    }
}
