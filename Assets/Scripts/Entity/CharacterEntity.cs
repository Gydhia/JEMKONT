using DownBelow.Spells;
using DownBelow.Spells.Alterations;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.Serialization;
using System;
using TMPro;
using DG.Tweening;

namespace DownBelow.Entity
{
    public abstract class CharacterEntity : MonoBehaviour
    {
        public delegate void StatModified();

        public event SpellEventData.Event OnHealthRemoved;
        public event SpellEventData.Event OnHealthAdded;
        public event SpellEventData.Event OnStrengthRemoved;
        public event SpellEventData.Event OnStrengthAdded;
        public event SpellEventData.Event OnSpeedRemoved;
        public event SpellEventData.Event OnSpeedAdded;
        public event SpellEventData.Event OnManaRemoved;
        public event SpellEventData.Event OnManaAdded;
        public event SpellEventData.Event OnDefenseRemoved;
        public event SpellEventData.Event OnDefenseAdded;
        public event SpellEventData.Event OnRangeRemoved;
        public event SpellEventData.Event OnRangeAdded;

        /// <summary>
        /// When you give an alteration to someone else.
        /// </summary>
        public event SpellEventData.Event OnAlterationGiven;

        /// <summary>
        /// When you receive an alteration from someone else.
        /// </summary>
        public event SpellEventData.Event OnAlterationReceived;

        public event EntityEventData.Event OnEntityTargetted;

        public event GameEventData.Event OnTurnBegun;
        public event GameEventData.Event OnTurnEnded;
        public event GameEventData.Event OnTryTakeDamage;
        public event GameEventData.Event OnDamageTaken;
        public event GameEventData.Event OnDeath;


        public event CellEventData.Event OnEnteredCell;
        public event CellEventData.Event OnExitedCell;

        public void FireExitedCell()
        {
            this.EntityCell.EntityIn = null;

            this.OnExitedCell?.Invoke(new CellEventData(this.EntityCell));
        }

        public void FireEnteredCell(Cell cell)
        {
            this.EntityCell = cell;

            this.OnEnteredCell?.Invoke(new CellEventData(cell));
        }

        public void FireEntityTargetted(CharacterEntity targeter)
        {
            this.OnEntityTargetted?.Invoke(new(targeter));
        }

        protected EntityStats RefStats;

        [OdinSerialize] public List<Alteration> Alterations = new();

        public TextMeshProUGUI healthText;

        public int TurnOrder;
        public bool IsAlly = true;

        public bool IsPlayingEntity = false;

        // Used for NPC. Determined UID to parse over network. 
        // TODO: Change it to a real Guid later
        public string UID = string.Empty;

        public WorldGrid CurrentGrid;

        // Movements
        public bool IsMoving = false;

        public Cell _entityCell;
        public Cell EntityCell
        {
            get { return this._entityCell; }
            set
            {
                if(_entityCell != null)
                    this._entityCell.EntityIn = null;

                this._entityCell = value;
                this._entityCell.EntityIn = this;
            }
        }

        public Cell NextCell = null;
        protected Coroutine moveCor = null;
        public List<Cell> CurrentPath;

        public List<CharacterEntity> Summons;

        public bool CanAutoAttack;


        #region alterationBooleans

        public bool Snared
        {
            get => Alterations.Any(x => x.GetType() == typeof(SnareAlteration)) &&
                   !Alterations.Any(x => Alteration.overrides[EAlterationType.Snare].Contains(x.ToEnum()));
        } //DONE

        public bool Stunned
        {
            get => Alterations.Any(x => x.GetType() == typeof(StunAlteration)) &&
                   !Alterations.Any(x => Alteration.overrides[EAlterationType.Stun].Contains(x.ToEnum()));
        } //DONE

        public bool Shattered
        {
            get => Alterations.Any(x => x.GetType() == typeof(ShatteredAlteration)) && !Alterations.Any(x =>
                Alteration.overrides[EAlterationType.Shattered].Contains(x.ToEnum()));
        } //DONE

        public bool DoT
        {
            get => Alterations.Any(x => x.GetType() == typeof(DoTAlteration)) &&
                   !Alterations.Any(x => Alteration.overrides[EAlterationType.DoT].Contains(x.ToEnum()));
        } //DONE

        public bool Bubbled
        {
            get => Alterations.Any(x => x.GetType() == typeof(BubbledAlteration)) &&
                   !Alterations.Any(x => Alteration.overrides[EAlterationType.Bubbled].Contains(x.ToEnum()));
        } //DONE

        public bool Sleeping
        {
            get => Alterations.Any(x => x.GetType() == typeof(SleepAlteration)) &&
                   !Alterations.Any(x => Alteration.overrides[EAlterationType.Sleep].Contains(x.ToEnum()));
        } //DONE

