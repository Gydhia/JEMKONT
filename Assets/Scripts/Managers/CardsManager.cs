using DownBelow.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;

namespace DownBelow.Managers
{
    [ShowOdinSerializedPropertiesInInspector]
    public class CardsManager : _baseManager<CardsManager>
    {
        public Dictionary<Guid, DeckScriptable> DeckPresets;
        public Dictionary<Guid, ScriptableCard> ScriptableCards;

        public void Init()
        {
            this._loadScriptableCards();
        }

        private void _loadScriptableCards()
        {
            List<DeckScriptable> decks = Resources.LoadAll<DeckScriptable>("Presets/Decks").ToList();

            this.DeckPresets = new Dictionary<Guid, DeckScriptable>();
            this.ScriptableCards = new Dictionary<Guid, ScriptableCard>();

            foreach (var deck in decks)
            {
                this.DeckPresets.Add(deck.UID, deck);

                // Only take the unique cards
                foreach (var card in deck.Deck.Cards)
                {
                    if (!this.ScriptableCards.ContainsKey(card.UID))
                        this.ScriptableCards.Add(card.UID, card);
                }
            }
        }
    }
}

