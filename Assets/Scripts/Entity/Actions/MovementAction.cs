using DownBelow.GridSystem;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public class MovementAction : ProgressiveAction
    {
        protected List<Cell> calculatedPath;

        public MovementAction(CharacterEntity RefEntity, Cell TargetCell)
            : base(RefEntity, TargetCell)
        {
        }

        public override void ExecuteAction()
        {
            this.calculatedPath = GridManager.Instance.FindPath(RefEntity, TargetCell.PositionInGrid);

            // The path if from the front of the entity to the destination. We so need to add the own entity's cell
            this.calculatedPath.Insert(0, this.RefEntity.EntityCell);

            this.MoveWithPath();
        }

        public override void EndAction()
        {
            base.EndAction();
        }

        public virtual void MoveWithPath()
        {
            // Useless to animate hidden players
            if (!this.RefEntity.gameObject.activeSelf)
            {
                // /!\ TEMPORY ONLY, SET THE CELL AS THE LAST ONE OF PATH
                // We should have events instead for later on
                if (this.calculatedPath.Count > 0)
                    this.RefEntity.EntityCell = this.calculatedPath[^1];

                return;
            }

            // TODO : Ahah. So, it's the only solution and a not that bad idea, but maybe we should have a common MonoBehaviour for this instead of GameManager ?
            GameManager.Instance.StartCoroutine(this.FollowPath());
        }


        public IEnumerator FollowPath()
        {
            this.RefEntity.IsMoving = true;

            int currentCell = 0, targetCell = 1;
            float TimeToCrossCell = SettingsManager.Instance.GridsPreset.TimeToCrossCell;
            float timer;
            while (currentCell < this.calculatedPath.Count - 1)
            {
                timer = 0f;
                while (timer <= TimeToCrossCell)
                {
                    this.RefEntity.transform.position = Vector3.Lerp(this.calculatedPath[currentCell].gameObject.transform.position, this.calculatedPath[targetCell].gameObject.transform.position, timer / TimeToCrossCell);
                    timer += Time.deltaTime;
                    yield return null;
                }

                this.RefEntity.FireExitedCell();

                this.RefEntity.EntityCell = this.calculatedPath[targetCell];

                this.RefEntity.FireEnteredCell(this.calculatedPath[targetCell]);

                if (this.abortAction)
                {
                    this.RefEntity.NextCell = null;
                    this.EndAction();
                    break;
                }

                currentCell++;
                targetCell++;

                if (targetCell <= this.calculatedPath.Count - 1)
                    this.RefEntity.NextCell = this.calculatedPath[targetCell];
            }
            if(!this.abortAction)
                this.EndAction();
        }

        public override object[] GetDatas()
        {
            return new object[0];
        }

        public override void SetDatas(object[] Datas) { }
    }

}