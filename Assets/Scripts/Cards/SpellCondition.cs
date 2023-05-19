using DownBelow.Entity;
using DownBelow.Managers;
using DownBelow.Spells.Alterations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Spells
{
    [CreateAssetMenu(menuName = "Cards/Condition_SO")]
    public class SpellCondition : SerializedScriptableObject
    {
        private SpellResult _currentResult;
        [HideInInspector]
        public List<CharacterEntity> TargetedEntities = new List<CharacterEntity>();

        [BoxGroup("Targeting")]
        public ETargetType TargetType;
        //[BoxGroup("Targeting")]
        //public bool OnlyAffectedTarget;

        [BoxGroup("Damages")]
        public bool Dmgs;
        [BoxGroup("Damages")]
        [EnableIf("@this.Dmgs")]
        public bool OneDamagedTarget;
        [BoxGroup("Damages")]
        [EnableIf("@this.Dmgs")]
        public int Damages;

        [BoxGroup("Healing")]
        public bool Healing;
        [BoxGroup("Healing")]
        [EnableIf("@Healing")]
        public bool OneHealedTarget;
        [BoxGroup("Healing")]
        [EnableIf("@this.Healing")]
        public int Heal;

        [InfoBox("Will check if the targets are altered or not by a certain alteration.")]
        [BoxGroup("Alterations")]
        public bool IsAltered;
        [BoxGroup("Alterations")]
        [EnableIf("@this.IsAltered")]
        public Alteration Alteration;
        [BoxGroup("Alterations")]
        public bool NotAltered;
        [BoxGroup("Alterations")]
        [EnableIf("@this.NotAltered")]
        public Alteration NotAlteration;

        [BoxGroup("Teleported")]
        public bool TeleportedToEntity;
        [BoxGroup("Teleported")]
        [EnableIf("@this.TeleportedToEntity")]
        public bool TeleportedToAlly;
        [BoxGroup("Teleported")]
        [EnableIf("@this.TeleportedToEntity")]
        public bool TeleportedToEnemy;

        public bool Check(SpellResult result)
        {
            // Since it's scriptable object, we need to clear it if used multiple times
            this.TargetedEntities.Clear();
            this._currentResult = result;

            bool validated = false;

            if (this.Dmgs)
                validated = this.CheckDamages();

            if (this.Healing)
                validated = this.CheckHealing();

            if (this.IsAltered)
                validated = this.CheckAlterations();

            if (this.NotAltered)
                validated = this.CheckAlterations(true);

            if (this.TeleportedToEntity)
                validated = this.CheckTeleported();

            this._currentResult = null;
            return validated;
        }

        public bool CheckDamages()
        {
            // If the condition is for one target, or every damages dealt
            bool dmgValidated = false;
            int total = 0;
            foreach (var dmg in this._currentResult.DamagesDealt)
            {
                if (!this.OneDamagedTarget)
                    total += dmg.Value;
                else if (dmg.Value > this.Damages)
                    return dmgValidated = true;

                this.TargetedEntities.Add(dmg.Key);
            }

            return dmgValidated || (!this.OneDamagedTarget && total > this.Damages);
        }

        public bool CheckHealing()
        {
            bool healValidated = false;
            int total = 0;
            foreach (var heal in this._currentResult.HealingDone)
            {
                if (!this.OneHealedTarget)
                    total += heal.Value;
                else if (heal.Value > this.Heal)
                    healValidated = true;

                this.TargetedEntities.Add(heal.Key);
            }

            return healValidated || (!this.OneHealedTarget && total > this.Heal);
        }

        public bool CheckAlterations(bool inverse = false)
        {
            //envicané? 
            // Just need this, with Target being a CharacterEntity
            //Target.Alterations.Any(x => x.GetType() == Buff.GetType());
            foreach (CharacterEntity item in _currentResult.TargetedCells.FindAll(x => x.EntityIn != null).Select(x => x.EntityIn))
            {
                if (inverse)
                {
                    if (!item.Alterations.Any(x => x.GetType() == NotAlteration.GetType()))
                    {
                        TargetedEntities.Add(item);
                    }
                } else
                {
                    if (item.Alterations.Any(x => x.GetType() == Alteration.GetType()))
                    {
                        TargetedEntities.Add(item);
                    }
                }
            }

            return false;
        }

        public bool CheckTeleported()
        {
            if (this._currentResult.TeleportedTo.Count > 0)
            {
                if (TeleportedToEnemy)
                {
                    if (this._currentResult.TeleportedTo.Any(x => x is EnemyEntity))
                    {
                        return true;
                    }
                }
                if (TeleportedToAlly)
                {
                    if (this._currentResult.TeleportedTo.Any(x => x is PlayerBehavior))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public List<CharacterEntity> GetValidatedTargets()
        {
            return this.TargetType switch
            {
                ETargetType.Self => new List<CharacterEntity> { this._currentResult.SpellRef.RefEntity },
                ETargetType.Enemy => this.TargetedEntities.Where(e => !e.IsAlly).ToList(),
                ETargetType.Ally => this.TargetedEntities.Where(e => e.IsAlly).ToList(),
                ETargetType.Entities => this.TargetedEntities,//NEEDS TO TARGET NCES ALSO?????? WHICH ARENT CHARACTERENTITIES????
                ETargetType.All or ETargetType.AllAllies => CombatManager.Instance.PlayingEntities.FindAll(x => x.IsAlly),
                ETargetType.AllEnemies => CombatManager.Instance.PlayingEntities.FindAll(x => !x.IsAlly),
                ETargetType.NCEs => this.TargetedEntities,//NEEDS TO TARGET NCES?????? WHICH ARENT CHARACTERENTITIES????
                ETargetType.CharacterEntities => this.TargetedEntities,
                ETargetType.Empty => null,
                _ => null,// Maybe that this shouldn't be selectable 
            };
        }
    }
}