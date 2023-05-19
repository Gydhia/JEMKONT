using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Spells;
using EODE.Wonderland;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace DownBelow.Entity
{
    public class PipeBomb : NonCharacterEntity
    {
        public EntityStatistics StatAffected = EntityStatistics.Health;
        public int Value = 3;
        [Range(0, 8)]
        public int NumberOfDuplicates;

        public override void Init(Cell AttachedCell, int TurnsLeft, CharacterEntity RefEntity, NCEPreset prefab)
        {
            base.Init(AttachedCell, TurnsLeft, RefEntity, prefab);
            foreach (var item in CombatManager.Instance.PlayingEntities.Where(e => !e.IsAlly))
            {
                item.OnEnteredCell += CheckIfInRange;
            }
        }

        private void CheckIfInRange(CellEventData Data)
        {
            //Calculate the path between the cells, path.Count-1 <= range applybuff.
            //TODO: ApplyBuff!
            if (Data.Cell.EntityIn is EnemyEntity enemy && Data.Cell == AttachedCell)
            {
                Explode(enemy);
            }
        }

        private void Explode(EnemyEntity entity)
        {
            //Effect
            NetworkManager.Instance.EntityAskToBuffAction(new Spell_Stats(new SpellData_Stats(!(Value < 0), StatAffected, System.Math.Abs(Value)), RefEntity, AttachedCell, null, null, 0));

            //Duplicate on random free neighbours
            Cell[] randomDupeCells = new Cell[NumberOfDuplicates];
            var neighbours = GridManager.Instance.GetNormalNeighbours(AttachedCell, AttachedCell.RefGrid);
            for (int i = 0;i < randomDupeCells.Length;i++)
            {
                var rand = neighbours.Random();
                randomDupeCells[i] = rand;
                neighbours.Remove(rand);
            }
            foreach (var cell in randomDupeCells)
            {
                NetworkManager.Instance.EntityAskToBuffAction(new Spell_SummonNCE(new SpellData_Summon(PresetRef), RefEntity, cell, null, null, 0));

            }

            DestroyEntity();
        }
    }
}
