using DownBelow.Mechanics;
using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck
{
    public List<ScriptableCard> Cards;
    public int Count => Cards.Count;
    public void ShuffleDeck() {
        Cards.Shuffle();
    }
    public ScriptableCard DrawCard() {
        var res = Cards[0];
        Cards.RemoveAt(0);
        return res;
    }
    public Deck Copy() {
        return new Deck(Cards);
    }
    public Deck(List<ScriptableCard> cards) {
        Cards = cards;
    }
    public Deck(Deck other) {
        Cards = other.Cards;
    }
}

