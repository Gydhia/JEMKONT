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
using System.CodeDom;

namespace DownBelow.Entity
{
    public abstract class CharacterEntity : MonoBehaviour
    {
        #region events
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

        public event GameEventData.Event OnStatisticsChanged;

        public Action OnManaMissing;

        /// <summary>
        /// When you give an alteration to someone else.
        /// </summary>
        public event SpellEventDataAlteration.Event OnAlterationGiven;

        /// <summary>
        /// When you receive an alteration from someone else.
        /// </summary>
        public event SpellEventDataAlteration.Event OnAlterationReceived;

        public event EntityEventData.Event OnEntityTargetted;

        public event GameEventData.Event OnTurnBegun;
        public event GameEventData.Event OnTurnEnded;
        public event GameEventData.Event OnTryTakeDamage;
        public event GameEventData.Event OnDamageTaken;
        public event EntityEventData.Event OnDeath;

        public event GameEventData.Event OnInited;

        public event CellEventData.Event OnEnteredCell;
        public event CellEventData.Event OnExitedCell;

        public void FireMissingMana()
        {
            OnManaMissing?.Invoke();
        }

        public void FireEntityInited()
        {
            this.OnInited?.Invoke(null);
        }

        public event SpellEventData.Event OnPushed;
        #endregion
        #region firingEvents
        public void FireExitedCell()
        {
            this.EntityCell.Datas.state = CellState.Walkable;
            this.EntityCell.EntityIn = null;

            this.OnExitedCell?.Invoke(new CellEventData(this.EntityCell));
        }

        public virtual void FireEnteredCell(Cell cell)
        {
            this.EntityCell = cell;

            this.OnEnteredCell?.Invoke(new CellEventData(cell));
        }

        public void FireEntityTargetted(CharacterEntity targeter)
        {
            this.OnEntityTargetted?.Invoke(new(targeter));
        }

        public void FireEntityPushed(SpellEventData data)
        {
            this.OnPushed?.Invoke(data);
        }
        #endregion
        protected EntityStats RefStats;

        [OdinSerialize] public List<Alteration> Alterations = new();

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

