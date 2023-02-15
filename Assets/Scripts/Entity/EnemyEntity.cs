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
        private Dictionary<MovementType, System.Action> MovementBehaviors = new Dictionary<MovementType, System.Action>();
        #endregion

        #region Attack
        public enum AttackType { ClosestRandom, FarthestRandom, LowestRandom, HighestRandom, Random };
        public AttackType attackType = AttackType.ClosestRandom;
        private Dictionary<AttackType, System.Action> AttackBehaviors = new Dictionary<AttackType, System.Action>();
        #endregion

        #region Target
        public AttackType TargetType = AttackType.ClosestRandom;
        private Dictionary<AttackType, System.Action> TargetBehaviors = new Dictionary<AttackType, System.Action>();
        #endregion


        public override void Init(EntityStats stats, Cell refCell, WorldGrid refGrid, int order = 0) {
            base.Init(stats, refCell, refGrid);

            this.UID = refGrid.UName + this.EnemyStyle.UName + order;
            MovementBehaviors.Add(MovementType.Straight, MovementStraight);
            MovementBehaviors.Add(MovementType.StraightToRange, MovementStraightToRange);
            MovementBehaviors.Add(MovementType.Flee, MovementFlee);
            MovementBehaviors.Add(MovementType.Kite, MovementKite);
            AttackBehaviors.Add(AttackType.ClosestRandom, AttackClosestRandom);
            TargetBehaviors.Add(AttackType.ClosestRandom, TargetClosestRandom);
            TargetBehaviors.Add(AttackType.FarthestRandom, TargetFarthestRandom);
            TargetBehaviors.Add(AttackType.HighestRandom, TargetHighestRandom);
            TargetBehaviors.Add(AttackType.LowestRandom, TargetLowestRandom);

        }

        public override void StartTurn() {
            CanAutoAttack = !Disarmed;//CanAutoAttack = true if !Disarmed
            this.ReinitializeStat(EntityStatistics.Speed);
            this.ReinitializeStat(EntityStatistics.Mana);
            if (Stunned || Sleeping) {
                EndTurn();
            }

            TargetBehaviors[TargetType]();
            MovementBehaviors[movementType]();
            CurrentTarget = null;
            AttackBehaviors[attackType]();
            CurrentTarget = null;
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
            NetworkManager.Instance.EntityAsksForPath(this, path[path.Count-1], this.CurrentGrid);
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

        /// <summary>
        /// Will flee target to the farthest possible point.
        /// </summary>
        private void MovementFlee() {
            GridPosition targPosition = CurrentTarget.EntityCell.PositionInGrid;
            int movePoints = this.Speed;
            Cell entityCell = this.EntityCell;

            int highestDist = 0; 
            int highestX = 0;
            int highestY = 0;

            for (int x = -movePoints; x <= movePoints; x++)
            {
                for (int y = -movePoints; y <= movePoints; y++)
                {
                    if (x == 0 && y == 0 || (Mathf.Abs(x) + Mathf.Abs(y) > movePoints))
                        continue;

                    int checkX = entityCell.Datas.widthPos + x;
                    int checkY = entityCell.Datas.heightPos + y;

                    if (checkX < 0 || checkX >= this.CurrentGrid.GridWidth || checkY < 0 || checkY >= this.CurrentGrid.GridHeight)
                        continue;

                    int dist = (Math.Abs(targPosition.longitude - checkX) + Math.Abs(targPosition.latitude - checkY));
                    if (dist > highestDist && this.CurrentGrid.Cells[checkX, checkY].Datas.state == CellState.Walkable) {
                        highestDist = dist;
                        highestX = checkX;
                        highestY = checkY;
                    }
                }
            }

            GridPosition toPosition = new GridPosition(highestX, highestY);
            List<Cell> path = GridManager.Instance.FindPath(this, toPosition);

            if (path == null || path.Count == 0)
                return;

            NetworkManager.Instance.EntityAsksForPath(this, path[path.Count-1], this.CurrentGrid);
        }

        /// <summary>
        /// Will flee target but will stay in attack range.
        /// </summary>
        private void MovementKite()
        {
            int xPos = this.EntityCell.Datas.widthPos;
            int yPos = this.EntityCell.Datas.heightPos;
            GridPosition targPosition = CurrentTarget.EntityCell.PositionInGrid;
            int dist = (Math.Abs(targPosition.longitude - xPos) + Math.Abs(targPosition.latitude - yPos));

            if (dist > this.Range) {
                MovementStraightToRange();
            }
            else {
                FleeToRange();
            }
        }

        private void FleeToRange()
        {
            GridPosition targPosition = CurrentTarget.EntityCell.PositionInGrid;
            int movePoints = this.Speed;
            Cell entityCell = this.EntityCell;

            int highestDist = 0;
            int highestX = 0;
            int highestY = 0;

            for (int x = -movePoints; x <= movePoints; x++)
            {
                for (int y = -movePoints; y <= movePoints; y++)
                {
                    if (x == 0 && y == 0 || (Mathf.Abs(x) + Mathf.Abs(y) > movePoints))
                        continue;

                    int checkX = entityCell.Datas.widthPos + x;
                    int checkY = entityCell.Datas.heightPos + y;

                    if (checkX < 0 || checkX >= this.CurrentGrid.GridWidth || checkY < 0 || checkY >= this.CurrentGrid.GridHeight)
                        continue;

                    int dist = (Math.Abs(targPosition.longitude - checkX) + Math.Abs(targPosition.latitude - checkY));
                    if (dist > highestDist && dist <= this.Range && this.CurrentGrid.Cells[checkX, checkY].Datas.state == CellState.Walkable)
                    {
                        highestDist = dist;
                        highestX = checkX;
                        highestY = checkY;
                    }
                }
            }

            GridPosition toPosition = new GridPosition(highestX, highestY);
            List<Cell> path = GridManager.Instance.FindPath(this, toPosition);

            if (path == null || path.Count == 0)
                return;

            NetworkManager.Instance.EntityAsksForPath(this, path[path.Count - 1], this.CurrentGrid);
        }

        #endregion

        // All Attack Behaviours
        #region Attack Behaviours
        /// <summary>
        /// Will attack the player according to the target behavior funtion
        /// </summary>
        private void AttackClosestRandom() {
            if (!CanAutoAttack) {
                return;
            }
            TargetBehaviors[attackType]();
            if (CurrentTarget != null) {
                Debug.Log("Try to aa");
                AutoAttack(CurrentTarget.EntityCell);
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

        /// <summary>
        /// Will target the player with highest health (random btwn two at the same health)
        /// <summary>
        private void TargetHighestRandom()
        {
            CharacterEntity[] PlayersByHealth = PlayersOrderedByHealth("Max", out int sameHealth);
            var HighestPlayer = PlayersByHealth[UnityEngine.Random.Range(0, sameHealth)];
            CurrentTarget = HighestPlayer;
        }

        /// <summary>
        /// Will target the player with lowest health (random btwn two at the same health)
        /// <summary>
        private void TargetLowestRandom()
        {
            CharacterEntity[] PlayersByHealth = PlayersOrderedByHealth("Min", out int sameHealth);
            var LowestPlayer = PlayersByHealth[UnityEngine.Random.Range(0, sameHealth)];
            CurrentTarget = LowestPlayer;
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

        public CharacterEntity[] PlayersOrderedByHealth(string type, out int sameHealth)
        {
            var Players = CurrentGrid.GridEntities.FindAll(x => x.IsAlly);
            var Provoking = Players.FindAll(x => x.Provoke);

            int[] Healths = new int[Players.Count];
            //Get all distances to player (from path) and set them in distances
            for (int i = 0; i < Players.Count; i++)
            {
                CharacterEntity item = Players[i];
                var path = GridManager.Instance.FindPath(this, item.EntityCell.PositionInGrid);
                Healths[i] = item.Health;
            }
            CharacterEntity[] orderedAllies = new CharacterEntity[Healths.Length];
            int ActualPlayer = 0;
            int TotalOfSameHealth = 0;
            bool recordSameHealth = true;
            int LastHealth = -1;
            int length = Healths.Length;
            //for all distances take the smallest 
            for (int i = 0; i < length; i++)
            {
                int Health = SetPlayerPositionInTargetList(i, type, ref Healths, Players, ref orderedAllies, ActualPlayer);
                if (LastHealth == Health && recordSameHealth)
                {
                    TotalOfSameHealth++;
                }
                else if (i > 0)
                {
                    recordSameHealth = false;
                }
                ActualPlayer++;
                LastHealth = Health;
            }
            sameHealth = TotalOfSameHealth;
            return orderedAllies;
        }

        private int SetPlayerPositionInTargetList(int i, string type, ref int[] RefArray, List<CharacterEntity> Players, ref CharacterEntity[] TargetList, int ActualIndex) {
            int Value;
            // Depending on the type of distance (close or far) get the good distances
            if (type == "Min") {
                Value = RefArray.Min();
            } else {
                Value = RefArray.Max();
            }
            int Index = Array.IndexOf(RefArray, Value);
            //if the player is provoking then make it first priority
            if (Players[Index].Provoke) {
                MakeProvokedFirst(Players[Index], ref TargetList, Players);
            } else {
                TargetList[ActualIndex] = Players[Index];
            }
            ArrayHelper.RemoveAt(ref RefArray, Index); ;

            return Value;
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
            return new Spell(CombatManager.Instance.PossibleAutoAttacks.Find(x => x is DamageStrengthSpellAction ));

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