using Jemkont.Entity;
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
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    PlayerBehavior newPlayer = Instantiate(this._playerPrefab, Vector3.zero, Quaternion.identity, this.transform);
                    // TODO: make it works with world grids
                    newPlayer.PlayerID = player.UserId;

                    if (player.UserId == PhotonNetwork.LocalPlayer.UserId)
                        this.SelfPlayer = newPlayer;

                    this.Players.Add(player.UserId, newPlayer);
                }
            }
        }
    }
}