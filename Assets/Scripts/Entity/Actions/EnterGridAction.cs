using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Entity
{
    public class EnterGridAction : EntityAction
    {
        public string TargetGrid;

        public EnterGridAction(CharacterEntity RefEntity, Cell TargetCell)
            : base(RefEntity, TargetCell)
        {
        }

        public virtual void Init(string TargetGrid)
        {
            this.TargetGrid = TargetGrid;
        }

        public override void ExecuteAction()
        {
            var gridToReach = GridManager.Instance.GetGridFromName(this.TargetGrid);

            if(gridToReach == null)
            {
                Debug.LogError("Couldn't load the passed grid. Aborted action");
                this.EndAction();
                return;
            }

            if(gridToReach.IsCombatGrid)
            {
                this._enterCombatGrid(gridToReach as CombatGrid);
            }
            else
            {
                this._enterWorldGrid(gridToReach);
            }

            EndAction();
        }

        private void _enterCombatGrid(CombatGrid combatGrid)
        {
            PlayerBehavior player = (PlayerBehavior)this.RefEntity;

            // If the player has a special item
            if (player.PlayerSpecialSlots.StorageItems.All(s => s.ItemPreset != null) && !combatGrid.HasStarted)
            {
                GameManager.Instance.FireEntitySwitchingGrid(player, combatGrid);
            }
            // Only notifies if it's the local player
            else if (player == GameManager.RealSelfPlayer)
            {
                UIManager.Instance.DatasSection.ShowWarningText(combatGrid.HasStarted ?
                    "You cannot enter a combat grid that has started combat" :
                    "You cannot enter a combat grid without all items equiped");
            }
        }

        private void _enterWorldGrid(WorldGrid worldGrid)
        {
            PlayerBehavior player = (PlayerBehavior)this.RefEntity;

            System.Guid spawnId = SettingsManager.Instance.SpawnablesPresets.First(k => k.Value is SpawnPreset).Key;
            // It SHOULDNT be null, it's dev job to put these
            GridPosition spawnLocation = worldGrid.SelfData.SpawnablePresets.Where(k => k.Value == spawnId).Select(kv => kv.Key).First();

            Cell spawnCell = worldGrid.Cells[spawnLocation.latitude, spawnLocation.longitude];

            if (spawnCell.Datas.state == CellState.Walkable)
            {
                player.FireExitedCell();
                GameManager.Instance.FireEntitySwitchingGrid(player, worldGrid);
                player.FireEnteredCell(spawnCell);
                player.transform.position = spawnCell.WorldPosition;
            }
            // No free spawn found. Not set or someone put object over them
            else
            {
                UIManager.Instance.DatasSection.ShowWarningText("Seems like your friends blocked the link between the two worlds...");
            }   
        }

        public override object[] GetDatas()
        {
            return new object[] { this.TargetGrid };
        }

        public override void SetDatas(object[] Datas)
        {
            this.TargetGrid = Datas[0] as string;
        }
    }
}