using Jemkont.Events;
using Jemkont.GridSystem;
using Jemkont.Managers;
using Jemkont.Spells;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Jemkont.Entity {
    public abstract class CharacterEntity : MonoBehaviour {
        public delegate void StatModified();

        public event SpellEventData.Event OnHealthRemoved;
        public event SpellEventData.Event OnHealthAdded;
        public event SpellEventData.Event OnShieldRemoved;
        public event SpellEventData.Event OnShieldAdded;
        public event SpellEventData.Event OnStrengthRemoved;
        public event SpellEventData.Event OnStrengthAdded;
        public event SpellEventData.Event OnInspirationRemoved;
        public event SpellEventData.Event OnInspirationAdded;
        public event SpellEventData.Event OnSpeedRemoved;
        public event SpellEventData.Event OnSpeedAdded;
        public event SpellEventData.Event OnManaRemoved;
        public event SpellEventData.Event OnManaAdded;
        public event SpellEventData.Event OnDefenseRemoved;
        public event SpellEventData.Event OnDefenseAdded;
        public event SpellEventData.Event OnRangeRemoved;
        public event SpellEventData.Event OnRangeAdded;

        protected EntityStats RefStats;

        public UnityEngine.UI.Slider HealthFill;
        public UnityEngine.UI.Slider ShieldFill;

        public int TurnOrder;
        public bool IsAlly = true;
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

        public int MaxHealth { get => RefStats.Health; set => RefStats.Health = value; }
        public Dictionary<EntityStatistics,int> Statistics;
        public int Health { get => Statistics[EntityStatistics.Health]; }
        public int Shield { get => Statistics[EntityStatistics.Shield]; }
        public int Strength { get => Statistics[EntityStatistics.Strength]; }
        public int Speed { get => Statistics[EntityStatistics.Speed]; }
        public int Mana { get => Statistics[EntityStatistics.Mana]; }
        public int Defense { get => Statistics[EntityStatistics.Defense]; }
        public int Range { get => Statistics[EntityStatistics.Range]; }

        public abstract Spell AutoAttackSpell();
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

                this.EntityCell = CurrentPath[targetCell];
                this.EntityCell.Datas.state = CellState.EntityIn;

                currentCell++;
                targetCell++;

                if (targetCell <= this.CurrentPath.Count - 1)
                    this.NextCell = CurrentPath[targetCell];
            }

            this.moveCor = null;
            this.IsMoving = false;
        }

        #endregion
        //TODO: CAll AutoAttack() with cell and everything.Need to drag/drop from player.
        /// <summary>
        /// Tries to attack the given cell.
        /// </summary>
        /// <param name="cellToAttack">The cell to attack.</param>
        public void AutoAttack(Cell cellToAttack) {
            if (isInAttackRange(cellToAttack)) {
                //Normally already verified. Just in case
                //Calculate straight path, see if obstacle.
                this.CanAutoAttack = false;
                GridManager.Instance.FindPath(this,cellToAttack.PositionInGrid,true);
                var notwalkable = GridManager.Instance.Path.Find(x => x.Datas.state != CellState.Walkable);
                if (notwalkable != null) {
                    switch (notwalkable.Datas.state) {
                        case CellState.Walkable:
                            // :)
                            break;
                        case CellState.Blocked:
                            //TODO : This blocked? What do we do? At least we do not attack. but it counts.
                            break;
                        case CellState.Shared:
                            //TODO: figure out wtf does that mean
                            break;
                        case CellState.EntityIn:
                            CastAutoAttack(notwalkable);
                            break;
                    }
                } else {
                    //There isn't any obstacle in the path, so the attack should go for it.
                    CastAutoAttack(cellToAttack);
                    //TODO: Shield/overheal? What do i do? Have we got shield in the game??????????????????????
                }
            }
        }
        protected void CastAutoAttack(Cell cell) {
            AutoAttackSpell().ExecuteSpell(this,cell);
        }
        public bool isInAttackRange(Cell cell) {
            bool res = Range >= Mathf.Abs(cell.PositionInGrid.latitude - EntityCell.PositionInGrid.latitude) + Mathf.Abs(cell.PositionInGrid.longitude - EntityCell.PositionInGrid.longitude);
            return res;
        }
        public void EndTurn() {
            CanAutoAttack = false;
        }

        public virtual void StartTurn() {
            CanAutoAttack = true;
            this.ReinitializeStat(EntityStatistics.Speed);
            this.ReinitializeStat(EntityStatistics.Mana);
            GridManager.Instance.ShowPossibleCombatMovements(this);
        }

        public virtual void Init(EntityStats stats,Cell refCell,WorldGrid refGrid,int order = 0) {
            this.transform.position = refCell.WorldPosition;
            this.EntityCell = refCell;
            this.CurrentGrid = refGrid;

            this.RefStats = stats;
            this.Statistics = new Dictionary<EntityStatistics,int>();

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
                case EntityStatistics.Mana: this.Statistics[EntityStatistics.Mana] = this.RefStats.Mana; break;
                case EntityStatistics.Speed: this.Statistics[EntityStatistics.Speed] = this.RefStats.Speed; break;
                case EntityStatistics.Strength: this.Statistics[EntityStatistics.Strength] = this.RefStats.Strength; break;
                case EntityStatistics.Defense: this.Statistics[EntityStatistics.Defense] = this.RefStats.Defense; break;
                case EntityStatistics.Range: this.Statistics[EntityStatistics.Range] = this.RefStats.Range; break;
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
                value = Mathf.Max(0,Defense - value);

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

        public void ApplySpeed(int value) {
            Statistics[EntityStatistics.Speed] += value;

            if (value > 0)
                this.OnSpeedAdded?.Invoke(new SpellEventData(this,value));
            else
                this.OnSpeedRemoved?.Invoke(new SpellEventData(this,-value));
        }

        public void ApplyStrength(int value) {
            Statistics[EntityStatistics.Strength] += value;

            if (value > 0)
                this.OnStrengthAdded?.Invoke(new SpellEventData(this,value));
            else
                this.OnStrengthRemoved?.Invoke(new SpellEventData(this,-value));
        }

        public override string ToString() {
            return @$"Name : {name}
IsAlly : {IsAlly}
GridPos : {EntityCell}";
        }
    }
}

