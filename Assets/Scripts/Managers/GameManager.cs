using Jemkont.Entity;
using Jemkont.Events;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Managers
{
    public class GameManager : _baseManager<GameManager>
    {
        #region EVENTS
        public event EntityEventData.Event OnEnteredGrid;
        public event EntityEventData.Event OnExitingGrid;

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

        private void Start()
        {
            this.ProcessPlayerWelcoming();

            UIManager.Instance.Init();
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
            PhotonNetwork.CreateRoom($"SoloRoom{Random.Range(-256,256)}");
        }

        public void WelcomePlayers()
        {
            if (PhotonNetwork.PlayerList.Length >= 1)
            {
                this.Players = new Dictionary<string, PlayerBehavior>();
                int counter = 0;
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    PlayerBehavior newPlayer = Instantiate(this._playerPrefab, Vector3.zero, Quaternion.identity, this.transform);
                    newPlayer.Deck = CardsManager.Instance.DeckPresets[^1].Deck;
                    //TO CHANGE WITH TOOL. FOR TESTING ONLY
                    GridPosition spawnPosition;

                    switch (counter)
                    {
                        case 0: spawnPosition = new GridPosition((int)GridManager.Instance.FirstSpawn.x, (int)GridManager.Instance.FirstSpawn.y); break;
                        case 1: spawnPosition = new GridPosition((int)GridManager.Instance.SecondSpawn.x, (int)GridManager.Instance.SecondSpawn.y); break;
                        case 2: spawnPosition = new GridPosition((int)GridManager.Instance.ThirdSpawn.x, (int)GridManager.Instance.ThirdSpawn.y); break;
                        default: spawnPosition = new GridPosition((int)GridManager.Instance.FourthSpawn.x, (int)GridManager.Instance.FourthSpawn.y); break;
                    }

                    newPlayer.Init(SettingsManager.Instance.PlayerStats, GridManager.Instance.MainWorldGrid.Cells[spawnPosition.latitude, spawnPosition.longitude], GridManager.Instance.MainWorldGrid);
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
            }
        }
    }
}