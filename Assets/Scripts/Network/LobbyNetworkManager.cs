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
    public TextMeshProUGUI LobbyName;
    public UIPlayerItem PlayerPrefab;
    public Transform PlayersHolder;

    public UIRoomItem RoomPrefab;
    public Transform RoomsHolder;

    public TMP_InputField RoomInput;

    public TMP_InputField PlayerNameInput;

    void Start()
    {
        _initialize();
        _connect();

        this.PlayerNameInput.onValueChanged.AddListener(x => _updateOwnerName());
    }
    private void _updateOwnerName()
    {
        PhotonNetwork.NickName = this.PlayerNameInput.text;
    }


    private void _initialize()
    {
        this.LeaveRoomBtn.interactable = false; 
    }

    private void _connect()
    {
        PhotonNetwork.NickName = this.PlayerNameInput.text;
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
            if (roomList[i].PlayerCount == 0)
                continue;

            this._roomList.Add(Instantiate(this.RoomPrefab, this.RoomsHolder));
            this._roomList[i].LobbyNetworkParent = this;
            this._roomList[i].SetName(roomList[i].Name, roomList[i].PlayerCount);
        }
    }

    private void _clearPlayers()
    {
        // Destroy existing players
        for (int i = 0; i < this._playerList.Count; i++)
            Destroy(this._playerList[i].gameObject);

        this._playerList.Clear();
    }

    public void UpdatePlayersInLobby()
    {
        this._clearPlayers();

        if (PhotonNetwork.CurrentRoom == null)
            return;

        // Create the updated ones
        int count = 0;
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            this._playerList.Add(Instantiate(this.PlayerPrefab, this.PlayersHolder));
            this._playerList[count].LobbyNetworkParent = this;
            this._playerList[count].SetPlayerDatas(player.Value.NickName, player.Value.UserId);
            //if()

            count++;
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (targetPlayer.CustomProperties.ContainsKey("isReady"))
        {
            foreach (UIPlayerItem item in this._playerList)
            {
                if (item.UserID == targetPlayer.UserId)
                {
                    item.ChangeReadyState((bool)targetPlayer.CustomProperties["isReady"]);
                }
            }
        }

        this.UpdatePlayersState();
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
        this.PlayerNameInput.interactable = false;
        this.LobbyName.text = PhotonNetwork.CurrentRoom.Name;
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        this.UpdatePlayersInLobby();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        this._clearPlayers();

        this.LeaveRoomBtn.interactable = false;
        this.PlayerNameInput.interactable = true;
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

        if(PhotonNetwork.IsMasterClient)
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
        if(!string.IsNullOrEmpty(this.RoomInput.text))
            PhotonNetwork.CreateRoom(this.RoomInput.text, new RoomOptions() {  MaxPlayers = 4, BroadcastPropsChangeToAll = true }, null);
    }
}
