using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon;
using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Linq;
using System.Runtime.CompilerServices;
using DownBelow.Mechanics;

namespace DownBelow.Managers {
    public class NetworkManager : MonoBehaviourPunCallbacks {
        public UIMenuLobby UILobby;

        public static NetworkManager Instance;

        private void Awake() {
            if (NetworkManager.Instance != null) {
                if (NetworkManager.Instance != this) {
#if UNITY_EDITOR
                    DestroyImmediate(this.gameObject,false);
#else
                Destroy(this.gameObject);
#endif
                    return;
                }
            }

            NetworkManager.Instance = this;

            Object.DontDestroyOnLoad(this);
        }

        void Start() {
            _connect();
        }

        public void UpdateOwnerName(string newName) {
            PhotonNetwork.NickName = newName;
        }

        private void _connect() {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        #region UI_calls
        public void ClickOnStart() {
            if (PhotonNetwork.IsMasterClient) {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.LoadLevel("Killian");
            }
        }

        public void ClickOnLeave() {
            PhotonNetwork.LeaveRoom();
        }


        public void JoinRoom(string roomName) {
            PhotonNetwork.JoinRoom(roomName);
        }

        public void CreateRoom() {
            if (!string.IsNullOrEmpty(this.UILobby.RoomInput.text)) {
                PhotonNetwork.CreateRoom(this.UILobby.RoomInput.text,new RoomOptions() { MaxPlayers = 4,BroadcastPropsChangeToAll = true,PublishUserId = true },null);
            }
        }

        #endregion


        #region Players_Callbacks
        public void EntityAsksForPath(CharacterEntity entity,Cell target,WorldGrid refGrid) {
            string mainGrid = refGrid is CombatGrid cGrid ? cGrid.ParentGrid.UName : refGrid.UName;
            string innerGrid = mainGrid == refGrid.UName ? string.Empty : refGrid.UName;

            GridManager.Instance.FindPath(entity,target.PositionInGrid);
            int[] positions = GridManager.Instance.SerializePathData();

            this.photonView.RPC("RPC_RespondWithEntityProcessedPath",RpcTarget.All,entity.UID,positions,mainGrid,innerGrid);
        }

        [PunRPC]
        public void RPC_RespondWithEntityProcessedPath(string entityID,int[] pathResult,string mainGrid,string innerGrid) {
            // TODO : remove dat Ugly func
            EnemyEntity entity = innerGrid != string.Empty ?
                GridManager.Instance.WorldGrids[mainGrid].InnerCombatGrids[innerGrid].GridEntities.First(e => e.UID == entityID) as EnemyEntity :
                GridManager.Instance.WorldGrids[mainGrid].GridEntities.First(e => e.UID == entityID) as EnemyEntity;

            // We manage the fact that 2 players won't obv be on the same grid, so we send the player
            entity.MoveWithPath(GridManager.Instance.DeserializePathData(entity,pathResult),string.Empty);
        }

        public void PlayerAsksForPath(PlayerBehavior player,GridSystem.Cell target,string otherGrid) {
            GridManager.Instance.FindPath(GameManager.Instance.Players[player.PlayerID],target.PositionInGrid);

            int[] positions = GridManager.Instance.SerializePathData();

            this.photonView.RPC("RPC_RespondWithProcessedPath",RpcTarget.All,player.PlayerID,positions,otherGrid);
        }

        [PunRPC]
        public void RPC_RespondWithProcessedPath(object[] pathDatas) {
            PlayerBehavior movingPlayer = GameManager.Instance.Players[pathDatas[0].ToString()];
            // We manage the fact that 2 players won't obv be on the same grid, so we send the player
            movingPlayer.MoveWithPath(GridManager.Instance.DeserializePathData(movingPlayer,(int[])pathDatas[1]),pathDatas[2].ToString());
        }

        public void PlayerAsksToEnterGrid(PlayerBehavior player,WorldGrid mainGrid,string targetGrid) {
            this.photonView.RPC("RPC_RespondToEnterGrid",RpcTarget.All,player.PlayerID,mainGrid.UName,targetGrid);
        }

        [PunRPC]
        public void RPC_RespondToEnterGrid(string playerID,string mainGrid,string targetGrid) {
            GameManager.Instance.FireEntityExitingGrid(playerID);

            if (!GridManager.Instance.WorldGrids[mainGrid].InnerCombatGrids.ContainsKey(targetGrid))
                Debug.LogError("Couldn't find mainGrid's inner grid called : " + targetGrid + ". Count of innerGrids is : " + GridManager.Instance.WorldGrids[mainGrid].InnerCombatGrids.Count);

            GameManager.Instance.Players[playerID].EnterNewGrid(GridManager.Instance.WorldGrids[mainGrid].InnerCombatGrids[targetGrid] as CombatGrid);

            GameManager.Instance.FireEntityEnteredGrid(playerID);
        }

        // /!\ Only one combat can be active at the moment, that is important
        public void PlayerAsksToStartCombat() {
            this.photonView.RPC("RPC_RespondToStartCombat",RpcTarget.All,GameManager.Instance.SelfPlayer.PlayerID);
        }

        [PunRPC]
        public void RPC_RespondToStartCombat(string playerID) {
            CombatManager.Instance.StartCombat(GameManager.Instance.Players[playerID].CurrentGrid as CombatGrid);
        }

        public void PlayerAsksToInteract(Cell interaction) {
            this.photonView.RPC("RPC_RespondToInteract",RpcTarget.All,GameManager.Instance.SelfPlayer.PlayerID,interaction.PositionInGrid.latitude,interaction.PositionInGrid.longitude);
        }

        [PunRPC]
        public void RPC_RespondToInteract(string playerID,int latitude,int longitude) {
            PlayerBehavior player = GameManager.Instance.Players[playerID];
            player.Interact(player.CurrentGrid.Cells[latitude,longitude]);
        }

        public void PlayerCanceledInteract(Cell interaction) {
            this.photonView.RPC("RPC_RespondCancelInteract",RpcTarget.All,GameManager.Instance.SelfPlayer.PlayerID,interaction.PositionInGrid.latitude,interaction.PositionInGrid.longitude);
        }

        [PunRPC]
        public void RPC_RespondCancelInteract(string playerID,int latitude,int longitude) {
            PlayerBehavior player = GameManager.Instance.Players[playerID];
            player.Interact(player.CurrentGrid.Cells[latitude,longitude]);
        }

        public void GiftOrRemovePlayerItem(string playerID,ItemPreset item,int quantity) {
            this.photonView.RPC("RPC_RespondGiftOrRemovePlayerItem",RpcTarget.All,GameManager.Instance.SelfPlayer.PlayerID,item.UID.ToString(),quantity);

        }

        [PunRPC]
        public void RPC_RespondGiftOrRemovePlayerItem(string playerID,string itemID,int quantity) {
            GameManager.Instance.Players[playerID].TakeResources(GridManager.Instance.ItemsPresets[System.Guid.Parse(itemID)],quantity);
        }
        public void TurnBegan(string playerID) {
            this.photonView.RPC("RPC_OnTurnBegan",RpcTarget.All,playerID);
        }
        [PunRPC]
        public void RPC_OnTurnBegan(string playerID) {
            if (CombatManager.Instance.PlayingEntities.Any(x => x == GameManager.Instance.SelfPlayer)) {
                //If there is the client player in the battle;
                if (playerID == GameManager.Instance.SelfPlayer.PlayerID) {
                    //If it is OUR turn;
                    UIManager.Instance.NextTurnButton.interactable = true;
                }
            }
        }
        public void CastSpell(ScriptableCard spellToCast) {
            this.photonView.RPC("RPC_CastSpell",RpcTarget.All,spellToCast.name);
        }
        [PunRPC]
        public void RPC_CastSpell(string spellName, int longitude, int latittude) {
            var card = CardsManager.Instance.ScriptableCards[spellName];
            if (card != null) {
                CombatManager.Instance.ExecuteSpells(CombatManager.Instance.CurrentPlayingGrid.Cells[longitude,latittude],card);
            }
        }
        #endregion

        #region Photon_UI_callbacks

        public override void OnRoomListUpdate(List<RoomInfo> roomList) {
            this.UILobby?.UpdateRoomList(roomList);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer,ExitGames.Client.Photon.Hashtable changedProps) {
            this.UILobby?.UpdatePlayersFromProperties(targetPlayer);
        }

        public override void OnJoinedRoom() {
            if (this.UILobby != null)
                this.UILobby?.OnJoinedRoom();
            else
                GameManager.Instance.WelcomePlayers();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer) {
            this.UILobby?.UpdatePlayersList();
        }

        public override void OnLeftRoom() {
            this.UILobby?.OnSelfLeftRoom();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer) {
            this.UILobby?.OnPlayerLeftRoom();
        }

        public override void OnConnectedToMaster() {
            Debug.Log("Connected to master serv");
            if (this.UILobby != null)
                PhotonNetwork.JoinLobby();
            else
                GameManager.Instance.WelcomePlayerLately();
        }

        public override void OnDisconnected(DisconnectCause cause) {
            Debug.Log("Disconnected");
        }

        public override void OnJoinedLobby() {
            base.OnJoinedLobby();
            Debug.Log("Joined Lobby !");
        }

        #endregion
    }

}
