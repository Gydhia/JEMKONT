using DownBelow.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells.Alterations
{

    public class InverseFlow : Alteration
    {
        public InverseFlow(int Cooldown) : base(Cooldown)
        {
        }
        public int DamageFromFlow = 2;

        public override void Setup(CharacterEntity entity)
        {
            base.Setup(entity);
            Target.OnPushed += FlowDamage;
        }

        private void FlowDamage(Events.SpellEventData Data)
        {
            Target.ApplyStat(EntityStatistics.Health, DamageFromFlow);
        }
        public override void WearsOff(CharacterEntity entity)
        {
            base.WearsOff(entity);
            Target.OnPushed -= FlowDamage;
        }
    }
}