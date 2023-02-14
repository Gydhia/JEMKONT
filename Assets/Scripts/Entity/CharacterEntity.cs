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

namespace DownBelow.Entity
{
    public abstract class CharacterEntity : MonoBehaviour
    {
        public delegate void StatModified();

        public event SpellEventData.Event OnHealthRemoved;
        public event SpellEventData.Event OnHealthAdded;
        public event SpellEventData.Event OnShieldRemoved;
        public event SpellEventData.Event OnShieldAdded;
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


        public event GameEventData.Event OnTurnBegun;
        public event GameEventData.Event OnTurnEnded;
        public event GameEventData.Event OnTryTakeDamage;
        public event GameEventData.Event OnDamageTaken;
        public event GameEventData.Event OnDeath;


        protected EntityStats RefStats;

        [OdinSerialize] public List<Alteration> Alterations = new();

        public UnityEngine.UI.Slider HealthFill;
        public UnityEngine.UI.Slider ShieldFill;

        public int TurnOrder;
        public bool IsAlly = true;
        public bool IsPlayingEntity = false;
        // Used for NPC. Determined UID to parse over network. 
        // TODO: Change it to a real Guid later
        public string UID = string.Empty;

        public WorldGrid CurrentGrid;

        // Movements
        public bool IsMoving = false;
        public Cell EntityCell = null;
        public Cell NextCell = null;
        protected Coroutine moveCor = null;
        public List<Cell> CurrentPath;

        public List<CharacterEntity> Summons;

        public bool CanAutoAttack;


        #region alterationBooleans
        public bool Snared { get => Alterations.Any(x => x.GetType() == typeof(SnareAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Snare].Contains(x.ToEnum())); }//DONE
        public bool Stunned { get => Alterations.Any(x => x.GetType() == typeof(StunAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Stun].Contains(x.ToEnum())); }//DONE
        public bool Disarmed { get => Alterations.Any(x => x.GetType() == typeof(DisarmedAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Disarmed].Contains(x.ToEnum())); }//DONE
        public bool Critical { get => Alterations.Any(x => x.GetType() == typeof(CriticalAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Critical].Contains(x.ToEnum())); }//Ouille. On met �a de c�t� le temps d'un gros refactor. impossible.
        public bool Dodge { get => Alterations.Any(x => x.GetType() == typeof(DodgeAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Dodge].Contains(x.ToEnum())); }//DONE
        public bool Camouflage { get => Alterations.Any(x => x.GetType() == typeof(CamouflageAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Camouflage].Contains(x.ToEnum())); }//DONE
        public bool Provoke { get => Alterations.Any(x => x.GetType() == typeof(ProvokeAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Provoke].Contains(x.ToEnum())); }//DONE
        public bool Ephemeral { get => Alterations.Any(x => x.GetType() == typeof(EphemeralAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Ephemeral].Contains(x.ToEnum())); }//DONE
        public bool Confused { get => Alterations.Any(x => x.GetType() == typeof(ConfusionAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Confusion].Contains(x.ToEnum())); }//DONE
        public bool Shattered { get => Alterations.Any(x => x.GetType() == typeof(ShatteredAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Shattered].Contains(x.ToEnum())); }//DONE
        public bool DoT { get => Alterations.Any(x => x.GetType() == typeof(DoTAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.DoT].Contains(x.ToEnum())); }//DONE
        public bool Inspired { get => Alterations.Any(x => x.GetType() == typeof(InspirationAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Inspiration].Contains(x.ToEnum())); }//DONE 
        public bool Bubbled { get => Alterations.Any(x => x.GetType() == typeof(BubbledAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Bubbled].Contains(x.ToEnum())); }//DONE
        public bool MindControl { get => Alterations.Any(x => x.GetType() == typeof(MindControlAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.MindControl].Contains(x.ToEnum())); }//FUCK YOU
        public bool Sleeping { get => Alterations.Any(x => x.GetType() == typeof(SleepAlteration)) && !Alterations.Any(x => Alteration.overrides[EAlterationType.Sleep].Contains(x.ToEnum())); }//DONE
        /// <summary>
        /// Returns the current Damage Up/Down alteration value. returns 0 of there isn't any.
        /// </summary>
        public int DmgUpDown {
            get {
                var alt = Alterations.Find(x => x is DmgUpDownAlteration);
                if (alt != null && !Alterations.Any(x => Alteration.overrides[EAlterationType.DmgUpDown].Contains(x.ToEnum()))) return ((DmgUpDownAlteration)alt).value;
                return 0;
            }
        }
        /// <summary>
        /// Returns the current Speed Up/Down alteration value. returns 0 of there isn't any.
        /// </summary>
        public int SpeedUpDown {
            get {
                var alt = Alterations.Find(x => x is SpeedUpDownAlteration);
                if (alt != null && !Alterations.Any(x => Alteration.overrides[EAlterationType.SpeedUpDown].Contains(x.ToEnum()))) return ((SpeedUpDownAlteration)alt).value;
                return 0;
            }
        }
        #endregion

