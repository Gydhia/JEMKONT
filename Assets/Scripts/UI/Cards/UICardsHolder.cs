using DownBelow.Entity;
using DownBelow.Managers;
using DownBelow.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;


public class UICardsHolder : SerializedMonoBehaviour
{
    public CanvasGroup CanvasGroup;
    public PlayerBehavior LinkedPlayer;

    public Dictionary<PileType, UICardsPile> Piles;

    public bool IsPileOpen;
    
    public int PileSize(PileType type)
    {
        return Piles[type].Cards.Count;
    }

    public void MoveCard(PileType FromPile, PileType ToPile, DraggableCard RefCard, bool drawed = true)
    {
        this.MoveCard(FromPile, ToPile, this.Piles[FromPile].Cards.IndexOf(RefCard), drawed);
    }

    public void MoveCard(PileType FromPile, PileType ToPile, int specificIndex = 0, bool drawed = true)
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

            if(drawed)
                ToCards[^1].DrawFromPile(this.Piles[FromPile], this.Piles[ToPile]);
            else
                ToCards[^1].DiscardToPile(this.Piles[ToPile]);
        }

        if (this.Piles[ToPile].MaxStackedCards != -1)
        {
            if (ToCards.Count > this.Piles[ToPile].MaxStackedCards)
            {
                this.Piles[PileType.Discard].Cards.Add(ToCards[^1]);
                ToCards.Remove(ToCards[^1]);

                this.Piles[PileType.Discard].Cards[^1]
                    .DiscardToPile(this.Piles[PileType.Discard]);
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
        this.Piles[Pile].ShufflePile(this.LinkedPlayer.UID);
    }
}
