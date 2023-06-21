using DownBelow.Entity;
using DownBelow.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells.Alterations
{
    public class SomberStitch : Alteration
    {
        public CharacterEntity EntityStitched;

        public SomberStitch(int Cooldown, CharacterEntity entityStitched) : base(Cooldown)
        {
            EntityStitched = entityStitched;
        }

        public override void Setup(CharacterEntity entity)
        {
            base.Setup(entity);
            Target.OnHealthRemoved += ReplicateDamage;
        }

        private void ReplicateDamage(Events.SpellEventData Data)
        {
            EntityStitched.ApplyStat(EntityStatistics.Health, -Data.Value, false);
            EntityStitched.AreYouAlive(Data);
        }

        protected override void Unsubbing(EntityEventData Data)
        {
            base.Unsubbing(Data);
            Target.OnHealthRemoved -= ReplicateDamage;
        }
    }

}
