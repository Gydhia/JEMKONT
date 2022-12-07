using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon;

namespace Jemkont.Managers
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public UIMenuLobby UILobby;

        public static NetworkManager Instance;

        private void Awake()
        {
            if (NetworkManager.Instance != null)
            {
                if (NetworkManager.Instance != this)
                {
#if UNITY_EDITOR
                    DestroyImmediate(this.gameObject, false);
#else
                Destroy(this.gameObject);
#endif
                    return;
                }
            }

            NetworkManager.Instance = this;

            Object.DontDestroyOnLoad(this);
        }

        void Start()
        {
            _connect();
        }

        public void UpdateOwnerName(string newName)
        {
            PhotonNetwork.NickName = newName;
        }

        private void _connect()
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        #region UI_calls
        public void ClickOnStart()
        {
            if (PhotonNetwork.IsMasterClient)
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
            if (!string.IsNullOrEmpty(this.UILobby.RoomInput.text))
            {
                PhotonNetwork.CreateRoom(this.UILobby.RoomInput.text, new RoomOptions() { MaxPlayers = 4, BroadcastPropsChangeToAll = true, PublishUserId = true }, null);
            }
        }

        #endregion


        #region Players_Callbacks

        public void PlayerAsksForPath(Entity.PlayerBehavior player, Jemkont.GridSystem.Cell target)
        {
            int[] position = new int[2] { target.PositionInGrid.longitude, target.PositionInGrid.latitude };

            if (!PhotonNetwork.IsMasterClient)
                this.photonView.RPC("RPC_ProcessAskedPath", RpcTarget.MasterClient, player.PlayerID, position);
            else
                this.RPC_ProcessAskedPath(player.PlayerID, position);
        }

        [PunRPC]
        public void RPC_ProcessAskedPath(string playerID, int[] target)
        {
            // /!\ To avoid too many requests, when the player already has a new path, don't recalculate another one
            Entity.PlayerBehavior player = GameManager.Instance.Players[playerID];

            GridManager.Instance.FindPath(GameManager.Instance.Players[playerID], new GridPosition(target[0], target[1]));

            int[] positions = GridManager.Instance.SerializePathData();
            // TODO: make a custom function to process combat and non-combat movements
            this.photonView.RPC("RPC_RespondWithProcessedPath", RpcTarget.All, playerID, positions);
        }

        [PunRPC]
        public void RPC_RespondWithProcessedPath(object[] pathDatas)
        {
            Entity.PlayerBehavior movingPlayer = GameManager.Instance.Players[pathDatas[0].ToString()];
            // We manage the fact that 2 players won't obv be on the same grid, so we send the player
            movingPlayer.MoveWithPath(GridManager.Instance.DeserializePathData(movingPlayer, (int[])pathDatas[1]));
        }

        

        #endregion

        #region Photon_UI_callbacks

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            this.UILobby?.UpdateRoomList(roomList);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            this.UILobby?.UpdatePlayersFromProperties(targetPlayer);
        }

        public override void OnJoinedRoom()
        {
            this.UILobby?.OnJoinedRoom();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            this.UILobby?.UpdatePlayersList();
        }

        public override void OnLeftRoom()
        {
            this.UILobby?.OnSelfLeftRoom();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            this.UILobby?.OnPlayerLeftRoom();
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
    }

}
