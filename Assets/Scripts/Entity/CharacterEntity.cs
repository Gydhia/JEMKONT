using DownBelow.Spells;
using DownBelow.Spells.Alterations;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.Serialization;
using System;
using DG.Tweening;
using Photon.Realtime;
using static UnityEngine.ParticleSystem;


namespace DownBelow.Entity
{
    public abstract class CharacterEntity : MonoBehaviour
    {
        #region events
        public delegate void StatModified();

        public event SpellEventData.Event OnHealthRemoved;
        public event SpellEventData.Event OnHealthRemovedRealValue;
        public event SpellEventData.Event OnHealthAdded;
        public event SpellEventData.Event OnHealthAddedRealValue;
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

        public event SpellEventData.Event OnStatisticsReinitialized;

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

        public event TeleportationEventData.Event OnTeleportation;

        public event SpellEventData.Event OnPushed;
        #endregion
        #region firingEvents
        public void FireOnTeleportation(TeleportationEventData data)
        {
            OnTeleportation?.Invoke(data);
        }
        public void FireMissingMana()
        {
            OnManaMissing?.Invoke();
        }

        public void FireEntityInited()
        {
            this.OnInited?.Invoke(null);
        }

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
        public EntityStats RefStats;

        [OdinSerialize] public List<Alteration> Alterations = new();

        public int TurnOrder;
        public bool IsAlly = true;

        public bool IsPlayingEntity = false;

        // Used for NPC. Determined UID to parse over network. 
        // TODO: Change it to a real Guid later
        public string UID = string.Empty;

        public string EntityName = "Entity";

        public WorldGrid CurrentGrid;

        // Movements
        public bool IsMoving = false;
        public bool CanMove = true;


        private Cell _entityCell;
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

        public Transform EntityHolder;
        public SkinnedMeshRenderer Renderer;
        public Animator Animator;

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
        protected int Buff(EntityStatistics stat)
        {
            int res = 0;
            var alt = Alterations.Find(x => x is BuffAlteration buffAlt && buffAlt.StatToBuff == stat);
            if (alt != null && alt is BuffAlteration buff)
            {
                res = buff.value;
            }
            return res;
        }

        Alteration GetAlteration<T>() where T : Alteration
        {
            return Alterations.Find(x => x.GetType() == typeof(T));
        }
        #endregion

