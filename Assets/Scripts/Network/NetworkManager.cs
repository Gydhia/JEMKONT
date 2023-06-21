using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Linq;
using DownBelow.Mechanics;
using DownBelow.Events;
using System;
using Newtonsoft.Json;
using DownBelow.Loading;

namespace DownBelow.Managers
{
    public enum DisconnectTarget
    {
        None = 0,
        ToConnection = 1,
        ToOfflineMode = 2,

        ToPlaySolo = 3,

        DisconnectToSolo = ToOfflineMode & ToPlaySolo
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
       

        public SerializedAction(string actionID, string contextAction, string entityID, string grid, Cell targetCell, string actionType, object[] datas)
        {
            this.ActionID = actionID;
            this.ContextAction = contextAction;
            this.EntityID = entityID;
            this.GridName = grid;
            this.ActionType = actionType;
            this.Datas = datas;
            this.GridLocation = targetCell == null ?
                null : 
                new int[2] { targetCell.PositionInGrid.latitude, targetCell.PositionInGrid.longitude };
            this.Datas = datas;
        }
    }

    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public event GameEventData.Event OnInternetReached;
        public event GameEventData.Event OnInternetLost;

        private bool _inited = false;

        public bool HasInternet;

        private string sharedSaveBuffer;
        private int endWrapCount = 0;

        protected DisconnectTarget DisconnectCallback = DisconnectTarget.None; 

        public static NetworkManager Instance;

        public bool IsLocalPlayer(PlayerBehavior player) => player.UID == PhotonNetwork.LocalPlayer.UserId;

        /// <summary>
        /// Players for whom playing entity has ended its turn
        /// </summary>
        private List<string> _playersNetState = new List<string>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                this.transform.parent = null;

                DontDestroyOnLoad(this.gameObject);