        public Sprite EntitySprite;
        public Cell EntityCell
        {
            get { return this._entityCell; }
            set
            {
                if (_entityCell != null)
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

        public GameObject PlayingIndicator;
        public GameObject SelectedIndicator;

        #region alterationBooleans

        public bool Snared
        {
            get => Alterations.Any(x => x.GetType() == typeof(SnareAlteration));
        } //DONE

        public bool Stunned
        {
            get => Alterations.Any(x => x.GetType() == typeof(StunAlteration));
        } //DONE

        public bool Shattered
        {
            get => Alterations.Any(x => x.GetType() == typeof(ShatteredAlteration));
        } //DONE

        public bool DoT
        {
            get => Alterations.Any(x => x.GetType() == typeof(DoTAlteration));
        } //DONE

        public bool Bubbled
        {
            get => Alterations.Any(x => x.GetType() == typeof(BubbledAlteration));
        } //DONE

        public bool Sleeping
        {
            get => Alterations.Any(x => x.GetType() == typeof(SleepAlteration));
        } //DONE

        /// <summary>
        /// Returns the current additionnal/reductionnal value we have on the given stat.
        /// </summary>
        /// <param name="stat">The given stat.</param>
        /// <returns></returns>
        int Buff(EntityStatistics stat)
        {
            int res = 0;
            var alt = Alterations.Find(x => x is BuffAlteration buffAlt && buffAlt.StatToBuff == stat);
            if (alt != null && alt is BuffAlteration buff)
            {
                res = buff.value;
            }
            return res;
        }
        #endregion

        public int MaxHealth
        {
            get => RefStats.Health;
            set => RefStats.Health = value;
        }

        public Dictionary<EntityStatistics, int> Statistics;

        public int Health => Statistics[EntityStatistics.Health] + Buff(EntityStatistics.Health);
        public int Strength => Statistics[EntityStatistics.Strength] + Buff(EntityStatistics.Strength);
        public int Speed => Snared ? 0 : Statistics[EntityStatistics.Speed] + Buff(EntityStatistics.Speed);
        public virtual int Mana => Statistics[EntityStatistics.Mana] + Buff(EntityStatistics.Mana);
        public int Defense => Shattered ? 0 : Statistics[EntityStatistics.Defense] + Buff(EntityStatistics.Defense);
        public int Range => Statistics[EntityStatistics.Range] + Buff(EntityStatistics.Range);


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
            // TODO : Move it from here later, and same for other indicators, but not a priority
            this.PlayingIndicator.transform.DOMoveY(this.PlayingIndicator.transform.position.y - 0.5f, 1.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            if(this.SelectedIndicator != null)
            {
                this.SelectedIndicator.transform.DOScale(0.13f, 1.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            }
            this.OnHealthRemoved += AreYouAlive;
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
            } else
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
            this.PlayingIndicator.SetActive(true);

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

            this.PlayingIndicator.SetActive(false);
            this.IsPlayingEntity = false;
            OnTurnEnded?.Invoke(new());
        }
        #endregion

        #region STATS

        public virtual void Init(Cell refCell, WorldGrid refGrid, int order = 0, bool isFake = false)
        {
            this.transform.position = refCell.WorldPosition;
            this.EntityCell = refCell;
            this.CurrentGrid = refGrid;
        }

        public virtual void SetStatistics(EntityStats stats)
        {
            this.RefStats = stats;
            this.Statistics = new Dictionary<EntityStatistics, int>
            {
                { EntityStatistics.MaxMana, stats.MaxMana },
                { EntityStatistics.Health, stats.Health },
                { EntityStatistics.Strength, stats.Strength },
                { EntityStatistics.Speed, stats.Speed },
                { EntityStatistics.Mana, stats.Mana },
                { EntityStatistics.Defense, stats.Defense },
                { EntityStatistics.Range, stats.Range }
            };

            this.OnStatisticsChanged?.Invoke(null);
        }

        public void ReinitializeAllStats()
        {
            foreach (EntityStatistics stat in System.Enum.GetValues(typeof(EntityStatistics)))
                this.ReinitializeStat(stat);
        }

        public void ReinitializeStat(EntityStatistics stat)
        {
            switch (stat)
            {
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
        ///<param name="triggerEvents">true by default. Used to </param>
        public void ApplyStat(EntityStatistics stat, int value, bool triggerEvents = true)
        {
            Debug.Log($"Applied stat {stat}, {value}, {Environment.StackTrace} ");
            Statistics[stat] += value;

            switch (stat)
            {
                case EntityStatistics.Health:
                    this._applyHealth(value, triggerEvents); break;
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

        private void _applyHealth(int value, bool triggerEvents = true)
        {
            if (value > 0)
            {
                // Check overheal
                if (this.Health + value > this.RefStats.Health)
                    value = this.RefStats.Health - Statistics[EntityStatistics.Health];
                //else
                //Statistics[EntityStatistics.Health] += value;
                //value stays at its primary value.
                if (triggerEvents)
                {
                    this.OnHealthAdded?.Invoke(new(this, value));
                }
            } else
            {
                value = Mathf.Max(0, Defense - value);
                if (this.Bubbled)
                {
                    value = 0;
                }
                if (triggerEvents)
                {
                    this.OnHealthRemoved?.Invoke(new SpellEventData(this, value));
                    if (value != 0) this.OnDamageTaken?.Invoke(new());
                }
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
        public void AddAlterations(List<Alteration> alterations)
        {
            alterations.ForEach(x => AddAlteration(x));
        }
        public void AddAlteration(Alteration alteration)
        {
            OnAlterationReceived?.Invoke(new SpellEventDataAlteration(this, alteration));
            Debug.Log($"Alteration: {alteration} to {this.name}");
            var alreadyFound = Alterations.Find(x => x.GetType() == alteration.GetType());
            if (alreadyFound != null)
            {
                //TODO : GD? Add Duration? Set duration?
            } else
            {
                Alterations.Add(alteration);
            }
            alteration.Setup(this);

            if (alteration.ClassicCountdown)
            {
                this.OnTurnEnded += alteration.DecrementAlterationCountdown;
            } else
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
            } else
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
            if (this.Health <= 0)
            {
                this.OnDeath?.Invoke(new EntityEventData(this));
            }
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

            this.FireExitedCell();
            StartCoroutine(_deathTime());
        }

        // TODO : temporary to wait for the player to die
        private IEnumerator _deathTime(float delay = 2f)
        {
            yield return new WaitForSeconds(delay);

            this.gameObject.SetActive(false);
        }

        #endregion

        #region SKILLS

        public void SubToSpell(SpellResult Action)
        {
        }

        public void UnsubToSpell(SpellResult Action)
        {
        }

        internal void FireOnAlterationGiven(SpellEventDataAlteration Data)
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

        /// <summary>
        /// Tries to teleport the character entity to the target cell. If it's occupied, will try to teleport on neighbours.
        /// if you don't want that behavior, try <c>Teleport()</c>.
        /// </summary>
        /// <param name="TargetCell"> the targeted cell.</param>
        /// <param name="Result">If the teleportation is due to a spell, the spell result of the spell.</param>
        public void SmartTeleport(Cell TargetCell, SpellResult Result = null)
        {
            var cellToTP = TargetCell;

            //Could be changed to a "while(cellToTP != walkable)"?
            if (cellToTP.Datas.state != CellState.Walkable)
            {
                List<Cell> freeNeighbours = GridManager.Instance.GetNormalNeighbours(cellToTP, cellToTP.RefGrid)
                    .FindAll(x => x.Datas.state == CellState.Walkable)
                    .OrderByDescending(x => Math.Abs(x.PositionInGrid.latitude - this.EntityCell.PositionInGrid.latitude) + Math.Abs(x.PositionInGrid.longitude - this.EntityCell.PositionInGrid.longitude))
                    .ToList();
                //Someday will need a Foreach, but i just don't know what we need to check on the cells before tp'ing, so just tp on the farther one.
                cellToTP = freeNeighbours[0];
            }

            //If the teleportation is due to a spell, add it in the result.
            if (cellToTP.EntityIn != null && Result != null)
            {
                Result.TeleportedTo.Add(cellToTP.EntityIn);
            }

            if (cellToTP.Datas.state != CellState.Walkable)
            {
                transform.position = cellToTP.gameObject.transform.position;

                FireExitedCell();

                EntityCell = cellToTP;

                FireEnteredCell(cellToTP);
            }
        }

        /// <summary>
        /// Tries to teleport the character entity to the target cell. If it's occupied, does not.
        /// If you want a spell that can teleport closely to an occupied cell, try <c>SmartTeleport()</c>.
        /// </summary>
        /// <param name="TargetCell"> the targeted cell.</param>
        /// <param name="Result">If the teleportation is due to a spell, the spell result of the spell.</param>
        public void Teleport(Cell TargetCell, SpellResult Result = null)
        {
            var cellToTP = TargetCell;

            //If the teleportation is due to a spell, add it in the result.
            if (cellToTP.EntityIn != null && Result != null)
            {
                Result.TeleportedTo.Add(cellToTP.EntityIn);
            }

            if (cellToTP.Datas.state != CellState.Walkable)
            {
                transform.position = cellToTP.gameObject.transform.position;

                FireExitedCell();

                EntityCell = cellToTP;

                FireEnteredCell(cellToTP);
            }
        }

        private void OnDestroy()
        {
            if (this.EntityCell != null)
            {
                this.FireExitedCell();
            }
            if (this.CurrentGrid != null)
            {
                this.CurrentGrid.GridEntities.Remove(this);
            }
        }
    }
}