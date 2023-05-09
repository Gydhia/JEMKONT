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
using DownBelow.UI.Menu;
using Newtonsoft.Json;

namespace DownBelow.Managers
{
    public enum DisconnectTarget
    {
        None = 0,
        ToConnection = 1,
        ToOfflineMode = 2,
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
        public event GameEventData.Event OnInternetReached;
        public event GameEventData.Event OnInternetLost;

        public bool HasInternet;

        private string sharedSaveBuffer;
        private int endWrapCount = 0;

        public MenuPopup_Lobby UILobby;
        public MenuPopup_Room UIRoom;

        protected DisconnectTarget DisconnectCallback = DisconnectTarget.None; 

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

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                this.HasInternet = true;
            }
            else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                this.HasInternet = false;
            }
        }

        public void Init()
        {

        }

        private void Update()
        {
            if (this.HasInternet && Application.internetReachability == NetworkReachability.NotReachable)
            {
                this.HasInternet = false;

                this.OnInternetLost?.Invoke(null);
            }
            else if (!this.HasInternet && 
                (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || 
                 Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork))
            {
                this.HasInternet = true;

                this.OnInternetReached?.Invoke(null);
            }

        }

        public void UpdateOwnerName(string newName)
        {
            PhotonNetwork.NickName = newName;
        }

        public void SwitchConnectionState(bool connect)
        {
            PhotonNetwork.Disconnect();
            if (connect && !PhotonNetwork.IsConnected)
            {
                this.DisconnectCallback = DisconnectTarget.ToConnection;
            }
            else if (!PhotonNetwork.OfflineMode)
            {
                this.DisconnectCallback = DisconnectTarget.ToOfflineMode;
            }
        }

        private void Start()
        {
            this.Connect();
        }

        public void Connect()
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public void TryReconnect()
        {
            if(!PhotonNetwork.IsConnected && !PhotonNetwork.InLobby)
            {
                this.Connect();
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("Disconnected");
            this.onSwitchedConnectionState();

            if(MenuManager.Instance != null)
            {
                MenuManager.Instance.SwitchConnectionAspect(false);
            }
        }

        protected void onSwitchedConnectionState()
        {
            PhotonNetwork.LeaveLobby();

            switch (this.DisconnectCallback)
            {
                case DisconnectTarget.ToConnection:
                    this.Connect();
                    break;
                case DisconnectTarget.ToOfflineMode:
                    PhotonNetwork.PhotonServerSettings.StartInOfflineMode = true;
                    PhotonNetwork.ConnectUsingSettings();

                    //PhotonNetwork.OfflineMode = true;
                    break;
            }

            this.DisconnectCallback = DisconnectTarget.None;
        }

        #region UI_calls
        public void ClickOnStart()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //PhotonNetwork.CurrentRoom.IsOpen = false;
                //PhotonNetwork.CurrentRoom.IsVisible = false;
               
                PhotonNetwork.LoadLevel("0_FarmLand");
            }
        }

        public void ShareSaveThroughRoom()
        {
            var saveFile = GameData.Game.RefGameDataContainer.SavegameFile;

            var textReader = saveFile.OpenText();
            long textLenght = textReader.BaseStream.Length;
            int counter = 0;

            while (counter < textLenght)
            {
                char[] buffer = new char[12288];
                int toRead = 12288;

                if(textLenght - counter < 12288)
                {
                    toRead = (int)(textLenght - counter);
                }
            
                textReader.ReadBlock(buffer, 0, toRead);
                counter += toRead;

                this.photonView.RPC("OnReceivedSharedSavePart", RpcTarget.All, new string(buffer));
            }

            textReader.Close();

            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                this.ClickOnStart();
            }
            else
            {
                this.photonView.RPC("WrapSaveParts", RpcTarget.All);
            }
        }

        [PunRPC]
        public void OnReceivedSharedSavePart(string savePart)
        {
            this.sharedSaveBuffer += savePart;
        }

        [PunRPC]
        public void WrapSaveParts()
        {
            GameData.Game.RefGameDataContainer = GameData.GameDataContainer.LoadSharedJson(this.sharedSaveBuffer);

            this.sharedSaveBuffer = string.Empty;

            this.photonView.RPC("ReceiveEndWrap", RpcTarget.MasterClient);
        }
        [PunRPC]
        public void ReceiveEndWrap()
        {
            this.endWrapCount++;

            // All players excluding ourselves
            if(this.endWrapCount >= PhotonNetwork.CurrentRoom.PlayerCount - 1)
            {
                this.ClickOnStart();
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

        public void CreateRoom(string roomName)
        {
            if(PhotonNetwork.CurrentRoom != null)
            {
                Debug.LogError("Trying to create a room while already in a room : " + PhotonNetwork.CurrentRoom);
                return;
            }
            if (!string.IsNullOrEmpty(roomName))
            {
                PhotonNetwork.CreateRoom(roomName, new RoomOptions() { MaxPlayers = 4, BroadcastPropsChangeToAll = true, PublishUserId = true }, null);
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
            this.UIRoom?.UpdatePlayersFromProperties(targetPlayer);
        }

        public override void OnJoinedRoom()
        {
            if (this.UIRoom != null)
            {
                MenuManager.Instance.SelectPopup(MenuPopup.Room);
                this.UIRoom.OnJoinedRoom();
            }
            else
            {
                // We go here only if starting from game scene
                GameData.Game.RefGameDataContainer = GameManager.MakeBaseGame("DownBelowBase");

                GridManager.Instance.CreateWholeWorld(GameData.Game.RefGameDataContainer);
                GameManager.Instance.ProcessPlayerWelcoming();
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            this.UIRoom?.UpdatePlayersList();
            this.UIRoom?.UpdatePlayersState();
        }

        public override void OnLeftRoom()
        {
            this.UIRoom?.OnSelfLeftRoom();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            this.UIRoom?.OnPlayerLeftRoom();
        }

        public override void OnConnectedToMaster()
        {
            if (this.UILobby != null)
            {
                PhotonNetwork.JoinLobby();
                MenuManager.Instance.SwitchConnectionAspect(true);
            }
            else
            {
                GameManager.Instance.WelcomePlayerLately();
            }
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
        }

        public override void OnLeftLobby()
        {
            base.OnLeftLobby();
        }

        public void DebugConnection()
        {
            string connected = "Is Connected = " + PhotonNetwork.IsConnected + "\n";
            string offline = "OfflineMode = " + PhotonNetwork.OfflineMode + "\n";
            string inLobby = "Is In Lobby = " + PhotonNetwork.InLobby + "\n";
            string currLobby = "Current Lobby = " + (PhotonNetwork.CurrentLobby == null ? "NONE" : PhotonNetwork.CurrentLobby.Name) + "\n";
            string currRoom = "Current Room = " + (PhotonNetwork.CurrentRoom == null ? "NONE" : PhotonNetwork.CurrentRoom.Name);

            Debug.LogError("Current Photon State : \n" + connected + offline + inLobby + currLobby + currRoom);
        }
        #endregion
    }

}
