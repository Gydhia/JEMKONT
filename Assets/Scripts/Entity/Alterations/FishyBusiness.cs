using DownBelow.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells.Alterations
{
    public class FishyBusiness : Alteration
    {
        public PlayerBehavior Player { get; set; }
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
            Player.Deck.DrawCard();
        }

        public override void WearsOff(CharacterEntity entity)
        {
            base.WearsOff(entity);
            Target.OnPushed -= FishyDraw;
        }
    }
}