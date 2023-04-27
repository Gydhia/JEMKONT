using EODE.Wonderland;

using DownBelow.Spells;
using DownBelow.Spells.Alterations;
using DownBelow.GridSystem;
using DownBelow.Managers;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Math = System.Math;


namespace DownBelow.Entity {


    public class EnemyAction : EntityAction
    {
        Action Behavior;

        public EnemyAction(CharacterEntity RefEntity, Cell TargetCell, Action ExecuteAction) : base(RefEntity, TargetCell)
        {
            this.RefEntity = RefEntity;
            this.TargetCell = TargetCell;
            this.Behavior = ExecuteAction;
        }

        public EnemyAction(CharacterEntity RefEntity, Cell TargetCell) : base(RefEntity, TargetCell)
        {
            this.RefEntity = RefEntity;
            this.TargetCell = TargetCell;
        }

        public override void ExecuteAction()
        {
            this.Behavior();
            /*if (this.RefEntity.CurrentGrid.IsCombatGrid)
            {
                if (this.RefEntity.IsMoving || !this.AllowedToProcess())
                {
                    EndAction();
                    return;
                }

                // -1 since the own entity's cell is included
                this.RefEntity.ApplyStat(EntityStatistics.Speed, -(this.calculatedPath.Count - 1));
            }

            base.MoveWithPath();*/
        }

        public override object[] GetDatas()
        {
            object[] Datas = new object[3];
            Datas[0] = this.RefBuffer;
            Datas[1] = this.RefEntity;
            Datas[2] = this.TargetCell;

            return Datas;
        }

        public override void SetDatas(object[] Datas)
        {
            throw new NotImplementedException();
        }



    }

    public class EnemyEntity : CharacterEntity {

        public EntityPreset EnemyStyle;
        CharacterEntity cachedAllyToAttack;

        public CharacterEntity CurrentTarget;

        #region Movement
        public enum MovementType { Straight, StraightToRange, Flee, Kite };
        public MovementType movementType = MovementType.Straight;
        private Dictionary<MovementType, EnemyAction> _movementBehaviors = new Dictionary<MovementType, EnemyAction>();
        #endregion

        #region Attack
        public enum AttackType { ClosestRandom, FarthestRandom, LowestRandom, HighestRandom, Random };
        public AttackType attackType = AttackType.ClosestRandom;
        private Dictionary<AttackType, EnemyAction> _attackBehaviors = new Dictionary<AttackType, EnemyAction>();
        #endregion

        #region Target
        public AttackType TargetType = AttackType.ClosestRandom;
        private Dictionary<AttackType, EnemyAction> _targetBehaviors = new Dictionary<AttackType, EnemyAction>();
        #endregion


        protected List<EnemyAction> _turnBehaviors = new List<EnemyAction>();

        public override void Init(EntityStats stats, Cell refCell, WorldGrid refGrid, int order = 0) {
            base.Init(stats, refCell, refGrid);

            this.UID = refGrid.UName + this.EnemyStyle.UName + order;
            _movementBehaviors.Add(MovementType.Straight, new EnemyAction(this, null, MovementStraight));
            _movementBehaviors.Add(MovementType.StraightToRange, new EnemyAction(this, null, MovementStraightToRange));
            _attackBehaviors.Add(AttackType.ClosestRandom, new EnemyAction(this, null, AttackClosestRandom));
            _targetBehaviors.Add(AttackType.ClosestRandom, new EnemyAction(this, null, TargetClosestRandom));
            _targetBehaviors.Add(AttackType.FarthestRandom, new EnemyAction(this, null, TargetFarthestRandom));

        }

        public override void StartTurn() 
        {
            base.StartTurn();
            this.EntityActionsBuffer.Add(this._targetBehaviors[TargetType]);
            this.EntityActionsBuffer.Add(this._movementBehaviors[movementType]);
            this.EntityActionsBuffer.Add(this._attackBehaviors[attackType]);

            Debug.Log("ENDTURN");
            EndTurn();
            CombatManager.Instance.ProcessEndTurn(this.UID);

            /*
            var TargetPosition = GetTargetPosition();
            Debug.Log("123456 1: " + TargetPosition.latitude +" : " + TargetPosition.longitude);
            GridManager.Instance.FindPath(this,TargetPosition,true);

            Debug.Log("123456 2: " + this.Speed);
            if (GridManager.Instance.Path.Count > 0 && this.Speed > 0) {
                Debug.Log("123456 3: " + this.Speed);
                GridManager.Instance.ShowPossibleCombatMovements(this);
                if (!Confused) NetworkManager.Instance.EntityAsksForPath(this, GridManager.Instance.Path[GridManager.Instance.Path.Count - 1], this.CurrentGrid);
            }
            //Moved (or not if was in range); and will now Autoattack:
            if (CanAutoAttack) {
                if (cachedAllyToAttack != null) {
                    //LETSGOOOOOO FIREEEEEEEEEEEEE
                    AutoAttack(cachedAllyToAttack.EntityCell);
                }
            }*/
            //TODO: ENEMY SPELL?
        }

        protected void processTurnActions()
        {

        }

        // All Movement Behaviours
        #region Movement Behaviours
        /// <summary>
        /// Will Go straight to the target's location
        /// </summary>
        private void MovementStraight() {
            GridPosition targPosition = CurrentTarget.EntityCell.PositionInGrid;
            var path = GridManager.Instance.FindPath(this, targPosition);
            if (path.Count > this.Speed)
                NetworkManager.Instance.EntityAskToBuffAction(new EnemyAction(this, path[this.Speed]));
            else
                NetworkManager.Instance.EntityAskToBuffAction(new EnemyAction(this, CurrentTarget.EntityCell));
        }

