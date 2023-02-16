using DownBelow.Entity;
using DownBelow.Events;
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

        private void Start()
        {
            UIManager.Instance.Init();
            GridManager.Instance.Init();
            CardsManager.Instance.Init();
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
                    SpawnPreset preset = null;
                    newPlayer.Init(SettingsManager.Instance.FishermanStats, GridManager.Instance.MainWorldGrid.Cells[spawnLocations.ElementAt(counter).latitude, spawnLocations.ElementAt(counter).longitude], GridManager.Instance.MainWorldGrid, preset);
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
    }
}