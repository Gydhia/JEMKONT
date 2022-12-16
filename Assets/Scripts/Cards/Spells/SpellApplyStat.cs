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
                if (ModifiedStatistic == EntityStatistics.Health && spellRef.Caster.Critical && StatisticValue < 0)
                    StatisticValue *= 2;
                entity.ApplyStat(this.ModifiedStatistic, this.StatisticValue, false);;
            }
            this.HasEnded = true;

            Destroy(this.gameObject, 1f);
        }

    }
}