        public int MaxHealth { get => RefStats.Health; set => RefStats.Health = value; }
        public Dictionary<EntityStatistics,int> Statistics;
        public int Health { get => Statistics[EntityStatistics.Health]; }
        public int Shield { get => Statistics[EntityStatistics.Shield]; }
        public int Strength { get => Statistics[EntityStatistics.Strength]; }
        public int Speed { get => Snared ? 0 : Statistics[EntityStatistics.Speed] + SpeedUpDown; }
        public virtual int Mana { get => Statistics[EntityStatistics.Mana]; }
        public int Defense { get => Shattered ? 0 : Statistics[EntityStatistics.Defense]; }
        public int Range { get => Statistics[EntityStatistics.Range]; }
        public int NumberOfTurnsPlayed = 0;
        /// <summary>
        /// </summary>
        /// <returns>the auto attack spell of this entity. Can be any Spell.</returns>
        public abstract Spell AutoAttackSpell();

        public bool TryGoTo(Cell destination,int cost) 
        {

            this.EntityCell.EntityIn = null;

            this.EntityCell = destination;
            this.transform.position = destination.WorldPosition;

            destination.EntityIn = this;
            return true;
        }

        public void Start() {
            this.OnHealthAdded += UpdateUILife;
            this.OnHealthRemoved += UpdateUILife;
            this.OnHealthRemoved += AreYouAlive;

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

        #region MOVEMENTS

        public virtual void MoveWithPath(List<Cell> newPath,string otherGrid) {
            // Useless to animate hidden players
            if (!this.gameObject.activeSelf) {
                // /!\ TEMPORY ONLY, SET THE CELL AS THE LAST ONE OF PATH
                // We should have events instead for later on
                this.EntityCell = newPath[^1];
                return;
            }

            if (this.moveCor == null) {
                this.CurrentPath = newPath;
                // That's ugly, find a clean way to build the path instead
                if (!this.CurrentPath.Contains(this.EntityCell))
                    this.CurrentPath.Insert(0,this.EntityCell);
                this.moveCor = StartCoroutine(FollowPath());
            }
        }

        public virtual IEnumerator FollowPath() {
            this.IsMoving = true;
            int currentCell = 0, targetCell = 1;

            float timer;
            while (currentCell < this.CurrentPath.Count - 1) {
                timer = 0f;
                while (timer <= 0.2f) {
                    this.transform.position = Vector3.Lerp(CurrentPath[currentCell].gameObject.transform.position,CurrentPath[targetCell].gameObject.transform.position,timer / 0.2f);
                    timer += Time.deltaTime;
                    yield return null;
                }

                this.EntityCell.Datas.state = CellState.Walkable;
                this.EntityCell.EntityIn = null;

                this.EntityCell = CurrentPath[targetCell];

                this.EntityCell.Datas.state = CellState.EntityIn;
                this.EntityCell.EntityIn = this;

                currentCell++;
                targetCell++;

                if (targetCell <= this.CurrentPath.Count - 1)
                    this.NextCell = CurrentPath[targetCell];
            }

            this.moveCor = null;
            this.IsMoving = false;
        }

        #endregion

        #region ATTACKS
        /// <summary>
        /// Tries to attack the given cell.
        /// </summary>
        /// <param name="cellToAttack">The cell to attack.</param>
        public void AutoAttack(Cell cellToAttack) {
            if (!isInAttackRange(cellToAttack)) {
                return;
            }
            //Normally already verified. Just in case
            //Calculate straight path, see if obstacle.
            this.CanAutoAttack = false;
            var path = GridManager.Instance.FindPath(this,cellToAttack.PositionInGrid, true);

            var notwalkable = path.Find(x => x.Datas.state != CellState.Walkable);
            if (notwalkable != null) {
                switch (notwalkable.Datas.state) {
                    case CellState.Blocked:
                        break;
                    case CellState.Shared:
                        break;
                    case CellState.EntityIn:
                        CastAutoAttack(notwalkable);
                        break;
                }
            } else {
                //There isn't any obstacle in the path, so the attack should go for it.
                if(cellToAttack.Datas.state == CellState.EntityIn)
                    CastAutoAttack(cellToAttack);
                //TODO: Shield/overheal? What do i do? Have we got shield in the game??????????????????????
            }
        }
        protected void CastAutoAttack(Cell cell) 
        {
            AutoAttackSpell().ExecuteSpell(this,cell);
        }
        public bool isInAttackRange(Cell cell) 
        {
            bool res = Range >= Mathf.Abs(cell.PositionInGrid.latitude - EntityCell.PositionInGrid.latitude) + Mathf.Abs(cell.PositionInGrid.longitude - EntityCell.PositionInGrid.longitude);
            return res;
        }
        #endregion

        #region TURNS
        public virtual void EndTurn() 
        {
            NumberOfTurnsPlayed++;
            CanAutoAttack = false;
            foreach (Alteration Alter in Alterations) {
                Alter.Apply(this);
            }
            this.IsPlayingEntity = false;
            OnTurnEnded?.Invoke(new());
        }
        public virtual void StartTurn() 
        {
            this.IsPlayingEntity = true;

            OnTurnBegun?.Invoke(new());
            CanAutoAttack = !Disarmed;//CanAutoAttack = true if !Disarmed

            this.ReinitializeStat(EntityStatistics.Speed);
            this.ReinitializeStat(EntityStatistics.Mana);

            UIManager.Instance.PlayerInfos.UpdateAllTexts();

            if (this.Stunned || this.Sleeping) 
                EndTurn();

            GridManager.Instance.ShowPossibleCombatMovements(this);
        }
        #endregion

        #region STATS
        public virtual void Init(EntityStats stats,Cell refCell,WorldGrid refGrid,int order = 0) {
            this.transform.position = refCell.WorldPosition;
            this.EntityCell = refCell;
            this.CurrentGrid = refGrid;

            this.RefStats = stats;
            this.Statistics = new Dictionary<EntityStatistics,int>();

            this.Statistics.Add(EntityStatistics.MaxMana,stats.MaxMana);
            this.Statistics.Add(EntityStatistics.Health,stats.Health);
            this.Statistics.Add(EntityStatistics.Shield,stats.BaseShield);
            this.Statistics.Add(EntityStatistics.Strength,stats.Strength);
            this.Statistics.Add(EntityStatistics.Speed,stats.Speed);
            this.Statistics.Add(EntityStatistics.Mana,stats.Mana);
            this.Statistics.Add(EntityStatistics.Defense,stats.Defense);
            this.Statistics.Add(EntityStatistics.Range,stats.Range);
        }

        public void ReinitializeAllStats() {
            foreach (EntityStatistics stat in System.Enum.GetValues(typeof(EntityStatistics)))
                this.ReinitializeStat(stat);
        }

        public void ReinitializeStat(EntityStatistics stat) {
            switch (stat) {
                case EntityStatistics.Health: this.Statistics[EntityStatistics.Health] = this.RefStats.Health; break;
                case EntityStatistics.Shield: this.Statistics[EntityStatistics.Shield] = this.RefStats.BaseShield; break;
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
        public void ApplyStat(EntityStatistics stat,int value,bool overShield = false) 
        {
            Debug.Log($"Applied stat {stat}, {value}, {Environment.StackTrace} ");
            Statistics[stat] += value;

            switch (stat) 
            {
                case EntityStatistics.Health:
                    this._applyHealth(value, overShield); break;
                case EntityStatistics.Shield: 
                    this._applyShield(value); break;
                case EntityStatistics.Mana: 
                    this._applyMana(value); break;
                case EntityStatistics.Speed:
                    this._applySpeed(value); break;
                case EntityStatistics.Strength:
                    this._applyStrength(value); break;
                case EntityStatistics.Defense:
                    this._applyDefense(value); break;
                case EntityStatistics.Range:
                    this._applyRange(value); break;
            }
        }

        private void _applyHealth(int value, bool overShield)
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
                if (this.Dodge)
                {
                    if (UnityEngine.Random.Range(0, 1) == 0) value = 0;
                }
                int onShield = this.Shield - value > 0 ? value : this.Shield;
                int onLife = overShield ? value : -(onShield - value);
                value = -onLife;
                if (!overShield)
                {
                    this.Statistics[EntityStatistics.Shield] -= onShield;//Only exception where we
                    this.OnShieldRemoved?.Invoke(new SpellEventData(this, onShield));
                }

                this.OnHealthRemoved?.Invoke(new SpellEventData(this, onLife));
                if (value != 0) this.OnDamageTaken?.Invoke(new());
            }
        }

        private void _applyShield(int value)
        {
            if (value > 0)
                OnShieldAdded?.Invoke(new(this, value));
            else 
                OnShieldRemoved?.Invoke(new(this, -value));
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


        public override string ToString() {
            return @$"Name : {name}
IsAlly : {IsAlly}
GridPos : {EntityCell}";
        }
        public void AddAlteration(EAlterationType type,int duration,int value) {
            OnAlterationReceived?.Invoke(new SpellEventDataAlteration(this,duration,type));
            Debug.Log($"Alteration: {type} to {this.name}");
            Alteration alteration;
            alteration = type switch
            {
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
                EAlterationType.DoT => new DoTAlteration(duration, 2),//Idfk how much dmg
                EAlterationType.Bubbled => new BubbledAlteration(duration),
                EAlterationType.MindControl => new MindControlAlteration(duration),
                EAlterationType.SpeedUpDown => new SpeedUpDownAlteration(duration, value),
                EAlterationType.DmgUpDown => new DmgUpDownAlteration(duration, value),
                EAlterationType.Inspiration => new InspirationAlteration(duration),
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
                    case CriticalAlteration crit:
                        //Don't worry guys
                        //Edit: WHY???? WHERE DID I- WHAT???? HELP
                        //Re-Edit : yeah check SubToSpell
                        break;
                    case DodgeAlteration dodge:
                        this.OnHealthRemoved += alteration.DecrementAlterationCountdown;
                        break;
                    case CamouflageAlteration camo:
                        this.OnTurnEnded += camo.DecrementAlterationCountdown;
                        this.OnHealthRemoved += camo.DecrementAlterationCountdown;
                        break;
                    case ProvokeAlteration prov:
                        this.OnTurnEnded += prov.DecrementAlterationCountdown;
                        this.OnHealthRemoved += prov.DecrementAlterationCountdown;
                        //OnDamageReceived too? tf
                        break;
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
                    case CriticalAlteration crit:
                        //Don't worry guys
                        //Edit: WHY???? WHERE DID I- WHAT???? HELP
                        //Re-Edit : yeah check SubToSpell
                        break;
                    case DodgeAlteration dodge:
                        this.OnHealthRemoved -= alteration.DecrementAlterationCountdown;
                        break;
                    case CamouflageAlteration camo:
                        this.OnTurnEnded -= camo.DecrementAlterationCountdown;
                        this.OnHealthRemoved -= camo.DecrementAlterationCountdown;
                        break;
                    case ProvokeAlteration prov:
                        this.OnTurnEnded -= prov.DecrementAlterationCountdown;
                        //OnDamageReceived too? tf
                        this.OnHealthRemoved -= prov.DecrementAlterationCountdown;
                        break;
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
        public void AreYouAlive(SpellEventData data) {
            if (this.Health <= 0) Die();
        }
        public virtual void Die() {
            while (Alterations.Count > 0) {
                Alteration alt = Alterations[0];
                alt.WearsOff(this);
                RemoveAlteration(alt);//You know what? Fuck you *unsubs your alterations*
                Alterations.RemoveAt(0);
            }
            //???
            OnDeath?.Invoke(new());
            Destroy(this.gameObject);
        }
        #endregion

        #region SKILLS
        public void SubToSpell(SpellAction Action) {
            //oh
            foreach (var item in Alterations) {
                switch (item) {
                    case CriticalAlteration crit:
                        Action.OnDamageDealt += crit.DecrementAlterationCountdown;
                        break;
                    default:
                        break;
                }
            }
        }
        public void UnsubToSpell(SpellAction Action) {
            foreach (var item in Alterations) {
                switch (item) {
                    case CriticalAlteration crit:
                        Action.OnDamageDealt -= crit.DecrementAlterationCountdown;
                        break;
                    default:
                        break;
                }
            }
        }

        internal void FireOnAlterationGiven(SpellEventData Data) {
            OnAlterationGiven?.Invoke(Data);
        }
        public string AlterationStates() {
            string res = "";
            if(Alterations.Count > 0) {
                res += "Alterations of this Entity:\n";
                foreach (Alteration item in Alterations) {
                    res += item.ToString();
                }
            }
            return res;
        }
        #endregion
    }
}

