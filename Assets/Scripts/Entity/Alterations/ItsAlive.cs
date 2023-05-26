using DownBelow.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells.Alterations
{
    public class ItsAlive : Alteration
    {
        public ItsAlive(int Cooldown) : base(Cooldown)
        {
        }

        public override void Setup(CharacterEntity entity)
        {
            base.Setup(entity);

            Target.OnDeath += Dont;
        }

        private void Dont(Events.EntityEventData Data)
        {
            Target.OnDeath -= Dont;

            Target.ApplyStat(EntityStatistics.Health, Target.MaxHealth);
            Target.AddAlteration(new SnareAlteration(3));

            WearsOff(Target);
        }
    }
}
