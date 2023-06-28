using DownBelow.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells.Alterations
{
    public class FishyBusiness : Alteration
    {
        public PlayerBehavior player;
        public FishyBusiness(int Cooldown) : base(Cooldown)
        {
        }
        public int CardsToDraw = 1;

        public override void Setup(CharacterEntity entity)
        {
            base.Setup(entity);
            Target.OnPushed += FishyDraw;
        }

        private void FishyDraw(Events.SpellEventData Data)
        {

        }

        public override void WearsOff(CharacterEntity entity)
        {
            base.WearsOff(entity);
            Target.OnPushed -= FishyDraw;
        }
    }
}