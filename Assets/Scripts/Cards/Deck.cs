using DownBelow.Mechanics;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck
{
    private System.Random rng = new System.Random();
    public List<ScriptableCard> Cards;
    public int Count => Cards.Count;
    public void ShuffleDeck() {
        int n = Cards.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            ScriptableCard value = Cards[k];
            Cards[k] = Cards[n];
            Cards[n] = value;
        }
    }
    public ScriptableCard DrawCard() {
        var res = Cards[0];
        Cards.RemoveAt(0);
        return res;
    }
    public Deck Copy() 
    {
        return new Deck(Cards);
    }
    public Deck(List<ScriptableCard> cards) {
        this.Cards = new List<ScriptableCard>(cards);
    }
    public Deck(Deck other) 
    {
        this.Cards = new List<ScriptableCard>(other.Cards);
    }
}

