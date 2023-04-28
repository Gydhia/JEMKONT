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
using DownBelow.Events;
using System;

namespace DownBelow.Managers
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

            UnityEngine.Object.DontDestroyOnLoad(this);
        }

        public void Init()
        {
            this._connect();

            this.SubToGameEvents();
        }

        public void SubToGameEvents()
        {
            CombatManager.Instance.OnTurnStarted += this.TurnBegan;
            CombatManager.Instance.OnTurnEnded += this.TurnEnded;
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

        /// <summary>
        /// Will ask to buff an action, ABORTING ANY OTHER ACROSS THE WAY. Check other parameters.
        /// </summary>
        /// <param name="action"> The Action to buff.</param>
        public void EntityAskToBuffAction(EntityAction action)
        {
            this.EntityAskToBuffAction(action, !InputManager.Instance.IsPressingShift);
        }

        /// <summary>
        /// Will buff several actions, not cancelling one another.
        /// </summary>
        /// <param name="actions"></param>
        public void EntityAskToBuffActions(EntityAction[] actions)
        {
            foreach (var item in actions)
            {
                if(item != null)
                    EntityAskToBuffAction(item, false);
            }
        }

        /// <summary>
        /// Will ask to buff an action. If abortOthers = true; will abort other actions in the buffer for the entity.
        /// </summary>
        /// <param name="action"> The Action to buff.</param>
        /// <param name="abortOthers">true if you want to abort any other action in process, false if you should just queue the action.</param>
        public void EntityAskToBuffAction(EntityAction action, bool abortOthers)
        {
            this.photonView.RPC("RPC_RespondWithProcessedBuffedAction", RpcTarget.All,
                action.RefEntity.UID,
                action.RefEntity.CurrentGrid.UName,
                new int[2] { action.TargetCell.PositionInGrid.latitude, action.TargetCell.PositionInGrid.longitude },
                action.GetType().ToString(),
                action.GetDatas(),
                abortOthers
             );
        }

        [PunRPC]
        public void RPC_RespondWithProcessedBuffedAction(string entityID, string grid, int[] gridLocation, string actionType, object[] datas, bool abortOthers)
        {
            WorldGrid entityGrid = GridManager.Instance.GetGridFromName(grid);
            Cell targetCell = entityGrid.Cells[gridLocation[0], gridLocation[1]];
            //If there are multiple CharacterEntities with the same UID; an exception will be thrown. Could use First/Find instead?
            CharacterEntity entity = entityGrid.GridEntities.Single(e => e.UID == entityID);

            object[] fullDatas = new object[datas.Length + 2];
            fullDatas[0] = entity;
            fullDatas[1] = targetCell;
            for (int i = 2;i < fullDatas.Length;i++)
                fullDatas[i] = datas[i - 2];

            Type type = Type.GetType(actionType);
            EntityAction myAction = Activator.CreateInstance(type, fullDatas) as EntityAction;

            myAction.RefEntity = entity;
            // 0 is latitude (height), 1 is longitude (width)
            myAction.TargetCell = targetCell;

            myAction.SetDatas(datas);

            GameManager.Instance.BuffAction(myAction, abortOthers);
        }


        // /IMPORTANT\ REPLUG IT KILLIAN PLEASE
        //// TODO: For now we only need to notify the UIManager, think about creating an event later if there are further needs
        //if (GameManager.Instance.SelfPlayer == movingPlayer)
        //    UIManager.Instance.PlayerMoved();


        // /!\ Only one combat can be active at the moment, that is important
        public void PlayerAsksToStartCombat()
        {
            this.photonView.RPC("RPC_RespondToStartCombat", RpcTarget.All, GameManager.Instance.SelfPlayer.PlayerID);
        }

        [PunRPC]
        public void RPC_RespondToStartCombat(string playerID)
        {
            CombatManager.Instance.StartCombat(GameManager.Instance.Players[playerID].CurrentGrid as CombatGrid);
        }

        public void GiftOrRemovePlayerItem(string playerID, ItemPreset item, int quantity)
        {
            this.photonView.RPC("RPC_RespondGiftOrRemovePlayerItem", RpcTarget.All, GameManager.Instance.SelfPlayer.PlayerID, item.UID.ToString(), quantity);
        }

        [PunRPC]
        public void RPC_RespondGiftOrRemovePlayerItem(string playerID, string itemID, int quantity)
        {
            GameManager.Instance.Players[playerID].TakeResources(GridManager.Instance.ItemsPresets[System.Guid.Parse(itemID)], quantity);
        }

        public void TurnBegan(EntityEventData EntityData)
        {
            if (!CombatManager.Instance.BattleGoing && GameManager.Instance.SelfPlayer.CurrentGrid.IsCombatGrid)
                return;

            this.photonView.RPC("RPC_OnTurnBegan", RpcTarget.All, EntityData.Entity.UID);
        }

        [PunRPC]
        public void RPC_OnTurnBegan(string entityID)
        {
            CombatManager.Instance.ProcessStartTurn(entityID);
            //If there is the client player in the battle;
            if (entityID == GameManager.Instance.SelfPlayer.PlayerID)
            {
                //If it is OUR turn;
                UIManager.Instance.NextTurnButton.interactable = true;
            }
        }

        public void TurnEnded(EntityEventData EntityData)
        {
            if (!CombatManager.Instance.BattleGoing && GameManager.Instance.SelfPlayer.CurrentGrid.IsCombatGrid)
                return;

            this.photonView.RPC("RPC_OnTurnEnded", RpcTarget.All, EntityData.Entity.UID);
        }

        [PunRPC]
        public void RPC_OnTurnEnded(string entityID)
        {
            CharacterEntity entity = CombatManager.Instance.PlayingEntities.Single(e => e.UID == entityID);

            CombatManager.Instance.ProcessEndTurn(entityID);
            //If there is the client player in the battle;
            if (entityID == GameManager.Instance.SelfPlayer.PlayerID)
            {
                //If it is OUR turn;
                UIManager.Instance.NextTurnButton.interactable = true;
            }
        }


        public void AskCastSpell(ScriptableCard spellToCast, Cell cell)
        {
            this.photonView.RPC("RPC_CastSpell", RpcTarget.All, spellToCast.UID.ToString(), cell.PositionInGrid.longitude, cell.PositionInGrid.latitude); ;
        }

        [PunRPC]
        public void RPC_CastSpell(string spellName, int longitude, int latittude)
        {
            ScriptableCard cardToPlay = CardsManager.Instance.ScriptableCards[System.Guid.Parse(spellName)];
            //if (cardToPlay != null)
            //    CombatManager.Instance.ExecuteSpells(CombatManager.Instance.CurrentPlayingGrid.Cells[latittude, longitude], cardToPlay);
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
            if (this.UILobby != null)
                this.UILobby?.OnJoinedRoom();
            else
                GameManager.Instance.WelcomePlayers();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            this.UILobby?.UpdatePlayersList();
            this.UILobby?.UpdatePlayersState();
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
            if (this.UILobby != null)
                PhotonNetwork.JoinLobby();
            else
                GameManager.Instance.WelcomePlayerLately();
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
