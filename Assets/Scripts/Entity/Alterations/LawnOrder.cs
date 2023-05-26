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
            int max = characterHasToDraw.Deck.RefCardsHolder.Piles[PileType.Hand].MaxStackedCards - characterHasToDraw.Deck.RefCardsHolder.PileSize(PileType.Hand);
            for (int i = 0;i < max;i++)
            {
                characterHasToDraw.Deck.DrawCard();
            }
        }
    }
}