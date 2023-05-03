using DownBelow.Mechanics;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Configuration;
using DownBelow.Managers;
using DownBelow.Events;

[System.Serializable]
public class Deck
{
    public List<ScriptableCard> Cards;
    public int Count => Cards.Count;

    public Deck(List<ScriptableCard> cards)
    {
        this.Cards = new List<ScriptableCard>(cards);
    }
    public Deck(Deck other)
    {
        this.Cards = new List<ScriptableCard>(other.Cards);
    }

    public void ShuffleDeck() 
    {
        this.Cards.Shuffle();
    }
    
    public ScriptableCard DrawCard() 
    {
        var drawedCard = Cards[0];
        Cards.RemoveAt(0);
        return drawedCard;
    }

    #region UTILITY
    public Deck Copy() 
    {
        return new Deck(Cards);
    }
    #endregion
}

