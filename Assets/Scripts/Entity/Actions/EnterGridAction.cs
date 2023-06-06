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
            // For now we assume that only player can switch from grids
            PlayerBehavior player = (PlayerBehavior)this.RefEntity;
            CombatGrid newGrid = GridManager.Instance.GetGridFromName(this.TargetGrid) as CombatGrid;

            // If the player has a special item
            if (player.PlayerSpecialSlots.StorageItems.All(s => s.ItemPreset != null) && !newGrid.HasStarted)
            {
                GameManager.Instance.FireEntityExitingGrid(player);

                player.EnterNewGrid(newGrid);

                GameManager.Instance.FireEntityEnteredGrid(player);
            }
            // Only notifies if it's the local player
            else if (player == GameManager.RealSelfPlayer)
            {
                UIManager.Instance.DatasSection.ShowWarningText(newGrid.HasStarted ?
                    "You cannot enter a combat grid that has started combat" :
                    "You cannot enter a combat grid without all items equiped");
            }

            //TODO: Abort all actions?
            EndAction();
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