        /// <summary>
        /// Returns the current Damage Up/Down alteration value. returns 0 of there isn't any.
        /// </summary>
        public int DmgUpDown
        {
            get
            {
                var alt = Alterations.Find(x => x is DmgUpDownAlteration);
                if (alt != null &&
                    !Alterations.Any(x => Alteration.overrides[EAlterationType.DmgUpDown].Contains(x.ToEnum())))
                    return ((DmgUpDownAlteration)alt).value;
                return 0;
            }
        }

        /// <summary>
        /// Returns the current Speed Up/Down alteration value. returns 0 of there isn't any.
        /// </summary>
        public int SpeedUpDown
        {
            get
            {
                var alt = Alterations.Find(x => x is SpeedUpDownAlteration);
                if (alt != null && !Alterations.Any(x =>
                        Alteration.overrides[EAlterationType.SpeedUpDown].Contains(x.ToEnum())))
                    return ((SpeedUpDownAlteration)alt).value;
                return 0;
            }
        }

        #endregion

        public int MaxHealth
        {
            get => RefStats.Health;
            set => RefStats.Health = value;
        }

        public Dictionary<EntityStatistics, int> Statistics;

        public int Health => Statistics[EntityStatistics.Health];
        public int Strength => Statistics[EntityStatistics.Strength];
        public int Speed => Snared ? 0 : Statistics[EntityStatistics.Speed] + SpeedUpDown;
        public virtual int Mana => Statistics[EntityStatistics.Mana];
        public int Defense => Shattered ? 0 : Statistics[EntityStatistics.Defense];
        public int Range => Statistics[EntityStatistics.Range];


        public int NumberOfTurnsPlayed = 0;


        public List<EntityAction> EntityActionsBuffer = new List<EntityAction>();

        public bool TryGoTo(Cell destination, int cost)
        {
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
            this.OnHealthRemoved += AreYouAlive;

            this.healthText.text = this.Health.ToString();
        }

        public void UpdateUILife(SpellEventData data)
        {
            int oldLife = int.Parse(this.healthText.text);
            if (oldLife > this.Health)
            {
                this.healthText.DOColor(Color.red, 0.4f).SetEase(Ease.OutQuad);
                this.healthText.transform.DOShakeRotation(0.8f).SetEase(Ease.OutQuad).OnComplete(() =>
                    this.healthText.DOColor(Color.white, 0.2f).SetEase(Ease.OutQuad));
                this.healthText.text = this.Health.ToString();
            }
            else
            {
                this.healthText.DOColor(Color.green, 0.4f).SetEase(Ease.OutQuad);
                this.healthText.transform.DOPunchScale(Vector3.one * 1.3f, 0.8f).SetEase(Ease.OutQuad).OnComplete(() =>
                    this.healthText.DOColor(Color.white, 0.2f).SetEase(Ease.OutQuad));
                this.healthText.text = this.Health.ToString();
            }
        }

        public void UpdateUIShield(SpellEventData data)
        {
        }

        private void LateUpdate()
        {
        }

        #region ATTACKS

        /// <summary>
        /// Tries to attack the given cell.
        /// </summary>
        /// <param name="cellToAttack">The cell to attack.</param>
        public void AutoAttack(Cell cellToAttack)
        {
            if (!isInAttackRange(cellToAttack))
            {
                return;
            }

            //Normally already verified. Just in case
            //Calculate straight path, see if obstacle.
            this.CanAutoAttack = false;
            var path = GridManager.Instance.FindPath(this, cellToAttack.PositionInGrid, true);

            var notwalkable = path.Find(x => x.Datas.state != CellState.Walkable);
            if (notwalkable != null)
            {
                switch (notwalkable.Datas.state)
                {
                    case CellState.Blocked:
                        break;
                    case CellState.EntityIn:
                        //CastAutoAttack(notwalkable);
                        break;
                }
            }
            else
            {
                //There isn't any obstacle in the path, so the attack should go for it.
                //if(cellToAttack.Datas.state == CellState.EntityIn)
                //    CastAutoAttack(cellToAttack);
                //TODO: Shield/overheal? What do i do? Have we got shield in the game??????????????????????
            }
        }

        public bool isInAttackRange(Cell cell)
        {
            bool res = Range >= Mathf.Abs(cell.PositionInGrid.latitude - EntityCell.PositionInGrid.latitude) +
                Mathf.Abs(cell.PositionInGrid.longitude - EntityCell.PositionInGrid.longitude);
            return res;
        }

        #endregion

        #region TURNS

        public virtual void StartTurn()
        {
            this.IsPlayingEntity = true;

            OnTurnBegun?.Invoke(new());

            this.ReinitializeStat(EntityStatistics.Speed);
            this.ReinitializeStat(EntityStatistics.Mana);

            UIManager.Instance.PlayerInfos.UpdateAllTexts();

            if (this.Stunned || this.Sleeping)
            {
                EndTurn();
                return;
            }

            GridManager.Instance.CalculatePossibleCombatMovements(this);
        }

