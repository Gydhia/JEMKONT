using Jemkont.Entity;
using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Managers
{
    public class PlayerManager : _baseManager<PlayerManager>
    {
        public PlayerBehavior PlayerPrefab;

        public PlayerBehavior SelfPlayer;
        public void CreatePlayer(GridSystem.CombatGrid spawnGrid)
        {
            this.SelfPlayer = Instantiate(this.PlayerPrefab, Vector3.zero, Quaternion.identity, this.transform);
            this.SelfPlayer.Init(SettingsManager.Instance.PlayerStats, spawnGrid.Cells[0, 0], spawnGrid);
        }
    }

}
