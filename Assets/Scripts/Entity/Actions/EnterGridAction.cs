using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public class EnterGridAction : EntityAction
    {
        public string TargetGrid;

        public EnterGridAction(CharacterEntity RefEntity, Cell TargetCell, string TargetGrid)
            : base(RefEntity, TargetCell)
        {
            this.TargetGrid = TargetGrid;
        }

        public override void ExecuteAction()
        {
            // For now we assume that only player can switch from grids
            PlayerBehavior player = (PlayerBehavior)this.RefEntity;

            GameManager.Instance.FireEntityExitingGrid(player);

            player.EnterNewGrid(GridManager.Instance.GetGridFromName(this.TargetGrid) as CombatGrid);

            GameManager.Instance.FireEntityEnteredGrid(player);
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