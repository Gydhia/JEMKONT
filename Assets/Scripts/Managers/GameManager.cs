using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Managers
{
    public class GameManager : _baseManager<GameManager>
    {
        #region EVENTS
        public event GameEventData.Event OnPlayersWelcomed;

        public event EntityEventData.Event OnEnteredGrid;
        public event EntityEventData.Event OnExitingGrid;

        public void FirePlayersWelcomed()
        {
            this.OnPlayersWelcomed?.Invoke(new());
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
        #endregion

        [SerializeField]
        private PlayerBehavior _playerPrefab;

        public Dictionary<string, PlayerBehavior> Players;
        public PlayerBehavior SelfPlayer;

        public static bool GameStarted = false;

        #region Players_Actions_Buffer

        public static List<EntityAction> CombatActionsBuffer = new List<EntityAction>();
        public static Dictionary<CharacterEntity, List<EntityAction>> NormalActionsBuffer =
            new Dictionary<CharacterEntity, List<EntityAction>>();

        public static bool IsUsingCombatBuffer = false;
        public static bool IsUsingNormalBuffer = false;


        #endregion

        private void Start()
        {
            UIManager.Instance.Init();
            CardsManager.Instance.Init();
            GridManager.Instance.Init();
            NetworkManager.Instance.SubToGameEvents();

            this.ProcessPlayerWelcoming();
        }

        public void ProcessPlayerWelcoming()
        {
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
                    PlayerBehavior newPlayer = Instantiate(this._playerPrefab, Vector3.zero, Quaternion.identity, this.transform);
                    newPlayer.Deck = CardsManager.Instance.DeckPresets.Values.Single(d => d.Name == "TestDeck").Copy();
                 
                    newPlayer.Init(SettingsManager.Instance.FishermanStats, GridManager.Instance.MainWorldGrid.Cells[spawnLocations.ElementAt(counter).latitude, spawnLocations.ElementAt(counter).longitude], GridManager.Instance.MainWorldGrid);
                    // TODO: make it works with world grids
                    newPlayer.PlayerID = player.UserId;

                    if (player.UserId == PhotonNetwork.LocalPlayer.UserId)
                    {
                        this.SelfPlayer = newPlayer;
                        CameraManager.Instance.AttachPlayerToCamera(this.SelfPlayer);
                    }

                    this.Players.Add(player.UserId, newPlayer);
                    counter++;
                }

                GameStarted = true;
                this.FirePlayersWelcomed();
            }
        }


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
            if(NormalActionsBuffer[refEntity].Count > 0)
            {
                IsUsingNormalBuffer = true;

                NormalActionsBuffer[refEntity][0].SetCallback(() => ExecuteNextNormalBuffer(refEntity));
                NormalActionsBuffer[refEntity][0].ExecuteAction();
            }
            else
                IsUsingNormalBuffer = false;
        }

        public void BuffSpell(Mechanics.ScriptableCard spellDatas, Cell targetCell, CharacterEntity refEntity)
        {
            // TODO : Not working since we're not using the constructor, when refactoring the combat find a way to do so
            for (int i = 0; i < spellDatas.Spells.Length; i++)
                spellDatas.Spells[i].Init(i > 0 ? spellDatas.Spells[i - 1] : null);

            CombatActionsBuffer.AddRange(spellDatas.Spells);

            if (!IsUsingCombatBuffer)
                this.ExecuteNextFromCombatBuffer();
        }

        // TODO : Since we're using BuffAction now, remove this later when no longer needed
        public void BuffMovement(Cell targetCell, CharacterEntity refEntity, bool resetBuffer = true)
        {
            MovementAction movement = new MovementAction(refEntity, targetCell);

            if (refEntity.CurrentGrid.IsCombatGrid)
            {
                CombatActionsBuffer.Add(movement);

                if (!IsUsingCombatBuffer)
                    this.ExecuteNextFromCombatBuffer();
            }
            else 
            {
                if (!NormalActionsBuffer.ContainsKey(refEntity))
                    NormalActionsBuffer.Add(refEntity, new List<EntityAction>());

                if (resetBuffer && NormalActionsBuffer[refEntity].Count > 0)
                {
                    // Abort the action if it's ongoing
                    if(NormalActionsBuffer[refEntity][0] is ProgressiveAction pAction)
                        pAction.AbortAction();

                    // Remove everything after the first entry since we're not going to use it anymore
                    if (NormalActionsBuffer[refEntity].Count > 1)
                        NormalActionsBuffer[refEntity].RemoveRange(1, NormalActionsBuffer[refEntity].Count - 1);
                }

                NormalActionsBuffer[refEntity].Add(movement);

                if (!IsUsingNormalBuffer)
                    this.ExecuteNextNormalBuffer(refEntity);
            }
        }

        /// <summary>
        /// Used to buff any action into the combat or normal buffer according to the grid containing the entity
        /// </summary>
        /// <param name="action"></param>
        /// <param name="resetBuffer">If we empty all the buffer to only keep the passed action</param>
        public void BuffAction(EntityAction action, bool resetBuffer = true)
        {
            if (action.RefEntity.CurrentGrid.IsCombatGrid)
            {
                CombatActionsBuffer.Add(action);

                if (!IsUsingCombatBuffer)
                    this.ExecuteNextFromCombatBuffer();
            }
            else
            {
                if (!NormalActionsBuffer.ContainsKey(action.RefEntity))
                    NormalActionsBuffer.Add(action.RefEntity, new List<EntityAction>());

                if (resetBuffer && NormalActionsBuffer[action.RefEntity].Count > 0)
                {
                    // Abort the action if it's ongoing
                    if (NormalActionsBuffer[action.RefEntity][0] is ProgressiveAction pAction)
                        pAction.AbortAction();

                    // Remove everything after the first entry since we're not going to use it anymore
                    if (NormalActionsBuffer[action.RefEntity].Count > 1)
                        NormalActionsBuffer[action.RefEntity].RemoveRange(1, NormalActionsBuffer[action.RefEntity].Count - 1);
                }

                NormalActionsBuffer[action.RefEntity].Add(action);

                if (!IsUsingNormalBuffer)
                    this.ExecuteNextNormalBuffer(action.RefEntity);
            }
        }

        public void BuffEnterGrid(string grid, CharacterEntity refEntity)
        {

        }

        public void RemoveTopFromBuffer(CharacterEntity refEntity)
        {
            if (refEntity.CurrentGrid.IsCombatGrid)
            {
                CombatActionsBuffer.RemoveAt(0);
            }
            else
            {
                NormalActionsBuffer[refEntity].RemoveAt(0);
            }
        }

        public void InsertToBuffer(EntityAction action)
        {
            CombatActionsBuffer.Insert(0, action);
        }

        #endregion
    }
}