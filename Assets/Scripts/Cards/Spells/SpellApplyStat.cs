using Jemkont.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells
{
    public class SpellApplyStat : SpellAction
    {
        public EntityStatistics ModifiedStatistic;
        public int StatisticValue = 1;

        public override void Execute(List<CharacterEntity> targets, Spell spellRef)
        {
            base.Execute(targets, spellRef);

            foreach (CharacterEntity entity in targets)
            {
                switch (this.ModifiedStatistic)
                {
                    case EntityStatistics.Health:
                        entity.ApplyHealth(this.StatisticValue, false);
                        break;
                    case EntityStatistics.Shield:
                        entity.ApplyShield(this.StatisticValue);
                        break;
                    case EntityStatistics.Mana:
                        entity.ApplyMana(this.StatisticValue);
                        break;
                    case EntityStatistics.Speed:
                        entity.ApplySpeed(this.StatisticValue);
                        break;
                    case EntityStatistics.Strength:
                        entity.ApplyStrength(this.StatisticValue);
                        break;
                    default:
                        break;
                }
            }
            
            this.HasEnded = true;

            Destroy(this.gameObject, 1f);
        }

    }
}