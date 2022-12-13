using DownBelow.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
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
                    case EntityStatistics.Movement:
                        entity.ApplyMovement(this.StatisticValue);
                        break;
                    case EntityStatistics.Strenght:
                        entity.ApplyStrenght(this.StatisticValue);
                        break;
                    case EntityStatistics.Dexterity:
                        entity.ApplyDexterity(this.StatisticValue);
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