using DownBelow.Entity;
using DownBelow.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells.Alterations
{

    public class LeechRain : Alteration
    {
        public int DamageIncrease = 1;

        public LeechRain(int Duration) : base(Duration)
        {
        }

        public override void Setup(CharacterEntity entity)
        {
            base.Setup(entity);
            ((PlayerBehavior)Target).OnCardPlayed += Buff;
        }

        private void Buff(CardEventData Data)
        {
            Target.ApplyStat(EntityStatistics.Strength, DamageIncrease);
        }

        protected override void Unsubbing(GameEventData Data)
        {
            base.Unsubbing(Data);
            ((PlayerBehavior)Target).OnCardPlayed -= Buff;

        }
    }
}