        public virtual void EndTurn()
        {
            NumberOfTurnsPlayed++;
            CanAutoAttack = false;
            foreach (Alteration Alter in Alterations)
            {
                Alter.Apply(this);
            }

            this.IsPlayingEntity = false;
            OnTurnEnded?.Invoke(new());
        }
        #endregion

        #region STATS

        public virtual void Init(EntityStats stats, Cell refCell, WorldGrid refGrid, int order = 0)
        {
            this.transform.position = refCell.WorldPosition;
            this.EntityCell = refCell;
            this.CurrentGrid = refGrid;

            this.RefStats = stats;
            this.Statistics = new Dictionary<EntityStatistics, int>();

            this.Statistics.Add(EntityStatistics.MaxMana,stats.MaxMana);
            this.Statistics.Add(EntityStatistics.Health,stats.Health);
            this.Statistics.Add(EntityStatistics.Strength,stats.Strength);
            this.Statistics.Add(EntityStatistics.Speed,stats.Speed);
            this.Statistics.Add(EntityStatistics.Mana,stats.Mana);
            this.Statistics.Add(EntityStatistics.Defense,stats.Defense);
            this.Statistics.Add(EntityStatistics.Range,stats.Range);
        }

        public void ReinitializeAllStats()
        {
            foreach (EntityStatistics stat in System.Enum.GetValues(typeof(EntityStatistics)))
                this.ReinitializeStat(stat);
        }

        public void ReinitializeStat(EntityStatistics stat) {
            switch (stat) {
                case EntityStatistics.Health: this.Statistics[EntityStatistics.Health] = this.RefStats.Health; break;
                case EntityStatistics.Mana: this.Statistics[EntityStatistics.Mana] = this.RefStats.Mana; break;
                case EntityStatistics.Speed: this.Statistics[EntityStatistics.Speed] = this.RefStats.Speed; break;
                case EntityStatistics.Strength: this.Statistics[EntityStatistics.Strength] = this.RefStats.Strength; break;
                case EntityStatistics.Defense: this.Statistics[EntityStatistics.Defense] = this.RefStats.Defense; break;
                case EntityStatistics.Range: this.Statistics[EntityStatistics.Range] = this.RefStats.Range; break;
            }
        }

        /// <summary>
        /// Applies any value on any stat.
        /// </summary>
        /// <param name="stat">The statistic to modify.</param>
        /// <param name="value">The value to modify the stat for (negative or positive.)</param>
        /// <param name="overShield">Only used to determined if a damage stat should pierce through shieldHP. Will be ignored if stat != health value is positive.</param>
        public void ApplyStat(EntityStatistics stat,int value) 
        {
            Debug.Log($"Applied stat {stat}, {value}, {Environment.StackTrace} ");
            Statistics[stat] += value;

            switch (stat)
            {
                case EntityStatistics.Health:
                    this._applyHealth(value); break;
                case EntityStatistics.Mana: 
                    this._applyMana(value); break;
                case EntityStatistics.Speed:
                    this._applySpeed(value);
                    break;
                case EntityStatistics.Strength:
                    this._applyStrength(value);
                    break;
                case EntityStatistics.Defense:
                    this._applyDefense(value);
                    break;
                case EntityStatistics.Range:
                    this._applyRange(value);
                    break;
            }
        }

        private void _applyHealth(int value)
        {
            if (value > 0)
            {
                // Check overheal
                if (this.Health + value > this.RefStats.Health)
                    value = this.RefStats.Health - Statistics[EntityStatistics.Health];
                //else
                //Statistics[EntityStatistics.Health] += value;
                //value stays at its primary value.
                this.OnHealthAdded?.Invoke(new(this, value));
            }
            else
            {
                value = Mathf.Max(0, Defense - value);
                if (this.Bubbled)
                {
                    value = 0;
                }

                this.OnHealthRemoved?.Invoke(new SpellEventData(this, value));
                if (value != 0) this.OnDamageTaken?.Invoke(new());
            }
        }

        private void _applyMana(int value)
        {
            if (value > 0)
                OnManaAdded?.Invoke(new(this, value));
            else
                OnManaRemoved?.Invoke(new(this, -value));
        }

        private void _applySpeed(int value)
        {
            if (value > 0)
                OnSpeedAdded?.Invoke(new(this, value));
            else
                OnSpeedRemoved?.Invoke(new(this, -value));
        }

        private void _applyStrength(int value)
        {
            if (value > 0)
                OnStrengthAdded?.Invoke(new(this, value));
            else
                OnStrengthRemoved?.Invoke(new(this, -value));
        }

