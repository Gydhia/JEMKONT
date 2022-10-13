using Jemkont.GridSystem;
using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Jemkont.Entity
{
    public abstract class CharacterEntity : MonoBehaviour
    {
        public delegate void StatModified();

        public event StatModified OnHealthRemoved;
        public event StatModified OnHealthAdded;
        public event StatModified OnStrenghtRemoved;
        public event StatModified OnStrenghtAdded;
        public event StatModified OnDexterityRemoved;
        public event StatModified OnDexterityAdded;
        public event StatModified OnMovementRemoved;
        public event StatModified OnMovementAdded;
        public event StatModified OnManaRemoved;
        public event StatModified OnManaAdded;

        private EntityStats RefStats;

        public int TurnOrder;
        public bool IsAlly = true;
        public GridPosition EntityPosition = GridPosition.zero;
        public CombatGrid CurrentGrid;

        public List<CharacterEntity> Invocations;


        public Dictionary<EntityStatistics, int> Statistics;
        public int Health 
        { 
            get => Statistics[EntityStatistics.Health];
            private set 
            {
                Statistics[EntityStatistics.Health] += value;
                if (value > 0)
                    this.OnHealthAdded?.Invoke();
                else
                    this.OnHealthRemoved?.Invoke();
            }
        }
        public int Strenght 
        { 
            get => Statistics[EntityStatistics.Strenght]; 
            private set 
            {
                Statistics[EntityStatistics.Strenght] += value;
                if (value > 0)
                    this.OnStrenghtAdded?.Invoke();
                else
                    this.OnStrenghtRemoved?.Invoke();
            } 
        }
        public int Dexterity
        { 
            get => Statistics[EntityStatistics.Dexterity]; 
            private set 
            { 
                Statistics[EntityStatistics.Dexterity] += value;
                if (value > 0)
                    this.OnDexterityAdded?.Invoke();
                else
                    this.OnDexterityRemoved?.Invoke();
            } 
        }
        public int Movement
        { 
            get => Statistics[EntityStatistics.Movement]; 
            private set 
            { 
                Statistics[EntityStatistics.Movement] += value;
                if (value > 0)
                    this.OnMovementAdded?.Invoke();
                else
                    this.OnMovementRemoved?.Invoke();
            } 
        }
        public int Mana 
        {
            get => Statistics[EntityStatistics.Mana]; 
            private set 
            { 
                Statistics[EntityStatistics.Mana] += value;
                if (value > 0)
                    this.OnManaAdded?.Invoke();
                else
                    this.OnManaRemoved?.Invoke();
            } 
        }

        public bool TryGoTo(Cell destination, int cost)
        {
            this.Movement = -cost;

            this.EntityPosition = new GridPosition(destination.Datas.heightPos, destination.Datas.widthPos);
            this.transform.position = destination.WorldPosition;
            
            return true;
        }

        public void EndTurn()
        {

        }

        public void StartTurn()
        {
            this.ReinitializeStat(EntityStatistics.Movement);
            this.ReinitializeStat(EntityStatistics.Mana);

            GridManager.Instance.ShowPossibleMovements(this);
        }

        public void Init(EntityStats stats, Cell refCell, CombatGrid refGrid)
        {
            this.transform.position = refCell.WorldPosition;
            this.EntityPosition = refCell.PositionInGrid;
            this.CurrentGrid = refGrid;

            this.RefStats = stats;
            this.Statistics = new Dictionary<EntityStatistics, int>();

            this.Statistics.Add(EntityStatistics.Health, stats.Health);
            this.Statistics.Add(EntityStatistics.Strenght, stats.Strenght);
            this.Statistics.Add(EntityStatistics.Dexterity, stats.Dexterity);
            this.Statistics.Add(EntityStatistics.Movement, stats.Movement);
            this.Statistics.Add(EntityStatistics.Mana, stats.Mana);
        }

        public void ReinitializeStat(EntityStatistics stat)
        {
            switch (stat)
            {
                case EntityStatistics.Health: this.Statistics[EntityStatistics.Health] = this.RefStats.Health; break;
                case EntityStatistics.Mana: this.Statistics[EntityStatistics.Mana] = this.RefStats.Mana; break;
                case EntityStatistics.Movement: this.Statistics[EntityStatistics.Movement] = this.RefStats.Movement; break;
                case EntityStatistics.Strenght: this.Statistics[EntityStatistics.Strenght] = this.RefStats.Strenght; break;
                case EntityStatistics.Dexterity: this.Statistics[EntityStatistics.Dexterity] = this.RefStats.Dexterity; break;
            }
        }
    }
}

