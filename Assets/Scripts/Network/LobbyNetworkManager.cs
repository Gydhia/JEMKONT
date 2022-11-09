using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class LobbyNetworkManager : MonoBehaviourPunCallbacks
{
    private List<UIRoomItem> _roomList = new List<UIRoomItem>();
    private List<UIPlayerItem> _playerList = new List<UIPlayerItem>();

    public Button LeaveRoomBtn;
    public Button StartBtn;

    public UIPlayerItem PlayerPrefab;
    public Transform PlayersHolder;

    public UIRoomItem RoomPrefab;
    public Transform RoomsHolder;

    public TMP_InputField RoomInput;

    void Start()
    {
        _initialize();
        _connect();
    }

    private void _initialize()
    {
        this.LeaveRoomBtn.interactable = false; 
    }

    private void _connect()
    {
        PhotonNetwork.NickName = "Player" + Random.Range(0, 4);
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    #region Photon_callbacks

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Destroy existing rooms
        for (int i = 0; i < this._roomList.Count; i++)
            Destroy(this._roomList[i].gameObject);

        this._roomList.Clear();

        // Create the updated ones
        for (int i = 0; i < roomList.Count; i++)
        {
            this._roomList.Add(Instantiate(this.RoomPrefab, this.RoomsHolder));
            this._roomList[i].LobbyNetworkParent = this;
            this._roomList[i].RoomName.text = roomList[i].Name;
        }
    }

    public void UpdatePlayersInLobby()
    {
        // Destroy existing players
        for (int i = 0; i < this._playerList.Count; i++)
            Destroy(this._playerList[i].gameObject);

        this._roomList.Clear();

        if (PhotonNetwork.CurrentRoom == null)
            return;

        // Create the updated ones
        int count = 0;
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            this._playerList.Add(Instantiate(this.PlayerPrefab, this.PlayersHolder));
            this._playerList[count].LobbyNetworkParent = this;
            this._playerList[count].PlayerName.text = player.Value.NickName;
            count++;
        }
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room :" + PhotonNetwork.CurrentRoom.Name);
        this.UpdatePlayersInLobby();
        this.UpdatePlayersState();
        this.LeaveRoomBtn.interactable = true;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        this.UpdatePlayersInLobby();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        this.LeaveRoomBtn.interactable = false;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        this.UpdatePlayersInLobby();
        this.UpdatePlayersState();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master serv");
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected");
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("Joined Lobby !");
    }

    #endregion
    public void UpdatePlayersState() 
    {
        bool allReady = true;
        for (int i = 0; i < _playerList.Count; i++)
            if (!this._playerList[i].IsReady)
                allReady = false;

        this.StartBtn.interactable = allReady;
    }

    public void ClickOnStart()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel("Killian");
        }
    }

    public void ClickOnLeave()
    {
        PhotonNetwork.LeaveRoom();
    }


    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void CreateRoom()
    {
        if(!string.IsNullOrEmpty(RoomInput.text))
            PhotonNetwork.CreateRoom(RoomInput.text, new RoomOptions() {  MaxPlayers = 4}, null);
    }
}
