using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DownBelow.Managers
{
    public class GameManager : _baseManager<GameManager>
    {
        #region EVENTS
        public event GameEventData.Event OnGameStarted;

        public event EntityEventData.Event OnEnteredGrid;
        public event EntityEventData.Event OnExitingGrid;
        public event EntityEventData.Event OnSelfPlayerSwitched;

        public void FireGameStarted()
        {
            this.OnGameStarted?.Invoke(new());
        }

        public void FireEntityEnteredGrid(string entityID)
        {
            this.FireEntityEnteredGrid(this.Players[entityID]);
        }
        public void FireEntityEnteredGrid(CharacterEntity entity)
        {
            this.OnEnteredGrid?.Invoke(new EntityEventData(entity));
        }

        public void FireEntityExitingGrid(string entityID)
        {
            this.FireEntityExitingGrid(this.Players[entityID]);
        }
        public void FireEntityExitingGrid(CharacterEntity entity)
        {
            OnExitingGrid?.Invoke(new EntityEventData(entity));
        }
        public void FireSelfPlayerSwitched(PlayerBehavior player)
        {
            this.SelfPlayer.SelectedIndicator.gameObject.SetActive(false);
            this.SelfPlayer = player != null ? player : this.RealSelfPlayer;
            this.SelfPlayer.SelectedIndicator.gameObject.SetActive(true);

            OnSelfPlayerSwitched?.Invoke(new EntityEventData(this.SelfPlayer));
        }
        #endregion

        public string SaveName;
        public PlayerBehavior PlayerPrefab;

        public Dictionary<string, PlayerBehavior> Players;
        /// <summary>
        /// The local player or its current FakePlayer
        /// </summary>
        public PlayerBehavior SelfPlayer;
        /// <summary>
        /// The local player
        /// </summary>
        public PlayerBehavior RealSelfPlayer { get { return this.SelfPlayer.IsFake ? this.SelfPlayer.Owner : this.SelfPlayer; } }


        public ItemPreset[] GameItems;

        public static bool GameStarted = false;

        #region Players_Actions_Buffer

        public static Dictionary<CharacterEntity, List<EntityAction>> NormalActionsBuffer =
            new Dictionary<CharacterEntity, List<EntityAction>>();
        public static Dictionary<CharacterEntity, bool> IsUsingNormalBuffer =
            new Dictionary<CharacterEntity, bool>();

        public static List<EntityAction> CombatActionsBuffer = new List<EntityAction>();
        public static bool IsUsingCombatBuffer = false;
        

        #endregion

        private void Start()
        {
            this.Init();
        }

        public void Init()
        {
            if (NetworkManager.Instance != null)
                NetworkManager.Instance.Init();

            if(MenuManager.Instance != null)
                MenuManager.Instance.Init();

            if (CombatManager.Instance != null)
                CombatManager.Instance.Init();

            if (UIManager.Instance != null)
                UIManager.Instance.Init();

            if (CardsManager.Instance != null)
                CardsManager.Instance.Init();

            if (ToolsManager.Instance != null)
                ToolsManager.Instance.Init();

            if (GridManager.Instance != null)
                GridManager.Instance.Init();


            if(GameData.Game.RefGameDataContainer != null)
            {
                GridManager.Instance.CreateWholeWorld(GameData.Game.RefGameDataContainer);
                this.ProcessPlayerWelcoming();
            }
        }

        public void ProcessPlayerWelcoming()
        {
            CombatManager.Instance.OnCombatStarted += this._subscribeForCombatBuffer;

            if (PhotonNetwork.CurrentRoom == null)
            {
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.AutomaticallySyncScene = true;
            } 
            else if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.OfflineMode = true;
            } 
            else
            {
                WelcomePlayers();
            }
        }

        public void WelcomePlayerLately()
        {
            PhotonNetwork.CreateRoom("SoloRoom" + UnityEngine.Random.Range(0,100000));
        }

        public void WelcomePlayers()
        {
            if (PhotonNetwork.PlayerList.Length >= 1)
            {
                System.Guid spawnId = GridManager.Instance.SpawnablesPresets.First(k => k.Value is SpawnPreset).Key;
                var spawnLocations = GridManager.Instance.MainWorldGrid.SelfData.SpawnablePresets.Where(k => k.Value == spawnId).Select(kv => kv.Key);

                this.Players = new Dictionary<string, PlayerBehavior>();
                int counter = 0;
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    PlayerBehavior newPlayer = Instantiate(this.PlayerPrefab, Vector3.zero, Quaternion.identity, this.transform);
                    //newPlayer.Deck = CardsManager.Instance.DeckPresets.Values.Single(d => d.Name == "TestDeck").Copy();
                 
                    newPlayer.Init(GridManager.Instance.MainWorldGrid.Cells[spawnLocations.ElementAt(counter).latitude, spawnLocations.ElementAt(counter).longitude], GridManager.Instance.MainWorldGrid);
                    // TODO: make it works with world grids
                    newPlayer.UID = player.UserId;

                    if (player.UserId == PhotonNetwork.LocalPlayer.UserId)
                    {
                        this.SelfPlayer = newPlayer;
                        CameraManager.Instance.AttachPlayerToCamera(this.SelfPlayer);
                    }

                    this.Players.Add(player.UserId, newPlayer);

                    IsUsingNormalBuffer.Add(newPlayer, false);

                    counter++;
                }

                GameStarted = true;
                this.FireGameStarted();
            }
        }

        private void _subscribeForCombatBuffer(GridEventData Data)
        {
            CombatManager.Instance.OnCardEndUse += this.BuffSpell;

            CombatManager.Instance.OnCombatStarted -= this._subscribeForCombatBuffer;
            CombatManager.Instance.OnCombatEnded += this._unsubscribeForCombatBuffer;
        }

        private void _unsubscribeForCombatBuffer(GridEventData Data)
        {
            CombatManager.Instance.OnCardEndUse -= this.BuffSpell;

            CombatManager.Instance.OnCombatStarted += this._subscribeForCombatBuffer;
            CombatManager.Instance.OnCombatEnded -= _unsubscribeForCombatBuffer;
        }

        #region DEBUG

        public string BufferStatus()
        {
            string res = $"Normal Buffer size: {NormalActionsBuffer.Count}.\n";

            if (NormalActionsBuffer.Count > 0)
            {
                res += "Next Actions:";
                foreach (var item in NormalActionsBuffer)
                {
                    res += $"\n\t{item.Key.ToString()}:";
                    foreach (EntityAction e in item.Value)
                    {
                        res+= "\n\t\t" + e.ToString();
                    }
                }
            }
            if (CombatActionsBuffer.Count > 0)
            {
                res += $"Combat Buffer size: {CombatActionsBuffer.Count}.\nNext Actions:";
                foreach (var item in CombatActionsBuffer)
                {
                    res += $"\n\t{item}:";
                }
            }

            return res;
        }

        #endregion
        #region ENTITY_ACTIONS



        private IEnumerator bufferWithDelay()
        {
            yield return new WaitForSeconds(SettingsManager.Instance.CombatPreset.DelayBetweenActions);
            this.ExecuteNextFromCombatBuffer();
        }
        private void _executeNextFromCombatBufferDelayed()
        {
            StartCoroutine(this.bufferWithDelay());
        }

        public void ExecuteNextFromCombatBuffer()
        {
            if (CombatActionsBuffer.Count > 0)
            {
                IsUsingCombatBuffer = true;

                CombatActionsBuffer[0].SetCallback(_executeNextFromCombatBufferDelayed);
                CombatActionsBuffer[0].ExecuteAction();
            } 
            else
                IsUsingCombatBuffer = false;
        }

        public void ExecuteNextNormalBuffer(CharacterEntity refEntity)
        {
            if (NormalActionsBuffer[refEntity].Count > 0)
            {
                IsUsingNormalBuffer[refEntity] = true;

                NormalActionsBuffer[refEntity][0].SetCallback(() => ExecuteNextNormalBuffer(refEntity));
                NormalActionsBuffer[refEntity][0].ExecuteAction();
            } 
            else
                IsUsingNormalBuffer[refEntity] = false;
        }

        public void BuffSpell(CardEventData Data)
        {
            if (!Data.Played)
                return;

            NetworkManager.Instance.EntityAskToBuffSpell(Data.GeneratedHeader);
        }

        /// <summary>
        /// Used to buff any action into the combat or normal buffer according to the grid containing the entity
        /// </summary>
        /// <param name="action"></param>
        /// <param name="resetBuffer">If we empty all the buffer to only keep the passed action</param>
        public void BuffAction(EntityAction action, bool resetBuffer)
        {
            if (action.RefEntity.CurrentGrid.IsCombatGrid)
            {
                //If the entity is on a combat grid,simply queue the action and do it whenever it's your turn.
                CombatActionsBuffer.Add(action);
                action.RefBuffer = CombatActionsBuffer;

                // Once the buffer is playing, it'll automatically process every actions until the end
                // So don't call it twice to avoid double buffering which should NEVER happens
                if (!IsUsingCombatBuffer)
                    this.ExecuteNextFromCombatBuffer();
            } else
            {
                //if we're not in combat and that the buffer didn't have any actions for this entity; create the key with a new list (to be able to add actions inside)
                if (!NormalActionsBuffer.ContainsKey(action.RefEntity))
                    NormalActionsBuffer.Add(action.RefEntity, new List<EntityAction>());

                //If we want to empty the buffer and that the buffer wasn't empty:
                if (resetBuffer && NormalActionsBuffer[action.RefEntity].Count > 0)
                {
                    // Abort the action if it's ongoing
                    if (NormalActionsBuffer[action.RefEntity][0] is ProgressiveAction pAction)
                        pAction.AbortAction();

                    // Remove everything after the first entry since we're not going to use it anymore
                    if (NormalActionsBuffer[action.RefEntity].Count > 1)
                    {
                        // Hide everything but the first one that is being played
                        if(action.RefEntity == this.SelfPlayer)
                            for (int i = 1; i < NormalActionsBuffer[action.RefEntity].Count; i++)
                                PoolManager.Instance.CellIndicatorPool.HideActionIndicators(NormalActionsBuffer[action.RefEntity][i]);

                        NormalActionsBuffer[action.RefEntity].RemoveRange(1, NormalActionsBuffer[action.RefEntity].Count - 1);
                    }
                }

                //Adding action to buffer
                NormalActionsBuffer[action.RefEntity].Add(action);
                action.RefBuffer = NormalActionsBuffer[action.RefEntity];

                if (action.RefEntity == this.SelfPlayer)
                    PoolManager.Instance.CellIndicatorPool.DisplayActionIndicators(action);

                //Still don't know what that means
                if (!IsUsingNormalBuffer[action.RefEntity])
                    this.ExecuteNextNormalBuffer(action.RefEntity);
            }
        }

        public EntityAction FindActionByID(CharacterEntity entity, Guid ID)
        {
            if (entity.CurrentGrid is CombatGrid)
            {
                return CombatActionsBuffer.SingleOrDefault(a => a.ID == ID);
            }
            else
            {
                return NormalActionsBuffer[entity].SingleOrDefault(a => a.ID == ID);
            }
        }

        #endregion

        #region SAVE

        public void Save(string fileName)
        {
            FileInfo fileinfo = _getFileToSave(fileName);

            this._saveGameData(fileinfo);
        }

        private FileInfo _getFileToSave(string fileName)
        {
            var folder = new System.IO.DirectoryInfo(Application.persistentDataPath + "/save");
            if (!folder.Exists)
                folder.Create();

            // Make sure that every character is understandable by the system, or replace them
            string savename = fileName;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                savename = savename.Replace(c, '_');
            }

            FileInfo fileinfo = new System.IO.FileInfo(Application.persistentDataPath + "/save/" + savename + ".dbw");
            if (!fileinfo.Exists)
                fileinfo.Create();

            return fileinfo;
        }

        private void _saveGameData(System.IO.FileInfo file)
        {
            try
            {
                GameData.GameData data = new GameData.GameData();

                TaskScheduler mainThread = TaskScheduler.FromCurrentSynchronizationContext();
                Task.Run(() => this._getCurrentGameDataSideThread(data))
                    .ContinueWith((previousTask) =>
                    {
                        if (previousTask.Exception != null)
                        {
                            Debug.LogError(previousTask.Exception, this);
                        }
                        previousTask.Result.Save(file);
                    }, mainThread);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex, this);
                throw ex;
            }
        }

        private GameData.GameData _getCurrentGameDataSideThread(GameData.GameData gameData)
        {
            // TODO : Make a global class to save EACH possible game's grid states
            gameData.grids_data = GridManager.Instance.GetGridDatas();
            gameData.game_version = GameData.GameVersion.Current.ToString();
            gameData.save_name = this.SaveName;

            return gameData;
        }

        public static GameData.GameDataContainer MakeBaseGame(string saveName)
        {
            var gamedata = new GameData.GameData()
            {
                game_version = GameData.GameVersion.Current.ToString(),
                save_name = saveName,
                save_time = DateTime.Now,
                grids_data = Instance.CreateBaseGridsDatas()
            };

            return gamedata.Save(Instance._getFileToSave(saveName));
        }

        public string CreateBaseGridsJSON()
        {
            TextAsset[] jsons = Resources.LoadAll<TextAsset>("Saves/Grids/");
            JArray gridsArray = new JArray();

            foreach (TextAsset json in jsons)
            {
                JObject grid = JsonConvert.DeserializeObject<JObject>(json.text);
                gridsArray.Add(grid);
            }

            return gridsArray.ToString();
        }
        public GridData[] CreateBaseGridsDatas()
        {
            TextAsset[] jsons = Resources.LoadAll<TextAsset>("Saves/Grids/");

            GridData[] grids = new GridData[jsons.Length];

            for( int i = 0; i < jsons.Length; i++)
            {
                GridData loadedData = JsonConvert.DeserializeObject<GridData>(jsons[i].text);
                grids[i] = loadedData;
            }

            return grids;
        }

        #endregion
    }
}