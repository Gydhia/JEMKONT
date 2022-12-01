using Jemkont.Events;
using Jemkont.GridSystem;
using Jemkont.Managers;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Jemkont.Spells.Alterations;

namespace Jemkont.Entity {
    public abstract class CharacterEntity : MonoBehaviour {
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

        public event GameEventData.Event OnTurnBegun;
        public event GameEventData.Event OnTurnEnded;

        private EntityStats RefStats;

        public List<Alteration> Alterations = new();

        public UnityEngine.UI.Slider HealthFill;
        public UnityEngine.UI.Slider ShieldFill;

        public int TurnOrder;
        public bool IsAlly = true;
        public GridPosition EntityPosition = GridPosition.zero;
        public CombatGrid CurrentGrid;

        public List<CharacterEntity> Summons;

        public int MaxHealth { get => RefStats.Health; set => RefStats.Health = value; }
        public Dictionary<EntityStatistics,int> Statistics;
        public int Health { get => Statistics[EntityStatistics.Health]; }
        public int Shield { get => Statistics[EntityStatistics.Shield]; }
        public int Strenght { get => Statistics[EntityStatistics.Strenght]; }
        public int Dexterity { get => Statistics[EntityStatistics.Dexterity]; }
        public int Movement { get => Snared ? 0 : Statistics[EntityStatistics.Movement]; }
        public int Mana { get => Statistics[EntityStatistics.Mana]; }

        #region alterationBooleans
        public bool Snared { get => Alterations.Any(x => x.GetType() == typeof(SnareAlteration)); }
        public bool Stunned { get => Alterations.Any(x => x.GetType() == typeof(StunAlteration)); }
        public bool Disarmed { get => Alterations.Any(x => x.GetType() == typeof(DisarmedAlteration)); }
        public bool Critical { get => Alterations.Any(x => x.GetType() == typeof(CriticalAlteration)); }
        public bool Dodge { get => Alterations.Any(x => x.GetType() == typeof(DodgeAlteration)); }
        public bool Camouflage { get => Alterations.Any(x => x.GetType() == typeof(CamouflageAlteration)); }
        public bool Provoke { get => Alterations.Any(x => x.GetType() == typeof(ProvokeAlteration)); }
        public bool Ephemeral { get => Alterations.Any(x => x.GetType() == typeof(EphemeralAlteration)); }
        public bool Confusion { get => Alterations.Any(x => x.GetType() == typeof(ConfusionAlteration)); }
        public bool Shattered { get => Alterations.Any(x => x.GetType() == typeof(ShatteredAlteration)); }
        public bool DoT { get => Alterations.Any(x => x.GetType() == typeof(DoTAlteration)); }
        public bool Spirit { get => Alterations.Any(x => x.GetType() == typeof(SpiritAlteration)); }
        public bool Bubbled { get => Alterations.Any(x => x.GetType() == typeof(BubbledAlteration)); }
        public bool MindControl { get => Alterations.Any(x => x.GetType() == typeof(MindControlAlteration)); }
        #endregion
        public bool TryGoTo(Cell destination,int cost) {
            this.ApplyMovement(-cost);

            this.CurrentGrid.Cells[this.EntityPosition.longitude,this.EntityPosition.latitude].EntityIn = null;

            this.EntityPosition = new GridPosition(destination.Datas.heightPos,destination.Datas.widthPos);
            this.transform.position = destination.WorldPosition;

            destination.EntityIn = this;

            return true;
        }

