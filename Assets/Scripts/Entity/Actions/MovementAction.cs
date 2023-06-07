using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Spells;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace DownBelow.Entity
{
    public class MovementAction : ProgressiveAction
    {
        protected List<Cell> calculatedPath;
        private List<CellIndicator> indic;
        public MovementAction(CharacterEntity RefEntity, Cell TargetCell)
            : base(RefEntity, TargetCell)
        {
        }

        public override void ExecuteAction()
        {
            this.calculatedPath = this.GetProcessedPath();
            if (calculatedPath != null)
            {
                // The path if from the front of the entity to the destination. We so need to add the own entity's cell
                this.calculatedPath.Insert(0, this.RefEntity.EntityCell);

                this.MoveWithPath();
            } 
            else
                EndAction();
        }

        protected virtual List<Cell> GetProcessedPath()
        {
            return GridManager.Instance.FindPath(RefEntity, TargetCell.PositionInGrid);
        }

        public virtual void MoveWithPath()
        {
            if (!this.RefEntity.CanMove)
            {
                EndAction();
                return;
            }

            // Useless to animate hidden players
            if (!this.RefEntity.gameObject.activeSelf)
            {
                // /!\ TEMPORARY ONLY, SET THE CELL AS THE LAST ONE OF PATH
                // We should have events instead for later on
                if (this.calculatedPath.Count > 0)
                    this.RefEntity.EntityCell = this.calculatedPath[^1];

                EndAction();
                return;
            }

            // TODO : Ahah. So, it's the only solution and a not that bad idea, but maybe we should have a common MonoBehaviour for this instead of GameManager ?
            GameManager.Instance.StartCoroutine(this.FollowPath());
        }


        public IEnumerator FollowPath()
        {
            this.RefEntity.IsMoving = true;
            
            int currentCell = 0, targetCell = 1;
            float constantCrossTime = SettingsManager.Instance.GridsPreset.TimeToCrossCell;
            float TimeToCrossCell;
            float timer;
            while (currentCell < this.calculatedPath.Count - 1)
            {
                bool isDiagonal =
                    this.RefEntity.EntityCell.PositionInGrid.latitude != this.calculatedPath[targetCell].PositionInGrid.latitude &&
                    this.RefEntity.EntityCell.PositionInGrid.longitude != this.calculatedPath[targetCell].PositionInGrid.longitude;

                // squaredroot ratio for diagonals
                TimeToCrossCell = isDiagonal ? constantCrossTime * 1.4142f : constantCrossTime;

                timer = 0f;
                while (timer <= TimeToCrossCell)
                {
                    Vector3 newPos = Vector3.Lerp(this.calculatedPath[currentCell].gameObject.transform.position, this.calculatedPath[targetCell].gameObject.transform.position, timer / TimeToCrossCell);
                    float velocity = (newPos - this.RefEntity.transform.position).magnitude;

                    this.RefEntity.Animator.SetFloat("Speed", velocity);

                    var targetRotation = Quaternion.LookRotation(newPos - this.RefEntity.transform.position);
                    this.RefEntity.EntityHolder.rotation = Quaternion.Slerp(this.RefEntity.EntityHolder.transform.rotation, targetRotation, timer / TimeToCrossCell);

                    this.RefEntity.transform.position = newPos;

                    //this.RefEntity.transform.LookAt(this.calculatedPath[targetCell].gameObject.transform.position);

                    timer += Time.deltaTime;
                    yield return null;
                }

                this.RefEntity.FireExitedCell();

                this.RefEntity.EntityCell = this.calculatedPath[targetCell];

                this.RefEntity.FireEnteredCell(this.calculatedPath[targetCell]);

                if (this.abortAction || !this.RefEntity.CanMove)
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
            if (!this.abortAction)
                this.EndAction();

            if (this._shouldEndAnim())
            {
                this.RefEntity.Animator.SetFloat("Speed", 0f);
            }
        }

        private bool _shouldEndAnim()
        {
            return
               this is CombatMovementAction ||
               this.RefBuffer.Count == 0 ||
               !(this.RefBuffer[0] is MovementAction) ||
               (this.RefBuffer.Count > 1 && !(this.RefBuffer[1] is MovementAction));
        }


        public override void EndAction()
        {
            this.RefEntity.IsMoving = false;
            base.EndAction();
        }
        public override object[] GetDatas()
        {
            return new object[0];
        }

        public override void SetDatas(object[] Datas) { }
    }

}