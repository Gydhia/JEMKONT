using DownBelow.GridSystem;
using DownBelow.Mechanics;
using DownBelow.Spells;
using DownBelow.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public class CardEventData : EventData<CardEventData>
    {
        public ScriptableCard Card;
        public DraggableCard DraggableCard;
        public SpellHeader GeneratedHeader;
        public Cell Cell;
        public bool Played;

        public CardEventData(ScriptableCard Card, DraggableCard DraggableCard, SpellHeader GeneratedHeader = null, Cell Cell = null, bool Played = false)
        {
            this.Card = Card;
            this.DraggableCard = DraggableCard;
            this.GeneratedHeader = GeneratedHeader;
            this.Cell = Cell;
            this.Played = Played;
        }
    }
}