        public int MaxHealth
        {
            get => RefStats.Health;
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
            if (this.SelectedIndicator != null)
            {
                this.SelectedIndicator.transform.DOScale(0.05f, 1.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
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

        public bool isInAttackRange(Cell cell)
        {
            bool res = Range >= Mathf.Abs(cell.PositionInGrid.latitude - EntityCell.PositionInGrid.latitude) +
                Mathf.Abs(cell.PositionInGrid.longitude - EntityCell.PositionInGrid.longitude);
            return res;
        }

        #endregion

        #region TURNS

        public virtual async void StartTurn()
        {
            this.IsPlayingEntity = true;
            this.PlayingIndicator.SetActive(true);
            this.CanAutoAttack = true;

            Debug.LogWarning("START TURN : " + this);
            OnTurnBegun?.Invoke(new());

            this.ReinitializeStat(EntityStatistics.Speed);
            this.ReinitializeStat(EntityStatistics.Mana);

            UIManager.Instance.PlayerInfos.UpdateAllTexts();

            await SFXManager.Instance.RefreshAlterationSFX(this);

            if (this.IsAlly && this is PlayerBehavior player)
            {
                if (GameManager.SelfPlayer == player)
                {
                    AkSoundEngine.PostEvent("Play_SSFX_MyTurnStart", AudioHolder.Instance.gameObject);

                }
                AkSoundEngine.PostEvent("Play_SSFX_AllyTurn", AudioHolder.Instance.gameObject);
            }
            else
            {
                AkSoundEngine.PostEvent("Play_SSFX_EnemyTurn", AudioHolder.Instance.gameObject);
            }

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
            this.CanAutoAttack = false;
            foreach (Alteration Alter in Alterations)
            {
                Alter.Apply(this);
            }

            this.PlayingIndicator.SetActive(false);
            this.IsPlayingEntity = false;
            if (this.IsAlly)
            {
                AkSoundEngine.PostEvent("Play_SSFX_MyTurnEnd", AudioHolder.Instance.gameObject);
            }
            OnTurnEnded?.Invoke(new());
        }
        #endregion

        #region STATS

        public virtual void Init(Cell refCell, WorldGrid refGrid, int order = 0)
        {
            this.transform.position = refCell.WorldPosition;
            this.EntityCell = refCell;
            this.CurrentGrid = refGrid;
        }

        public virtual void SetStatistics(EntityStats stats, bool notify = true)
        {
            this.RefStats = stats;
            this.Statistics = new Dictionary<EntityStatistics, int>
            {
                { EntityStatistics.Health, stats.Health },
                { EntityStatistics.Strength, stats.Strength },
                { EntityStatistics.Speed, stats.Speed },
                { EntityStatistics.Mana, stats.Mana },
                { EntityStatistics.Defense, stats.Defense },
                { EntityStatistics.Range, stats.Range }
            };

            if (notify)
            {
                this.OnStatisticsChanged?.Invoke(null);
            }
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

            this.OnStatisticsReinitialized?.Invoke(new SpellEventData(this, this.RefStats.GetStatistic(stat), stat));
        }

        /// <summary>
        /// Applies any value on any stat.
        /// </summary>
        /// <param name="stat">The statistic to modify.</param>
        /// <param name="value">The value to modify the stat for (negative or positive.)</param>
        ///<param name="triggerEvents">true by default. Used to </param>
        public void ApplyStat(EntityStatistics stat, int value, bool triggerEvents = true)
        {
            Debug.Log($"Applied stat {stat}, {value} to {ToString()} {Environment.StackTrace} ");

            int maxStat = this.RefStats.GetStatistic(stat);
            Statistics[stat] += value;

            if (Statistics[stat] > maxStat)
            {
                this.Statistics[stat] = maxStat;

                if (value > 0)
                {
                    // Check overheal
                    if (this.Statistics[stat] + value > maxStat)
                    {
                        value = maxStat - this.Statistics[stat];
                    }
                }
            }


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
                Debug.Log("HEALED : "+ value);

                if (triggerEvents)
                {
                    this.OnHealthAdded?.Invoke(new(this, value));
                    this.OnHealthAddedRealValue?.Invoke(new(this, value));
                }

                this.Animator.SetTrigger("OnHit");
            }
            else
            {
                this.OnHealthRemovedRealValue?.Invoke(new SpellEventData(this, value));
                value = Mathf.Max(0, Defense - value);
                if (this.Bubbled)
                {
                    value = 0;
                }
                if (triggerEvents)
                {
                    
                    this.OnHealthRemoved?.Invoke(new SpellEventData(this, value));
                    if (value != 0)
                    {
                        this.OnDamageTaken?.Invoke(new());
                    }
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
            string res = @$"Name : {name}
            IsAlly : {IsAlly}
            GridPos : {EntityCell}\n";
            if (Alterations != null && Alterations.Count > 0)
            {
                res += "Alterations :";
                foreach (var item in Alterations)
                {
                    res += $"\n{item}";
                }
            }

            return res;
        }
        public void AddAlterations(List<Alteration> alterations)
        {
            alterations.ForEach(x => AddAlteration(x));
        }
        public void AddAlteration(Alteration alteration)
        {
            OnAlterationReceived?.Invoke(new SpellEventDataAlteration(this, alteration));
            Debug.Log($"Alteration: {alteration} to {this.name}");
            Alteration alreadyFound = null;
            if (alteration is not BuffAlteration)
            {
                alreadyFound = Alterations.Find(x => x.GetType() == alteration.GetType());
            }
            if (alreadyFound != null)
            {
                alreadyFound.Duration = alteration.Duration;
                return;
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
            if (this.Health <= 0)
            {
                this.OnDeath?.Invoke(new EntityEventData(this));

                if (this is PlayerBehavior && this.IsPlayingEntity)
                {
                    var endTurn = new EndTurnAction(this, this.EntityCell);

                    NetworkManager.Instance.EntityAskToBuffAction(endTurn);
                }
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

            var particle = Instantiate(SettingsManager.Instance.GridsPreset.PlayerSwitchPrefab, this.transform.position + (Vector3.up * 0.75f), Quaternion.identity);
            Destroy(particle.gameObject, 6f);

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
        /// <returns>The modified cell to tp on.</returns>
        public Cell SmartTeleport(Cell TargetCell, SpellResult Result = null)
        {
            var cellToTP = TargetCell;

            //Could be changed to a "while(cellToTP != walkable)"?
            if (cellToTP.Datas.state != CellState.Walkable)
            {
                List<Cell> freeNeighbours = GridManager.Instance.GetNormalNeighbours(cellToTP, cellToTP.RefGrid)
                    .FindAll(x => x.Datas.state == CellState.Walkable)
                    .OrderByDescending(x => Math.Abs(x.PositionInGrid.latitude - this.EntityCell.PositionInGrid.latitude) + Math.Abs(x.PositionInGrid.longitude - this.EntityCell.PositionInGrid.longitude))
                    .ToList();
                //Someday will need a Foreach, but i just don't know what other things we need to check on the cells before tp'ing,
                //so just tp on the farther one.
                cellToTP = freeNeighbours[0];
            }

            if (cellToTP.Datas.state == CellState.Walkable)
            {
                transform.position = cellToTP.gameObject.transform.position;

                FireExitedCell();

                EntityCell = cellToTP;

                FireEnteredCell(cellToTP);
                if (Result != null && Result.Caster != null && Result.Caster == this)
                {
                    GridManager.Instance.CalculatePossibleCombatMovements(this);
                }

            }
            return cellToTP;
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

            if (cellToTP.Datas.state == CellState.Walkable)
            {
                transform.position = cellToTP.gameObject.transform.position;

                FireExitedCell();

                EntityCell = cellToTP;

                FireEnteredCell(cellToTP);
                if (Result != null && Result.Caster != null && Result.Caster == this)
                {
                    GridManager.Instance.CalculatePossibleCombatMovements(this);
                }

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