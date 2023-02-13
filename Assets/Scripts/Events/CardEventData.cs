using DownBelow.GridSystem;
using DownBelow.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public class CardEventData : EventData<CardEventData>
    {
        public ScriptableCard Card;
        public Cell Cell;
        public bool Played;

        public CardEventData(ScriptableCard Card, Cell Cell = null, bool Played = false)
        {
            this.Card = Card;
            this.Cell = Cell;
            this.Played = Played;
        }
    }
}