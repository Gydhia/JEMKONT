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
    public class EnemyEntity : CharacterEntity {

        public EntityPreset EnemyStyle;
        CharacterEntity cachedAllyToAttack;

        public CharacterEntity CurrentTarget;

        #region Movement
        public enum MovementType { Straight, StraightToRange, Flee, Kite };
        public MovementType movementType = MovementType.Straight;
        private Dictionary<MovementType, System.Action> _movementBehaviors = new Dictionary<MovementType, System.Action>();
        #endregion

        #region Attack
        public enum AttackType { ClosestRandom, FarthestRandom, LowestRandom, HighestRandom, Random };
        public AttackType attackType = AttackType.ClosestRandom;
        private Dictionary<AttackType, System.Action> _attackBehaviors = new Dictionary<AttackType, System.Action>();
        #endregion

        #region Target
        public AttackType TargetType = AttackType.ClosestRandom;
        private Dictionary<AttackType, System.Action> _targetBehaviors = new Dictionary<AttackType, System.Action>();
        #endregion


        protected List<System.Action> _turnBehaviors = new List<Action>();

        public override void Init(EntityStats stats, Cell refCell, WorldGrid refGrid, int order = 0) {
            base.Init(stats, refCell, refGrid);

            this.UID = refGrid.UName + this.EnemyStyle.UName + order;
            _movementBehaviors.Add(MovementType.Straight, MovementStraight);
            _movementBehaviors.Add(MovementType.StraightToRange, MovementStraightToRange);
            _attackBehaviors.Add(AttackType.ClosestRandom, AttackClosestRandom);
            _targetBehaviors.Add(AttackType.ClosestRandom, TargetClosestRandom);
            _targetBehaviors.Add(AttackType.FarthestRandom, TargetFarthestRandom);

        }

        public override void StartTurn() 
        {
            base.StartTurn();

            this._turnBehaviors.Add(this._targetBehaviors[TargetType]);
            this._turnBehaviors.Add(this._movementBehaviors[movementType]);
            this._turnBehaviors.Add(this._attackBehaviors[attackType]);

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
            if (path.Count > this.Speed) {
                NetworkManager.Instance.EntityAsksForPath(this, path[this.Speed], this.CurrentGrid);
            }
            NetworkManager.Instance.EntityAsksForPath(this, this.CurrentGrid);
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
                NetworkManager.Instance.EntityAsksForPath(this, path[this.Speed], this.CurrentGrid);
            else
                NetworkManager.Instance.EntityAsksForPath(this, path[path.Count - 1], this.CurrentGrid);
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
            _targetBehaviors[attackType]();
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
        }

        /// <summary>
        /// Will target the closest player (random btwn two at the same range)
        /// </summary>
        private void TargetFarthestRandom() {
            CharacterEntity[] PlayersByDistance = PlayersOrderedByDistance("Max", out int sameDist);
            var FarthestPlayer = PlayersByDistance[UnityEngine.Random.Range(0, sameDist)];

            CurrentTarget = FarthestPlayer;
        }
        #endregion


        #region Player Sensors
        /// <summary>
        /// Returns Players ordered from closest to farthest / farthest to Closest. If an entity is provoking, clones it to put in index 0 of the array.
        /// Arguments : Type = "Min" or "Max" depending on getting the closest or farthest
        /// </summary>
        public CharacterEntity[] PlayersOrderedByDistance(string type, out int sameDist) {
            var Players = CurrentGrid.GridEntities.FindAll(x => x.IsAlly);
            var Provoking = Players.FindAll(x => x.Provoke);

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
            //if the player is provoking then make it first priority
            if (Players[Index].Provoke) {
                MakeProvokedFirst(Players[Index], ref orderedAllies, Players);
            } else {
                orderedAllies[ActualPlayer] = Players[Index];
            }
            ArrayHelper.RemoveAt(ref distances, Index); ;

            return Dist;
        }

        private void MakeProvokedFirst(CharacterEntity provoked, ref CharacterEntity[] array, List<CharacterEntity> players) {
            for (int i = 0;i < array.Length;i++) {
                if (players[i].Provoke) {
                    continue;
                }
                ArrayHelper.Insert(ref array, i, provoked);
                break;
            }
        }
        #endregion

        public override Spell AutoAttackSpell() {
            return new Spell(CombatManager.Instance.PossibleAutoAttacks.Find(x => x is DamageStrengthSpellDealer ));

        }
        Cell RandomCellInRange() {
            List<Cell> cells = new List<Cell>();
            foreach (var item in CurrentGrid.Cells) {
                cells.Add(item);
            }
            return cells.FindAll(x => (Mathf.Abs(x.PositionInGrid.latitude - this.EntityCell.PositionInGrid.latitude) + Mathf.Abs(x.PositionInGrid.longitude - this.EntityCell.PositionInGrid.longitude)) >= Speed).Random();

        }
    }
}