        private void _applyDefense(int value)
        {
            if (value > 0)
                OnDefenseAdded?.Invoke(new(this, value));
            else
                OnDefenseRemoved?.Invoke(new(this, -value));
        }

        private void _applyRange(int value)
        {
            if (value > 0)
                OnRangeAdded?.Invoke(new(this, value));
            else
                OnRangeRemoved?.Invoke(new(this, -value));
        }


        public override string ToString()
        {
            return @$"Name : {name}
            IsAlly : {IsAlly}
            GridPos : {EntityCell}";
        }

        public void AddAlteration(EAlterationType type, int duration, int value)
        {
            OnAlterationReceived?.Invoke(new SpellEventDataAlteration(this, duration, type));
            Debug.Log($"Alteration: {type} to {this.name}");
            Alteration alteration;
            alteration = type switch
            {
                EAlterationType.Stun => new StunAlteration(duration),
                EAlterationType.Snare => new SnareAlteration(duration),
                EAlterationType.Shattered => new ShatteredAlteration(duration),
                EAlterationType.DoT => new DoTAlteration(duration, 2), //Idfk how much dmg
                EAlterationType.Bubbled => new BubbledAlteration(duration),
                EAlterationType.SpeedUpDown => new SpeedUpDownAlteration(duration, value),
                EAlterationType.DmgUpDown => new DmgUpDownAlteration(duration, value),
                EAlterationType.Sleep => new SleepAlteration(duration),
                _ => throw new System.NotImplementedException($"NEED TO IMPLEMENT ENUM MEMBER {type}"),
            };
            var found = Alterations.Find(x => x.GetType() == alteration.GetType());
            if (found != null)
            {
                //TODO : GD? Add Duration? Set duration?
            }
            else
            {
                Alterations.Add(alteration);
            }

            alteration.Setup(this);
            if (alteration.ClassicCountdown)
            {
                this.OnTurnEnded += alteration.DecrementAlterationCountdown;
            }
            else
            {
                switch (alteration)
                {
                    case ShatteredAlteration shat:
                        this.OnTurnEnded += shat.DecrementAlterationCountdown;
                        this.OnHealthRemoved += shat.DecrementAlterationCountdown;
                        break;
                    case BubbledAlteration bubble:
                        this.OnTurnEnded += bubble.DecrementAlterationCountdown;
                        this.OnHealthRemoved += bubble.DecrementAlterationCountdown;
                        break;
                    case SleepAlteration sleep:
                        this.OnHealthRemoved += sleep.DecrementAlterationCountdown;
                        break;
                    default:
                        Debug.LogError("ALTERATION ERROR: SPECIAL COUNTDOWN NOT IMPLEMENTED.");
                        break;
                }
            }
        }

        public void RemoveAlteration(Alteration alteration)
        {
            if (alteration.ClassicCountdown)
            {
                this.OnTurnEnded += alteration.DecrementAlterationCountdown;
            }
            else
            {
                switch (alteration)
                {
                    case ShatteredAlteration shat:
                        this.OnTurnEnded -= shat.DecrementAlterationCountdown;
                        this.OnHealthRemoved -= shat.DecrementAlterationCountdown;
                        break;
                    case BubbledAlteration bubble:
                        this.OnTurnEnded -= bubble.DecrementAlterationCountdown;
                        this.OnHealthRemoved -= bubble.DecrementAlterationCountdown;
                        break;
                    case SleepAlteration sleep:
                        this.OnHealthRemoved -= sleep.DecrementAlterationCountdown;
                        break;
                    default:
                        Debug.LogError("ALTERATION ERROR: SPECIAL COUNTDOWN NOT IMPLEMENTED.");
                        break;
                }
            }
        }

        #endregion

        #region INSTANCE

        public void AreYouAlive(SpellEventData data)
        {
            if (this.Health <= 0) Die();
        }

        public virtual void Die()
        {
            while (Alterations.Count > 0)
            {
                Alteration alt = Alterations[0];
                alt.WearsOff(this);
                RemoveAlteration(alt); //You know what? Fuck you *unsubs your alterations*
                Alterations.RemoveAt(0);
            }

            //???
            OnDeath?.Invoke(new());
            Destroy(this.gameObject);
        }

        #endregion

        #region SKILLS

        public void SubToSpell(SpellResult Action)
        {
        }

        public void UnsubToSpell(SpellResult Action)
        {
        }

        internal void FireOnAlterationGiven(SpellEventData Data)
        {
            OnAlterationGiven?.Invoke(Data);
        }

        public string AlterationStates()
        {
            string res = "";
            if (Alterations.Count > 0)
            {
                res += "Alterations of this Entity:\n";
                foreach (Alteration item in Alterations)
                {
                    res += item.ToString();
                }
            }

            return res;
        }

        #endregion
    }
}