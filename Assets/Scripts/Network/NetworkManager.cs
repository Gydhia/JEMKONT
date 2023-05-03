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
    public enum BuffAnswer
    {
        EndTurn,
        StartTurn
    }

    [Serializable]
    public struct SerializedAction
    {
        public string ActionID;
        public string ContextAction;
        public string EntityID;
        public string GridName;
        public int[] GridLocation;
        public string ActionType;
        public object[] Datas;

        public SerializedAction(string actionID, string contextAction, string entityID, string grid, int[] gridLocation, string actionType, object[] datas)
        {
            this.ActionID = actionID;
            this.ContextAction = contextAction;
            this.EntityID = entityID;
            this.GridName = grid;
            this.ActionType = actionType;
            this.Datas = datas;
            this.GridLocation = gridLocation;
            this.Datas = datas;
        }
    }

    public class NetworkManager : MonoBehaviourPunCallbacks
    {

        public UIMenuLobby UILobby;

        public static NetworkManager Instance;

        public bool IsLocalPlayer(PlayerBehavior player) => player.UID == PhotonNetwork.LocalPlayer.UserId;

        /// <summary>
        /// Players for whom playing entity has ended its turn
        /// </summary>
        private List<PlayerBehavior> _playersTurnState = new List<PlayerBehavior>();

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

        private void Start()
        {
            this._connect();
        }

        public void Init()
        {
            this.SubToCombatEvents();
        }

        public void SubToCombatEvents()
        {
            // Only the master client should handle the turns processing
            if (PhotonNetwork.IsMasterClient)
            {
                //CombatManager.Instance.OnTurnEnded += ;
            }
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
            
            SerializedAction actionData = new SerializedAction(action.ID.ToString(),
                action.ContextActionId.ToString(),
                action.RefEntity.UID,
                action.RefEntity.CurrentGrid.UName,
                new int[2] { action.TargetCell.PositionInGrid.latitude, action.TargetCell.PositionInGrid.longitude },
                action.GetType().ToString(),
                action.GetDatas()
                );

            this.EntityAskToBuffAction(new SerializedAction[1] { actionData }, !InputManager.Instance.IsPressingShift);
        }

        /// <summary>
        /// Will buff several actions, not cancelling one another.
        /// </summary>
        /// <param name="actions"></param>
        public void EntityAskToBuffActions(EntityAction[] actions)
        {
            SerializedAction[] actionArray = new SerializedAction[actions.Where(a => a != null).Count()];
            int index = 0;
            foreach (var action in actions)
            {
                if (action != null)
                {
                    actionArray[index] = new SerializedAction(action.ID.ToString(),
                         action.ContextActionId.ToString(),
                         action.RefEntity.UID,
                         action.RefEntity.CurrentGrid.UName,
                         new int[2] { action.TargetCell.PositionInGrid.latitude, action.TargetCell.PositionInGrid.longitude },
                         action.GetType().ToString(),
                         action.GetDatas()
                     );

                    index++;
                }
            }
            this.EntityAskToBuffAction(actionArray, false);
        }

        /// <summary>
        /// Will ask to buff an action. If abortOthers = true; will abort other actions in the buffer for the entity.
        /// </summary>
        /// <param name="action"> The Action to buff.</param>
        /// <param name="abortOthers">true if you want to abort any other action in process, false if you should just queue the action.</param>
        public void EntityAskToBuffAction(SerializedAction[] actions, bool abortOthers)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(actions);

            this.photonView.RPC("RPC_RespondWithProcessedBuffedAction", RpcTarget.All, json, abortOthers);
        }

        [PunRPC]
        public void RPC_RespondWithProcessedBuffedAction(string actions, bool abortOthers)
        {
            SerializedAction[] pActions = Newtonsoft.Json.JsonConvert.DeserializeObject<SerializedAction[]>(actions);
            List<EntityAction> generatedActions = new List<EntityAction>();

            for (int i = 0; i < pActions.Length; i++)
            {
                WorldGrid entityGrid = GridManager.Instance.GetGridFromName(pActions[i].GridName);
                Cell targetCell = entityGrid.Cells[pActions[i].GridLocation[0], pActions[i].GridLocation[1]];
                CharacterEntity entity = entityGrid.GridEntities.Single(e => e.UID == pActions[i].EntityID);

                pActions[i].Datas ??= new object[0];
                object[] fullDatas = new object[2 + pActions[i].Datas.Length];

                fullDatas[0] = entity;
                fullDatas[1] = targetCell;
                for (int j = 2; j < fullDatas.Length; j++)
                {
                    fullDatas[j] = pActions[i].Datas[j - 2];
                }

                Type type = Type.GetType(pActions[i].ActionType);
                EntityAction myAction = Activator.CreateInstance(type, fullDatas) as EntityAction;

                myAction.ID = Guid.Parse(pActions[i].ActionID);
                myAction.ContextActionId = Guid.Parse(pActions[i].ContextAction);
                myAction.RefEntity = entity;
                myAction.TargetCell = targetCell;

                myAction.SetDatas(pActions[i].Datas);
                generatedActions.Add(myAction);
            }

            foreach (var action in generatedActions)
            {
                if (action.ContextActionId != Guid.Empty)
                {
                    var foundAction = generatedActions.SingleOrDefault(a => a.ID == action.ContextActionId);
                    if (foundAction != null)
                        action.SetContextAction(foundAction);
                    else
                        Debug.LogError("Couldn't find context action of id [" + action.ContextActionId + "] in the generated datas");
                }
            }

            foreach (var action in generatedActions)
                GameManager.Instance.BuffAction(action, abortOthers);
        }


        // /IMPORTANT\ REPLUG IT KILLIAN PLEASE
        //// TODO: For now we only need to notify the UIManager, think about creating an event later if there are further needs
        //if (GameManager.Instance.SelfPlayer == movingPlayer)
        //    UIManager.Instance.PlayerMoved();


        // /!\ Only one combat can be active at the moment, that is important
        public void PlayerAsksToStartCombat()
        {
            this.photonView.RPC("RPC_RespondToStartCombat", RpcTarget.All, GameManager.Instance.SelfPlayer.UID);
        }

        [PunRPC]
        public void RPC_RespondToStartCombat(string playerID)
        {
            CombatManager.Instance.StartCombat(GameManager.Instance.Players[playerID].CurrentGrid as CombatGrid);
        }

        public void GiftOrRemovePlayerItem(string playerID, ItemPreset item, int quantity)
        {
            this.photonView.RPC("RPC_RespondGiftOrRemovePlayerItem", RpcTarget.All, GameManager.Instance.SelfPlayer.UID, item.UID.ToString(), quantity);
        }

        [PunRPC]
        public void RPC_RespondGiftOrRemovePlayerItem(string playerID, string itemID, int quantity)
        {
            GameManager.Instance.Players[playerID].TakeResources(GridManager.Instance.ItemsPresets[System.Guid.Parse(itemID)], quantity);
        }

        #region TURNS


        public void StartEntityTurn()
        {
            this._playersTurnState.Clear();

            this.photonView.RPC("NotifyPlayerStartTurn", RpcTarget.All, GameManager.Instance.SelfPlayer.UID);
        }

        [PunRPC]
        public void NotifyPlayerStartTurn(string PlayerID)
        {
            CombatManager.Instance.ProcessStartTurn();

            //if (CombatManager.Instance.CurrentPlayingEntity is PlayerBehavior)
            //{
            //    CombatManager.Instance.CurrentPlayingEntity.StartTurn();
            //}
            CombatManager.Instance.CurrentPlayingEntity.StartTurn();
            this.photonView.RPC("PlayerAnswerStartTurn", RpcTarget.MasterClient, GameManager.Instance.SelfPlayer.UID);
        }

        [PunRPC]
        public void PlayerAnswerStartTurn(string PlayerID)
        {
            this._playersTurnState.Add(GameManager.Instance.Players[PlayerID]);

            if (this._playersTurnState.Count >= GameManager.Instance.Players.Count)
            {
                this.NotifyEnemyActions();
            }
        }

        public void NotifyEnemyActions()
        {
            var entity = CombatManager.Instance.CurrentPlayingEntity;

            if (entity is EnemyEntity enemy)
            {
                this.EntityAskToBuffActions(enemy.CreateEnemyActions());
            }
        }

        /// <summary>
        /// From the player to end the turn of himself /!\ OR /!\ its current playing entity. NOT for other players
        /// </summary>
        public void PlayerAsksEndTurn()
        {
            this.photonView.RPC("RespondMasterEndEntityTurn", RpcTarget.MasterClient, GameManager.Instance.SelfPlayer.UID);
        }

        [PunRPC]
        public void RespondMasterEndEntityTurn(string PlayerID)
        {
            this._playersTurnState.Add(GameManager.Instance.Players[PlayerID]);

            if (this._playersTurnState.Count >= GameManager.Instance.Players.Count)
            {
                this.photonView.RPC("RespondPlayerEndEntityTurn", RpcTarget.All, PlayerID);
            }
        }

        [PunRPC]
        public void RespondPlayerEndEntityTurn(string PlayerID)
        {
            this._playersTurnState.Clear();

            // Each player process it 
            CombatManager.Instance.ProcessEndTurn();

            this.photonView.RPC("ConfirmPlayerEndedTurn", RpcTarget.MasterClient, GameManager.Instance.SelfPlayer.UID);
        }

        [PunRPC]
        public void ConfirmPlayerEndedTurn(string EntityID)
        {
            this._playersTurnState.Add(GameManager.Instance.Players[EntityID]);

            if (this._playersTurnState.Count >= GameManager.Instance.Players.Count)
            {
                this.StartEntityTurn();
            }
        }



        // TODO : Make a generic answer architecture for network such as EntityActions
        /*
        protected List<string> answersBuffer = new List<string>();

        public void BuffExpectedAnswer(string functionName)
        {
            this.answersBuffer.Add(functionName);
        }

        [PunRPC]
        public void WaitForAnswers(string PlayerID)
        {

        }
        */
        #endregion
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
