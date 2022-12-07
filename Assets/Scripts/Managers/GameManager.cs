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
        [SerializeField]
        private PlayerBehavior _playerPrefab;

        public Dictionary<string, PlayerBehavior> Players;
        public PlayerBehavior SelfPlayer;

        private void Start()
        {
            this.WelcomePlayers();
        }

        public void WelcomePlayers()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 1)
            {
                this.Players = new Dictionary<string, PlayerBehavior>();
                int counter = 0;
                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    PlayerBehavior newPlayer = Instantiate(this._playerPrefab, Vector3.zero, Quaternion.identity, this.transform);
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
                        this.SelfPlayer = newPlayer;

                    this.Players.Add(player.UserId, newPlayer);
                    counter++;
                }
            }
        }
    }
}