using DownBelow.Entity;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells.Alterations
{
    public class Thornmail : Alteration
    {
        private int multiplier = 1;
        public Thornmail(int Cooldown) : base(Cooldown)
        {
        }

        public override void Apply(CharacterEntity entity)
        {
            //base.Apply replays an animation on beginning of turn.
        }
        public override void Setup(CharacterEntity entity)
        {
            base.Setup(entity);
            entity.OnHealthRemoved += SpikeyDamage;
        }

        public override void WearsOff(CharacterEntity entity)
        {
            base.WearsOff(entity);
            entity.OnHealthRemoved -= SpikeyDamage;
        }

        private void SpikeyDamage(Events.SpellEventData Data)
        {
            Debug.Log($"{Data.Entity.name} did damage to {this.Target} but they have spikey damage!");
            NetworkManager.Instance.EntityAskToBuffAction(new Spell_Stats(new SpellData_Stats(true, EntityStatistics.Health, this.Target.Defense*multiplier), this.Target, Data.Entity.EntityCell, null, null, 0));
        }
    }

}
