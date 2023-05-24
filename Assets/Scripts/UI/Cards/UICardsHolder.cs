using DownBelow.Managers;
using DownBelow.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;


public class UICardsHolder : SerializedMonoBehaviour
{
    public CanvasGroup CanvasGroup;

    public Dictionary<PileType, UICardsPile> Piles;

    public void MoveCard(PileType FromPile, PileType ToPile, DraggableCard RefCard)
    {
        this.MoveCard(FromPile, ToPile, this.Piles[FromPile].Cards.IndexOf(RefCard));
    }

    public void MoveCard(PileType FromPile, PileType ToPile, int specificIndex = 0)
    {
        var FromCards = this.Piles[FromPile].Cards;
        var ToCards = this.Piles[ToPile].Cards;

        if (specificIndex == -1)
            specificIndex = 0;

        if (FromCards.Count > 0)
        {
            // Get a card from the FromPile --TO--> ToPile
            ToCards.Add(FromCards[specificIndex]);
            FromCards.RemoveAt(specificIndex);

            ToCards[^1].DrawFromPile(this.Piles[FromPile], this.Piles[ToPile]);
        }

        if (this.Piles[ToPile].MaxStackedCards != -1)
        {
            if (ToCards.Count > this.Piles[ToPile].MaxStackedCards)
            {
                ToCards[^1].DiscardToPile(this.Piles[ToPile]);
                ToCards.Remove(ToCards[^1]);
            }
        }
    }

    public void CheckMainPilesState()
    {
        var drawPile = this.Piles[PileType.Draw].Cards;
        var discardPile = this.Piles[PileType.Discard].Cards;

        if (drawPile.Count == 0)
        {
            this.ShufflePile(PileType.Discard);

            for (int i = discardPile.Count - 1; i >= 0; i--)
            {
                drawPile.Add(discardPile[i]);
                discardPile.RemoveAt(i);
            }
        }
    }

    public int GetCardIndex(PileType Pile,DraggableCard RefCard)
    {
        return this.Piles[Pile].Cards.IndexOf(RefCard);
    }
    public void AddRangeToPile(PileType Pile, List<DraggableCard> Cards, bool suffle = false)
    {
        this.Piles[Pile].Cards.AddRange(Cards);
        if (suffle)
        {
            this.ShufflePile(Pile);
        }
    }

    public void ShufflePile(PileType Pile)
    {
        this.Piles[Pile].Cards.Shuffle();
    }
}
