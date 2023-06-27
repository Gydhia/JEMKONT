using DownBelow.Managers;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI.Menu
{
    public class MenuPopup_Room : BaseMenuPopup
    {
        public Button LeaveRoomBtn;
        public Button StartBtn;

        public UIPlayerItem PlayerPrefab;
        public Transform PlayersHolder;

        public TextMeshProUGUI LobbyName;
        public TextMeshProUGUI PlayerCount;

        private List<UIPlayerItem> _playerList = new List<UIPlayerItem>();
        public Action OnRoomJoined;

        protected void Start()
        {
            this.StartBtn.onClick.AddListener(() => { MenuManager.Instance.StartGame(); this.StartBtn.interactable = false; });
        }

        private void OnEnable()
        {
            if(GameData.Game.RefGameDataContainer != null && PhotonNetwork.CurrentRoom == null)
            {
                string roomName = PhotonNetwork.LocalPlayer.NickName + " - " + GameData.Game.RefGameDataContainer.SaveName;
                this.LobbyName.text = roomName;
                NetworkManager.Instance.CreateRoom(roomName);
                this.UpdatePlayersState();
            }
        }

        public override void HidePopup()
        {
            base.HidePopup();

            NetworkManager.Instance.ClickOnLeave();
        }
        
        
        public void OnPlayerLeftRoom()
        {
            this.UpdatePlayersList();
            this.UpdatePlayersState();
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
                this._playerList[count].SetPlayerDatas(player.Value.NickName, player.Value.UserId);

                // Do not make the toggle interactable if it's not ourself
                if (PhotonNetwork.LocalPlayer.UserId != player.Value.UserId)
                    this._playerList[count].ReadyToggle.interactable = false;

                count++;
            }
            
            
        }

        public void UpdatePlayersState()
        {
            bool allReady = true;
            int readyPlayers = 0;
            for (int i = 0; i < _playerList.Count; i++)
            {
                if (!this._playerList[i].IsReady)
                    allReady = false;
                else
                    readyPlayers++;

            }
            
            PlayerCount.text = "Ready Players : " + readyPlayers.ToString() +"/" + this._playerList.Count.ToString();
            this.StartBtn.interactable = (PhotonNetwork.IsMasterClient && allReady);
        }

        public void OnJoinedRoom()
        {
            this.UpdatePlayersList();
            this.LeaveRoomBtn.interactable = true;
            this.LobbyName.text = PhotonNetwork.CurrentRoom.Name;
            this.UpdatePlayersState();
            this.StartBtn.interactable = false;

            OnRoomJoined?.Invoke();
        }

        public void OnSelfLeftRoom()
        {
            this._clearPlayers();
        }
        private void _clearPlayers()
        {
            // Destroy existing players
            for (int i = 0; i < this._playerList.Count; i++)
            {
                if(this._playerList[i] != null)
                {
                    Destroy(this._playerList[i].gameObject);
                }
            }

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
        
    }
}