                // SUPER MEGA IMPORTANT. photonView isn't reliable with DontDestroyOnLoad, so we're adding it manually at the first instantiation.
                if(this.photonView == null)
                {
                    var newView = this.gameObject.AddComponent<PhotonView>();

                    newView.OwnershipTransfer = OwnershipOption.Fixed;
                    newView.ViewID = PhotonNetwork.AllocateViewID(0);
                }
                else
                {
                    Debug.LogError("NetworkManager already has a photonView at first Instantiation. This shouldn't occur since it'll collide by returning to the loaded scene");
                }
            }
            else
            {
#if UNITY_EDITOR
                DestroyImmediate(this.gameObject, false);
#else
                Destroy(this.gameObject);
#endif
            }

            
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
            this._inited = true;
        }

        public void SelfLoadedGame()
        {
            this.photonView.RPC("RPC_SelfLoadedGame", RpcTarget.AllBuffered, GameManager.RealSelfPlayer.UID);
        }

        [PunRPC]
        public void RPC_SelfLoadedGame(string PlayerID)
        {
            this._playersNetState.Add(PlayerID);

            // Hide loading screen ONLY when all players have loaded the scene
            if (this._playersNetState.Count >= GameManager.Instance.Players.Count)
            {
                this._playersNetState.Clear();
                LoadingScreen.Instance.Hide();
            }
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

        public void SwitchConnectionState(bool connect, bool toPlay = false)
        {
            PhotonNetwork.Disconnect();
            if (connect && !PhotonNetwork.IsConnected)
            {
                this.DisconnectCallback = DisconnectTarget.ToConnection;
            }
            else if (!PhotonNetwork.OfflineMode)
            {
                if (toPlay)
                {
                    this.DisconnectCallback = DisconnectTarget.ToPlaySolo;
                }
                else
                {
                    this.DisconnectCallback = DisconnectTarget.ToOfflineMode;
                }
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
                
            // Means that we have no internet connection, and we're launching directly
            if (cause == DisconnectCause.DnsExceptionOnConnect && MenuManager.Instance == null)
            {
                this.DisconnectCallback = DisconnectTarget.ToPlaySolo;
            }

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
                    PhotonNetwork.OfflineMode = true;
                    PhotonNetwork.ConnectUsingSettings();
                    break;
                case DisconnectTarget.ToPlaySolo:
                    PhotonNetwork.OfflineMode = true;
                    //PhotonNetwork.ConnectUsingSettings();
                    this.ClickOnStart();
                    break;
            }

            this.DisconnectCallback = DisconnectTarget.None;
        }

        #region UI_calls
        public void ClickOnStart()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!PhotonNetwork.InRoom)
                {
                    PhotonNetwork.JoinRandomOrCreateRoom();
                }
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;

                this.LoadScene(LevelName.FarmLand);
            }
            else
            {
                this.SwitchConnectionState(false, true);
            }
        }

        public void ShareSaveThroughRoom()
        {
            // No need to share files when alone
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                this.ClickOnStart();
                return;
            }

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

            this.photonView.RPC("WrapSaveParts", RpcTarget.All);   
        }

        [PunRPC]
        public void OnReceivedSharedSavePart(string savePart)
        {
            // As a client, we wanna show the loading screen when receiving datas
            LoadingScreen.Instance.Show();

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

        public void EntityAskToBuffSpell(Spells.SpellHeader spellHeader)
        {
            string parsedHeader = JsonConvert.SerializeObject(spellHeader);
         
            this.photonView.RPC("RPC_OnEntityAskedBuffSpell", RpcTarget.All, parsedHeader);
        }

        [PunRPC]
        public void RPC_OnEntityAskedBuffSpell(string header)
        {
            Spells.SpellHeader spellHeader = JsonConvert.DeserializeObject<Spells.SpellHeader>(header);

            ScriptableCard refCard = SettingsManager.Instance.ScriptableCards[spellHeader.RefCard];

            Spells.Spell[] buffedSpells = new Spells.Spell[refCard.Spells.Length];
            Array.Copy(refCard.Spells, buffedSpells, refCard.Spells.Length);


            WorldGrid currGrid = CombatManager.CurrentPlayingGrid;
            CharacterEntity caster = CombatManager.Instance.PlayingEntities.Single(pe => pe.UID == spellHeader.CasterID);

            for (int i = 0; i < refCard.Spells.Length; i++)
            {
                buffedSpells[i].TargetCell = currGrid.GetCell(spellHeader.TargetedCells[i]);
                buffedSpells[i].SpellHeader = spellHeader;
                buffedSpells[i].RefEntity = caster;
                if (i > 0) buffedSpells[i].ParentSpell = buffedSpells[i - 1];
                GameManager.Instance.BuffAction(buffedSpells[i], false);
            }
        }


        /// <summary>
        /// Used to tick a pendular action. This is for actions that requires multiple actions from inputs before ending
        /// </summary>
        /// <remarks> By design, the pendular action must be the current effective one to be ticked </remarks>
        /// <param name="action"></param>
        public void EntityAskToTickAction(EntityAction action, bool result)
        {
            this.photonView.RPC("RPC_EntityRespondToTickAction", RpcTarget.All, action.RefEntity.UID, result);
        }

        [PunRPC]
        public void RPC_EntityRespondToTickAction(string playerID, bool result)
        {
            // For now, pendular actions must be out of combat ones
            var player = GameManager.Instance.Players[playerID];
            var onGoingAction = GameManager.NormalActionsBuffer[player][0];

            if(onGoingAction is PendularAction pendularAction)
            {
                pendularAction.LocalTick(result);
            }
        }

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
                action.TargetCell,
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
                         action.TargetCell,
                         action.GetAssemblyName(),
                         action.GetDatas()
                     );

                    index++;
                }
            }
            this.EntityAskToBuffAction(actionArray, !InputManager.Instance.IsPressingShift);
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
                Cell targetCell = pActions[i].GridLocation == null ? null : entityGrid.Cells[pActions[i].GridLocation[0], pActions[i].GridLocation[1]];
                CharacterEntity entity = entityGrid.GridEntities.Single(e => e.UID == pActions[i].EntityID);

                pActions[i].Datas ??= new object[0];

                Type type = Type.GetType(pActions[i].ActionType);
                EntityAction myAction = Activator.CreateInstance(type, new object[2] { entity, targetCell }) as EntityAction;
                myAction.RefGrid = entityGrid;

                myAction.ID = Guid.Parse(pActions[i].ActionID);
                myAction.ContextActionId = Guid.Parse(pActions[i].ContextAction);

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
            {
                GameManager.Instance.BuffAction(action, abortOthers);
                abortOthers = false;
            }
        }


        // /IMPORTANT\ REPLUG IT KILLIAN PLEASE
        //// TODO: For now we only need to notify the UIManager, think about creating an event later if there are further needs
        //if (GameManager.SelfPlayer == movingPlayer)
        //    UIManager.Instance.PlayerMoved();


        public void PlayerAskToLeaveCombat()
        {
            this.photonView.RPC("RPC_RespondMasterToLeaveCombat", RpcTarget.MasterClient, GameManager.SelfPlayer.UID);
        }

        [PunRPC]
        public void RPC_RespondMasterToLeaveCombat(string PlayerID)
        {
            this._playersNetState.Add(PlayerID);

            if(this._playersNetState.Count >= GameManager.Instance.Players.Values.Count(p => p.CurrentGrid.IsCombatGrid))
            {
                this.photonView.RPC("RPC_RespondPlayersToLeaveCombat", RpcTarget.All, GameManager.SelfPlayer.UID);
            }
        }
        [PunRPC]
        public void RPC_RespondPlayersToLeaveCombat(string PlayerID)
        {
            UIManager.Instance.RewardSection.EnableContinue();
        }


         // /!\ Only one combat can be active at the moment, that is important
         public void PlayerAsksToStartCombat()
        {
            this.photonView.RPC("RPC_RespondToStartCombat", RpcTarget.All, GameManager.SelfPlayer.UID);
        }

        [PunRPC]
        public void RPC_RespondToStartCombat(string playerID)
        {
            CombatManager.Instance.StartCombat(GameManager.Instance.Players[playerID].CurrentGrid as CombatGrid);
        }

        public void GiftOrRemovePlayerItem(string playerID, ItemPreset item, int quantity, int preferedSlot = -1)
        {
            this.photonView.RPC("RPC_RespondGiftOrRemovePlayerItem", RpcTarget.All, GameManager.SelfPlayer.UID, item.UID.ToString(), quantity, preferedSlot);
        }

        [PunRPC]
        public void RPC_RespondGiftOrRemovePlayerItem(string playerID, string itemID, int quantity, int preferedSlot = -1)
        {
            var storage = GameManager.RealSelfPlayer.PlayerInventory;
            var item = SettingsManager.Instance.ItemsPresets[System.Guid.Parse(itemID)];

            if (quantity > 0)
            {
                storage.TryAddItem(item, quantity, preferedSlot);
            }
            else
            {
                storage.RemoveItem(item, -quantity, preferedSlot);
            }
        }

        public void GiftOrRemoveStorageItem(InteractableStorage storage, ItemPreset item, int quantity, int slot)
        {
            this.photonView.RPC("RPC_RespondGiftOrRemoveStorageItem", RpcTarget.All,
                storage.RefCell.RefGrid.UName, storage.RefCell.Datas.heightPos, storage.RefCell.Datas.widthPos, item.UID.ToString(), quantity, slot);
        }

        [PunRPC]
        public void RPC_RespondGiftOrRemoveStorageItem(string gridname, int latitude, int longitude, string itemID, int quantity, int slot)
        {
            var grid = GridManager.Instance.WorldGrids[gridname];
            var storage = (grid.Cells[latitude, longitude].AttachedInteract as InteractableStorage).Storage;
            var item = SettingsManager.Instance.ItemsPresets[System.Guid.Parse(itemID)];

            if(quantity > 0)
            {
                storage.TryAddItem(item, quantity, slot);
            }
            else
            {
                storage.RemoveItem(item, -quantity, slot);
            }
        }
        #region TURNS


        public void StartEntityTurn()
        {
            this._playersNetState.Clear();

            this.photonView.RPC("NotifyPlayerStartTurn", RpcTarget.All);
        }

        [PunRPC]
        public void NotifyPlayerStartTurn()
        {
            CombatManager.Instance.ProcessStartTurn();

            CombatManager.CurrentPlayingEntity.StartTurn();

            this.photonView.RPC("PlayerAnswerStartTurn", RpcTarget.MasterClient, GameManager.RealSelfPlayer.UID);
        }

        [PunRPC]
        public void PlayerAnswerStartTurn(string PlayerID)
        {
            this._playersNetState.Add(PlayerID);

            // When all players have started the playing entity's turn, create the actions IF it's an enemy
            if (this._playersNetState.Count >= GameManager.Instance.Players.Count)
            {
                this._playersNetState.Clear();

                if (CombatManager.CurrentPlayingEntity is EnemyEntity enemy)
                {
                    this.EntityAskToBuffActions(enemy.CreateEnemyActions());
                }
            }
        }

        /// <summary>
        /// From the player to end the turn of himself /!\ OR /!\ its current playing entity. NOT for other players
        /// </summary>
        public void PlayerAsksEndTurn()
        {
            this.photonView.RPC("RespondMasterEndEntityTurn", RpcTarget.MasterClient, GameManager.RealSelfPlayer.UID);
        }

        [PunRPC]
        public void RespondMasterEndEntityTurn(string PlayerID)
        {
            this._playersNetState.Add(PlayerID);

            if (this._playersNetState.Count >= GameManager.Instance.Players.Count)
            {
                this.photonView.RPC("RespondPlayerEndEntityTurn", RpcTarget.All, PlayerID);
            }
        }

        [PunRPC]
        public void RespondPlayerEndEntityTurn(string PlayerID)
        {
            this._playersNetState.Clear();

            // Each player process it 
            CombatManager.Instance.ProcessEndTurn();

            this.photonView.RPC("ConfirmPlayerEndedTurn", RpcTarget.MasterClient, GameManager.RealSelfPlayer.UID);
        }

        [PunRPC]
        public void ConfirmPlayerEndedTurn(string PlayerID)
        {
            this._playersNetState.Add(PlayerID);

            if (this._playersNetState.Count >= GameManager.Instance.Players.Count)
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
            MenuManager.Instance?.UILobby?.UpdateRoomList(roomList);
        }


        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            MenuManager.Instance?.UIRoom?.UpdatePlayersFromProperties(targetPlayer);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("JOINED ROOM");
           
            if (MenuManager.Instance && MenuManager.Instance.UIRoom != null)
            {
                MenuManager.Instance.SelectPopup(MenuPopup.Room);
                MenuManager.Instance.UIRoom.OnJoinedRoom();
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
            MenuManager.Instance?.UIRoom?.UpdatePlayersList();
          //  MenuManager.Instance?.UIRoom?.UpdatePlayersState();
        }

        public override void OnLeftRoom()
        {
            MenuManager.Instance?.UIRoom?.OnSelfLeftRoom();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if(MenuManager.Instance && MenuManager.Instance.UIRoom != null)
            {
                MenuManager.Instance.UIRoom.OnPlayerLeftRoom();
            }
            else
            {
                // Everyone should leave the game
                if (otherPlayer.IsMasterClient)
                {
                    this.LoadScene(LevelName.MainMenu);
                }
            }
        }

        public override void OnConnectedToMaster()
        {
            if (MenuManager.Instance && MenuManager.Instance.UILobby != null)
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
            Debug.Log("JOINED LOBBY");
            base.OnJoinedLobby();
        }

        public override void OnLeftLobby()
        {
            Debug.Log("LEFT LOBBY");
            base.OnLeftLobby();
        }

        public void DebugConnection()
        {
            string connected = "Is Connected = " + PhotonNetwork.IsConnected + "\n";
            string offline = "OfflineMode = " + PhotonNetwork.OfflineMode + "\n";
            string inLobby = "Is In Lobby = " + PhotonNetwork.InLobby + "\n";
            string currLobby = "Current Lobby = " + (PhotonNetwork.CurrentLobby == null ? "NONE" : PhotonNetwork.CurrentLobby.Name) + "\n";
            string currRoom = "Current Room = " + (PhotonNetwork.CurrentRoom == null ? "NONE" : PhotonNetwork.CurrentRoom.Name) + "\n";
            string currServer = "Current server = " + PhotonNetwork.Server;

            Debug.LogError("Current Photon State : \n" + connected + offline + inLobby + currLobby + currRoom + currServer);
        }
        #endregion

        #region SCENE_LOADING

        public void LoadScene(LevelName levelName)
        {
            switch (levelName)
            {
                case LevelName.FarmLand:
                    PhotonNetwork.LoadLevel("0_FarmLand");
                    break;
                case LevelName.MainMenu:
                    // Empty the ref GameData to avoid problems while in mainmenu
                    GameData.Game.RefGameDataContainer = null;

                    // Leave the room to avoid useless synchronisation
                    PhotonNetwork.LeaveRoom();

                    // Locally load scene, since network has to be lost
                    UnityEngine.SceneManagement.SceneManager.LoadScene("UI_MainMenu");
                    break;
            }

            LoadingScreen.Instance.Show();
        }

        #endregion
    }

}