        public void Start() {
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
        public void UpdateUILife(SpellEventData data) {
            this.HealthFill.value = this.Health;
        }

        public void UpdateUIShield(SpellEventData data) {
            this.ShieldFill.value = this.Shield;
        }

        private void LateUpdate() {
            this.HealthFill.transform.LookAt(Camera.main.transform.position);
            this.ShieldFill.transform.LookAt(Camera.main.transform.position);
        }
        public void EndTurn() {
            OnTurnEnded?.Invoke(new GameEventData());
        }

        public virtual void StartTurn() {
            OnTurnBegun?.Invoke(new GameEventData());
            this.ReinitializeStat(EntityStatistics.Movement);
            this.ReinitializeStat(EntityStatistics.Mana);
            foreach (Alteration alteration in Alterations) {
                alteration.Apply(this);
            }
            if (Stunned) {
                EndTurn();
                return;
            }
            GridManager.Instance.ShowPossibleMovements(this);
        }

        public void Init(EntityStats stats,Cell refCell,CombatGrid refGrid) {
            this.transform.position = refCell.WorldPosition;
            this.EntityPosition = refCell.PositionInGrid;
            this.CurrentGrid = refGrid;

            this.RefStats = stats;
            this.Statistics = new Dictionary<EntityStatistics,int>();

            this.Statistics.Add(EntityStatistics.Health,stats.Health);
            this.Statistics.Add(EntityStatistics.Shield,stats.BaseShield);
            this.Statistics.Add(EntityStatistics.Strenght,stats.Strenght);
            this.Statistics.Add(EntityStatistics.Dexterity,stats.Dexterity);
            this.Statistics.Add(EntityStatistics.Movement,stats.Movement);
            this.Statistics.Add(EntityStatistics.Mana,stats.Mana);
        }

        public void ReinitializeStat(EntityStatistics stat) {
            switch (stat) {
                case EntityStatistics.Health: this.Statistics[EntityStatistics.Health] = this.RefStats.Health; break;
                case EntityStatistics.Mana: this.Statistics[EntityStatistics.Mana] = this.RefStats.Mana; break;
                case EntityStatistics.Movement: this.Statistics[EntityStatistics.Movement] = this.RefStats.Movement; break;
                case EntityStatistics.Strenght: this.Statistics[EntityStatistics.Strenght] = this.RefStats.Strenght; break;
                case EntityStatistics.Dexterity: this.Statistics[EntityStatistics.Dexterity] = this.RefStats.Dexterity; break;
            }
        }

        public void ApplyHealth(int value,bool overShield) {
            if (value > 0) {
                // Check overheal
                if (this.Health + value > this.RefStats.Health)
                    Statistics[EntityStatistics.Health] = this.RefStats.Health;
                else
                    Statistics[EntityStatistics.Health] += value;

                this.OnHealthAdded?.Invoke(new SpellEventData(this,value));
            } else {
                // Better to understand like that :)
                value = -value;

                int onShield = this.Shield - value > 0 ? value : this.Shield;
                int onLife = overShield ? value : -(onShield - value);

                Statistics[EntityStatistics.Health] -= onLife;

                if (!overShield) {
                    Statistics[EntityStatistics.Shield] -= onShield;
                    this.OnShieldRemoved?.Invoke(new SpellEventData(this,onShield));
                }
                this.OnHealthRemoved?.Invoke(new SpellEventData(this,onLife));
            }
        }

        public void ApplyShield(int value) {
            Statistics[EntityStatistics.Shield] += value;

            if (value > 0)
                this.OnShieldAdded?.Invoke(new SpellEventData(this,value));
            else
                this.OnShieldRemoved?.Invoke(new SpellEventData(this,-value));
        }

        public void ApplyMana(int value) {
            Statistics[EntityStatistics.Mana] += value;

            if (value > 0)
                this.OnManaAdded?.Invoke(new SpellEventData(this,value));
            else
                this.OnManaRemoved?.Invoke(new SpellEventData(this,-value));
        }

        public void ApplyMovement(int value) {
            Statistics[EntityStatistics.Movement] += value;

            if (value > 0)
                this.OnMovementAdded?.Invoke(new SpellEventData(this,value));
            else
                this.OnMovementRemoved?.Invoke(new SpellEventData(this,-value));
        }

        public void ApplyStrenght(int value) {
            Statistics[EntityStatistics.Strenght] += value;

            if (value > 0)
                this.OnStrenghtAdded?.Invoke(new SpellEventData(this,value));
            else
                this.OnStrenghtRemoved?.Invoke(new SpellEventData(this,-value));
        }

        public void ApplyDexterity(int value) {
            Statistics[EntityStatistics.Dexterity] += value;

            if (value > 0)
                this.OnDexterityAdded?.Invoke(new SpellEventData(this,value));
            else
                this.OnDexterityRemoved?.Invoke(new SpellEventData(this,-value));
        }
        public override string ToString() {
            return @$"Name : {name}
IsAlly : {IsAlly}
GridPos : {EntityPosition}";
        }
        public void AddAlteration(EAlterationType type,int duration) {
            Alteration alteration;
            alteration = type switch {
                EAlterationType.Stun => new StunAlteration(duration),
                EAlterationType.Snare => new SnareAlteration(duration),
                EAlterationType.Disarmed => new DisarmedAlteration(duration),
                EAlterationType.Critical => new CriticalAlteration(duration),
                EAlterationType.Dodge => new DodgeAlteration(duration),
                EAlterationType.Camouflage => new CamouflageAlteration(duration),
                EAlterationType.Provoke => new ProvokeAlteration(duration),
                EAlterationType.Ephemeral => new EphemeralAlteration(duration),
                EAlterationType.Confusion => new ConfusionAlteration(duration),
                EAlterationType.Shattered => new ShatteredAlteration(duration),
                EAlterationType.DoT => new DoTAlteration(duration,2),//Idfk how much dmg
                EAlterationType.Spirit => new SpiritAlteration(duration),
                EAlterationType.Bubbled => new BubbledAlteration(duration),
                EAlterationType.MindControl => new MindControlAlteration(duration),
                _ => throw new System.NotImplementedException($"NEED TO IMPLEMENT ENUM MEMBER {type}"),
            };
            var found = Alterations.Find(x => x.GetType() == alteration.GetType());
            if (found != null) {
                //TODO : GD? Add Duration? Set duration?
            } else {
                Alterations.Add(alteration);
            }

            alteration.Setup(this);
            if (alteration.ClassicCountdown) {
                this.OnTurnEnded += alteration.DecrementAlterationCountdown;
            } else {
                switch (alteration) {
                    case CriticalAlteration crit:
                        //TODO: Plug decrement on a OnDamageDoneEvent.
                        break;
                    case DodgeAlteration dodge:
                        //TODO: Plug decrement manually/OnBeforeDamage? Dodge has 50% chance of nullifying damage.
                        break;
                    case CamouflageAlteration camo:
                        this.OnTurnEnded += camo.DecrementAlterationCountdown;
                        //TODO: Also add on damage taken.
                        //this.OnHealthRemoved+= camo.DecrementAlterationCountdown; doesn't work. @kiki
                        break;
                    case ProvokeAlteration prov:
                        this.OnTurnEnded += prov.DecrementAlterationCountdown;
                        //TODO: Also add on damage taken.
                        //this.OnHealthRemoved+= prov.DecrementAlterationCountdown; doesn't work. @kiki
                        break;
                    case ShatteredAlteration shat:
                        this.OnTurnEnded+= shat.DecrementAlterationCountdown;
                        //TODO: Also add on damage taken.
                        //this.OnHealthRemoved+= prov.DecrementAlterationCountdown; doesn't work. @kiki
                        break;
                    case BubbledAlteration bubble:
                        this.OnTurnEnded += bubble.DecrementAlterationCountdown;
                        //TODO: Also add on damage taken.
                        //this.OnHealthRemoved+= prov.DecrementAlterationCountdown; doesn't work. @kiki
                        break;
                    default:
                        Debug.LogError("ALTERATION ERROR: SPECIAL COUNTDOWN NOT IMPLEMENTED.");
                        break;
                }
            }
        }

    }
}

