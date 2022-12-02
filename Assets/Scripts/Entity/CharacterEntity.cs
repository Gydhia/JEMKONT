using Jemkont.Events;
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

        public event SpellEventData.Event OnHealthRemoved;
        public event SpellEventData.Event OnHealthAdded;
        public event SpellEventData.Event OnShieldRemoved;
        public event SpellEventData.Event OnShieldAdded;
        public event SpellEventData.Event OnStrenghtRemoved;
        public event SpellEventData.Event OnStrenghtAdded;
        public event SpellEventData.Event OnDexterityRemoved;
        public event SpellEventData.Event OnDexterityAdded;
        public event SpellEventData.Event OnMovementRemoved;
        public event SpellEventData.Event OnMovementAdded;
        public event SpellEventData.Event OnManaRemoved;
        public event SpellEventData.Event OnManaAdded;

        private EntityStats RefStats;


        public UnityEngine.UI.Slider HealthFill;
        public UnityEngine.UI.Slider ShieldFill;

        public int TurnOrder;
        public bool IsAlly = true;
        public Cell EntityCell = null;
        public WorldGrid CurrentGrid;

        public List<CharacterEntity> Summons;

        public int MaxHealth { get => RefStats.Health; set => RefStats.Health = value; }
        public Dictionary<EntityStatistics, int> Statistics;
        public int Health {  get => Statistics[EntityStatistics.Health]; }
        public int Shield { get => Statistics[EntityStatistics.Shield]; }
        public int Strenght { get => Statistics[EntityStatistics.Strenght]; }
        public int Dexterity { get => Statistics[EntityStatistics.Dexterity]; }
        public int Movement { get => Statistics[EntityStatistics.Movement]; }
        public int Mana { get => Statistics[EntityStatistics.Mana]; }

        public bool TryGoTo(Cell destination, int cost)
        {
            this.ApplyMovement(-cost);

            this.EntityCell.EntityIn = null;

            this.EntityCell = destination;
            this.transform.position = destination.WorldPosition;

            destination.EntityIn = this;

            return true;
        }

        public void Start()
        {
            this.OnHealthAdded += UpdateUILife;
            this.OnHealthRemoved += UpdateUILife;

            this.OnShieldAdded += UpdateUIShield;
            this.OnShieldRemoved += UpdateUIShield;

            this.HealthFill.maxValue = this.Statistics[EntityStatistics.Health];
            this.HealthFill.minValue = 0;
            this.HealthFill.value = this.Health;

            this.ShieldFill.maxValue = this.Statistics[EntityStatistics.Health];
            this.ShieldFill.minValue = 0;
            this.ShieldFill.value = 0;
        }
        public void UpdateUILife(SpellEventData data)
        {
            this.HealthFill.value = this.Health;
        }

        public void UpdateUIShield(SpellEventData data)
        {
            this.ShieldFill.value = this.Shield;
        }

        private void LateUpdate()
        {
            this.HealthFill.transform.LookAt(Camera.main.transform.position);
            this.ShieldFill.transform.LookAt(Camera.main.transform.position);
        }
        public void EndTurn()
        {

        }

        public virtual void StartTurn()
        {
            this.ReinitializeStat(EntityStatistics.Movement);
            this.ReinitializeStat(EntityStatistics.Mana);
            GridManager.Instance.ShowPossibleCombatMovements(this);
        }

        public void Init(EntityStats stats, Cell refCell, WorldGrid refGrid)
        {
            this.transform.position = refCell.WorldPosition;
            this.EntityCell = refCell;
            this.CurrentGrid = refGrid;

            this.RefStats = stats;
            this.Statistics = new Dictionary<EntityStatistics, int>();

            this.Statistics.Add(EntityStatistics.Health, stats.Health);
            this.Statistics.Add(EntityStatistics.Shield, stats.BaseShield);
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

        public void ApplyHealth(int value, bool overShield)
        {
            if (value > 0)
            {
                // Check overheal
                if(this.Health + value > this.RefStats.Health)
                    Statistics[EntityStatistics.Health] = this.RefStats.Health;
                else
                    Statistics[EntityStatistics.Health] += value;

                this.OnHealthAdded?.Invoke(new SpellEventData(this, value));
            }
            else
            {
                // Better to understand like that :)
                value = -value;

                int onShield = this.Shield - value > 0 ? value : this.Shield;
                int onLife = overShield ? value : -(onShield - value);

                Statistics[EntityStatistics.Health] -= onLife;

                if (!overShield) {
                    Statistics[EntityStatistics.Shield] -= onShield;
                    this.OnShieldRemoved?.Invoke(new SpellEventData(this, onShield));
                }
                this.OnHealthRemoved?.Invoke(new SpellEventData(this, onLife));
            }
        }

        public void ApplyShield(int value)
        {
            Statistics[EntityStatistics.Shield] += value;

            if (value > 0)
                this.OnShieldAdded?.Invoke(new SpellEventData(this, value));
            else
                this.OnShieldRemoved?.Invoke(new SpellEventData(this, -value));
        }

        public void ApplyMana(int value)
        {
            Statistics[EntityStatistics.Mana] += value;

            if (value > 0)
                this.OnManaAdded?.Invoke(new SpellEventData(this, value));
            else
                this.OnManaRemoved?.Invoke(new SpellEventData(this, -value));
        }

        public void ApplyMovement(int value)
        {
            Statistics[EntityStatistics.Movement] += value;

            if (value > 0)
                this.OnMovementAdded?.Invoke(new SpellEventData(this, value));
            else
                this.OnMovementRemoved?.Invoke(new SpellEventData(this, -value));
        }

        public void ApplyStrenght(int value)
        {
            Statistics[EntityStatistics.Strenght] += value;

            if (value > 0)
                this.OnStrenghtAdded?.Invoke(new SpellEventData(this, value));
            else
                this.OnStrenghtRemoved?.Invoke(new SpellEventData(this, -value));
        }

        public void ApplyDexterity(int value)
        {
            Statistics[EntityStatistics.Dexterity] += value;

            if (value > 0)
                this.OnDexterityAdded?.Invoke(new SpellEventData(this, value));
            else
                this.OnDexterityRemoved?.Invoke(new SpellEventData(this, -value));
        }
        public override string ToString() {

            return @$"Name : {name}
IsAlly : {IsAlly}
GridPos : {EntityCell}";
        }
    }
}

