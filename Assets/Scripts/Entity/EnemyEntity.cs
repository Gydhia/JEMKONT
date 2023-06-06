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


namespace DownBelow.Entity 
{
    public enum MovementType { Straight = 1, StraightToRange = 2, Flee = 3, Kite = 4 };
    public enum AttackType { ClosestRandom = 1, FarthestRandom = 2, LowestRandom = 3, HighestRandom = 4, Random = 5 };


    public class EnemyEntity : CharacterEntity 
    {

        public EntityPreset EnemyStyle;
        CharacterEntity cachedAllyToAttack;

        public CharacterEntity CurrentTarget;

        #region Movement
        public MovementType movementType = MovementType.Straight;
        #endregion

        #region Attack
        public AttackType attackType = AttackType.ClosestRandom;
        #endregion

        #region Target
        public AttackType TargetType = AttackType.ClosestRandom;
        #endregion


        public override void Init(Cell refCell, WorldGrid refGrid, int order = 0) 
        {
            base.Init(refCell, refGrid);

            this.UID = refGrid.UName + this.EnemyStyle.UName + order;

            this.FireEntityInited();
        }

        public override void StartTurn() 
        {
            base.StartTurn();
        }


        public EntityAction[] CreateEnemyActions()
        {
            var targettingAction = new TargettingAction(this, this.EntityCell);

            var movementAction = new EnemyMovementAction(this, this.EntityCell);
            movementAction.Init(this.movementType);
            movementAction.SetContextAction(targettingAction);

            var attackAction = new AttackingAction(this, this.EntityCell);
            attackAction.SetContextAction(targettingAction);

            var endTurnAction = new EndTurnAction(this, this.EntityCell);

            // TODO : Implement a deck for enemy
            //var attackAction = new Spell();

            return new EntityAction[4] { targettingAction, movementAction, attackAction, endTurnAction };
        }

        // All Attack Behaviours
        #region Attack Behaviours
        /// <summary>
        /// Will attack the closest player (random btwn two at same range)
        /// </summary>
        private void AttackClosestRandom() 
        {
            //if (!CanAutoAttack) 
            //    return;
            
            //_targetBehaviors[attackType].ExecuteAction();

            //if (cachedAllyToAttack != null) 
            //    AutoAttack(cachedAllyToAttack.EntityCell);
        }
        #endregion


        #region Player Sensors
        /// <summary>
        /// Returns Players ordered from closest to farthest / farthest to Closest. If an entity is provoking, clones it to put in index 0 of the array.
        /// Arguments : Type = "Min" or "Max" depending on getting the closest or farthest
        /// </summary>
        public CharacterEntity[] PlayersOrderedByDistance(string type, out int sameDist) {
            var Players = CurrentGrid.GridEntities.FindAll(x => x.IsAlly && x.Health > 0);

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