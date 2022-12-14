using EODE.Wonderland;

using DownBelow.Spells;
using DownBelow.Spells.Alterations;
using DownBelow.GridSystem;
using DownBelow.Managers;
using MyBox;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Math = System.Math;


namespace DownBelow.Entity {
    public class EnemyEntity : CharacterEntity {
        public EntityPreset EnemyStyle;
        CharacterEntity cachedAllyToAttack;
        public override void Init(EntityStats stats,Cell refCell,WorldGrid refGrid,int order = 0) {
            base.Init(stats,refCell,refGrid);

            this.UID = refGrid.UName + this.EnemyStyle.UName + order;
        }

        public override void StartTurn() {

            this.ReinitializeStat(EntityStatistics.Speed);
            this.ReinitializeStat(EntityStatistics.Mana);
            if (Stunned || Sleeping) EndTurn();
            var TargetPosition = GetTargetPosition();
            GridManager.Instance.FindPath(this,TargetPosition,true);

            if (GridManager.Instance.Path.Count > 0 && this.Speed > 0) {
                GridManager.Instance.ShowPossibleCombatMovements(this);
                if(!Confused) NetworkManager.Instance.EntityAsksForPath(this, GridManager.Instance.Path[GridManager.Instance.Path.Count - 1],this.CurrentGrid);
            }
            //Moved (or not if was in range); and will now Autoattack:
            if (CanAutoAttack) {
                if (cachedAllyToAttack != null) {
                    //LETSGOOOOOO FIREEEEEEEEEEEEE
                    AutoAttack(cachedAllyToAttack.EntityCell);
                }
            }
            //TODO: ENEMY SPELL?
        }
        /// <summary>
        /// gets the position the enemy should go, depending on it's AI.
        /// </summary>
        public GridPosition GetTargetPosition() {
            var ClosestAlly = AlliesOrdered()[0];
            if (isInAttackRange(ClosestAlly.EntityCell)) {
                cachedAllyToAttack = ClosestAlly;
                return this.EntityCell.PositionInGrid;
            }
            return ClosestAlly.EntityCell.PositionInGrid;
        }
        /// <summary>
        /// Returns allies ordered from closest to farthest. If an entity is provoking, clones it to put in index 0 of the array.
        /// </summary>
        public CharacterEntity[] AlliesOrdered() {
            var Allies = CurrentGrid.GridEntities.FindAll(x => x.IsAlly);
            var Provoking = Allies.FindAll(x => x.Provoke);
            int[] distances = new int[Allies.Count];
            int[] provokingDistances = new int[Provoking.Count];
            for (int i = 0;i < Allies.Count;i++) {
                CharacterEntity item = Allies[i];
                GridManager.Instance.FindPath(this,item.EntityCell.PositionInGrid,true);
                distances[i] = GridManager.Instance.Path.Count;
            }
            for (int i = 0;i < Provoking.Count;i++) {
                CharacterEntity item = Provoking[i];
                GridManager.Instance.FindPath(this,item.EntityCell.PositionInGrid,true);
                provokingDistances[i] = GridManager.Instance.Path.Count;
            }
            CharacterEntity[] orderedAllies = new CharacterEntity[distances.Length];
            CharacterEntity[] ProvOrderedAllies = new CharacterEntity[provokingDistances.Length];
            int ActualPlayer = 0;
            while (distances.Length > 0) {
                int minIndex = distances.IndexOfItem(distances.Min());
                orderedAllies[ActualPlayer] = Allies[minIndex];
                distances = distances.RemoveAt(minIndex);
            }
            while (provokingDistances.Length > 0) {
                int minIndex = provokingDistances.IndexOfItem(provokingDistances.Min());
                orderedAllies[ActualPlayer] = Allies[minIndex];
                provokingDistances = provokingDistances.RemoveAt(minIndex);
            }
            if (ProvOrderedAllies.Length > 0 && ProvOrderedAllies[0] != orderedAllies[0]) {
                orderedAllies = orderedAllies.InsertAt(0);
                orderedAllies[0] = ProvOrderedAllies[0];
            }
            //Distances[i] = x tel que x = la distance entre l'enemy et le players[i]. trouver i => Distance[i] est le plus petit de tous, return players[i]
            return orderedAllies;
        }
        public override Spell AutoAttackSpell() {
            return new Spell(CombatManager.Instance.PossibleAutoAttacks.Find(x => x.Is<DamageStrengthSpellAction>()));

            Cell RandomCellInRange() {
                List<Cell> cells = new();
                foreach (var item in CurrentGrid.Cells) {
                    cells.Add(item);
                }
                return cells.FindAll(x => (Mathf.Abs(x.PositionInGrid.latitude - this.EntityCell.PositionInGrid.latitude) + Mathf.Abs(x.PositionInGrid.longitude - this.EntityCell.PositionInGrid.longitude)) >= Speed).GetRandom();

            }
        }
    }
}