        /// <summary>
        /// Will Go straight to the target stops when in range
        /// </summary>
        private void MovementStraightToRange() {
            GridPosition targPosition = CurrentTarget.EntityCell.PositionInGrid;
            List<Cell> path = GridManager.Instance.FindPath(this, targPosition, false, this.Range);

            if (path == null || path.Count == 0)
                return;

            if (path.Count > this.Speed)
                NetworkManager.Instance.EntityAskToBuffAction(new EnemyAction(this, path[this.Speed]));
            else
                NetworkManager.Instance.EntityAskToBuffAction(new EnemyAction(this, path[path.Count - 1]));
        }
        #endregion

        // All Attack Behaviours
        #region Attack Behaviours
        /// <summary>
        /// Will attack the closest player (random btwn two at same range)
        /// </summary>
        private void AttackClosestRandom() {
            if (!CanAutoAttack) {
                return;
            }
            _targetBehaviors[attackType].ExecuteAction();
            if (cachedAllyToAttack != null) {
                AutoAttack(cachedAllyToAttack.EntityCell);
            }
        }
        #endregion

        // All Target Behaviours (target is used for the movement)
        #region Target Behaviours
        /// <summary>
        /// Will target the closest player (random btwn two at the same range)
        /// </summary>
        private void TargetClosestRandom() {
            CharacterEntity[] PlayersByDistance = PlayersOrderedByDistance("Min", out int sameDist);
            var ClosestPlayer = PlayersByDistance[UnityEngine.Random.Range(0, sameDist)];

            CurrentTarget = ClosestPlayer;
            this._targetBehaviors[TargetType].TargetCell = ClosestPlayer.EntityCell;
            this._movementBehaviors[movementType].TargetCell = ClosestPlayer.EntityCell;
            this._attackBehaviors[attackType].TargetCell = ClosestPlayer.EntityCell;
        }

        /// <summary>
        /// Will target the closest player (random btwn two at the same range)
        /// </summary>
        private void TargetFarthestRandom() {
            CharacterEntity[] PlayersByDistance = PlayersOrderedByDistance("Max", out int sameDist);
            var FarthestPlayer = PlayersByDistance[UnityEngine.Random.Range(0, sameDist)];

            CurrentTarget = FarthestPlayer;
            this._targetBehaviors[TargetType].TargetCell = FarthestPlayer.EntityCell;
            this._movementBehaviors[movementType].TargetCell = FarthestPlayer.EntityCell;
            this._attackBehaviors[attackType].TargetCell = FarthestPlayer.EntityCell;

        }
        #endregion


        #region Player Sensors
        /// <summary>
        /// Returns Players ordered from closest to farthest / farthest to Closest. If an entity is provoking, clones it to put in index 0 of the array.
        /// Arguments : Type = "Min" or "Max" depending on getting the closest or farthest
        /// </summary>
        public CharacterEntity[] PlayersOrderedByDistance(string type, out int sameDist) {
            var Players = CurrentGrid.GridEntities.FindAll(x => x.IsAlly);

            int[] distances = new int[Players.Count];
            //Get all distances to player (from path) and set them in distances
            for (int i = 0;i < Players.Count;i++) {
                CharacterEntity item = Players[i];
                var path = GridManager.Instance.FindPath(this, item.EntityCell.PositionInGrid);
                distances[i] = path == null ? int.MaxValue : path.Count;
            }
            CharacterEntity[] orderedAllies = new CharacterEntity[distances.Length];
            int ActualPlayer = 0;
            int TotalOfSameDist = 0;
            bool recordSameDist = true;
            int LastDist = -1;
            int length = distances.Length;
            //for all distances take the smallest 
            for (int i = 0;i < length;i++) {
                int Dist = SetPlayerPositionInTargetList(i, type, ref distances, Players, ref orderedAllies, ActualPlayer);
                if (LastDist == Dist && recordSameDist) {
                    TotalOfSameDist++;
                } else if (i > 0) {
                    recordSameDist = false;
                }
                ActualPlayer++;
                LastDist = Dist;
            }
            sameDist = TotalOfSameDist;
            return orderedAllies;
        }

        private int SetPlayerPositionInTargetList(int i, string type, ref int[] distances, List<CharacterEntity> Players, ref CharacterEntity[] orderedAllies, int ActualPlayer) {
            int Dist;
            // Depending on the type of distance (close or far) get the good distances
            if (type == "Min") {
                Dist = distances.Min();
            } else {
                Dist = distances.Max();
            }
            int Index = Array.IndexOf(distances, Dist);

            orderedAllies[ActualPlayer] = Players[Index];
            
            ArrayHelper.RemoveAt(ref distances, Index); ;

            return Dist;
        }

        private void MakeProvokedFirst(CharacterEntity provoked, ref CharacterEntity[] array, List<CharacterEntity> players) {
            for (int i = 0;i < array.Length;i++) {
                ArrayHelper.Insert(ref array, i, provoked);
                break;
            }
        }
        #endregion

        Cell RandomCellInRange() {
            List<Cell> cells = new List<Cell>();
            foreach (var item in CurrentGrid.Cells) {
                cells.Add(item);
            }
            return cells.FindAll(x => (Mathf.Abs(x.PositionInGrid.latitude - this.EntityCell.PositionInGrid.latitude) + Mathf.Abs(x.PositionInGrid.longitude - this.EntityCell.PositionInGrid.longitude)) >= Speed).Random();

        }
    }
}