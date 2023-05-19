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
        public Spell[] GeneratedSpells;
        public Cell Cell;
        public bool Played;

        public CardEventData(ScriptableCard Card, DraggableCard DraggableCard, Spell[] GeneratedSpells = null, Cell Cell = null, bool Played = false)
        {
            this.Card = Card;
            this.DraggableCard = DraggableCard;
            this.GeneratedSpells = GeneratedSpells;
            this.Cell = Cell;
            this.Played = Played;
        }
    }
}