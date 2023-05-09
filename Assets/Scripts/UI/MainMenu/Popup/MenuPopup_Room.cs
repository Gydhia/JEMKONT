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
                NetworkManager.Instance.CreateRoom(GameData.Game.RefGameDataContainer.SaveName);
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
            for (int i = 0; i < _playerList.Count; i++)
                if (!this._playerList[i].IsReady)
                    allReady = false;

            this.StartBtn.interactable = (PhotonNetwork.IsMasterClient && allReady);
        }

        public void OnJoinedRoom()
        {
            this.UpdatePlayersList();
            this.UpdatePlayersState();

            this.LeaveRoomBtn.interactable = true;
            this.LobbyName.text = PhotonNetwork.CurrentRoom.Name;

            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                this._playerList[0].ReadyToggle.isOn = true;
                this.StartBtn.interactable = true;
            }
            else
                this.StartBtn.interactable = false;

            OnRoomJoined?.Invoke();
        }

        public void OnSelfLeftRoom()
        {
            this._clearPlayers();

            this.LeaveRoomBtn.interactable = false;
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
    }
}