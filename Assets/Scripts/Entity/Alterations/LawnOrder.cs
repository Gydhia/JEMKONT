using DownBelow.Entity;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells.Alterations
{

    public class LawnOrder : Alteration
    {
        public LawnOrder(int Duration) : base(Duration)
        {
        }
        public PlayerBehavior characterHasToDraw;

        public override void Setup(CharacterEntity entity)
        {
            base.Setup(entity);
            entity.OnDeath += Draw;
        }

        private void Draw(Events.GameEventData Data)
        {
            SettingsManager.Instance.CombatPreset.MaxCardsInHand - characterHasToDraw.;
            characterHasToDraw.Deck.DrawCard();
        }
    }
}