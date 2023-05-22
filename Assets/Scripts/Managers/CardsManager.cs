using DownBelow.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using DownBelow.Events;
using DownBelow.UI;
using EODE.Wonderland;

namespace DownBelow.Managers
{
    [ShowOdinSerializedPropertiesInInspector]
    public class CardsManager : _baseManager<CardsManager>
    {
        #region DATAS
        /// <summary>
        /// Do not use this, Inspector only. Use <see cref="DeckPresets"/>
        /// </summary>
        public List<DeckPreset> AvailableDecks;

        [HideInInspector]
        public Dictionary<Guid, DeckPreset> DeckPresets;
        public Dictionary<Guid, ScriptableCard> ScriptableCards;
        public List<ScriptableCard> OwnedCards;

        public void Init()
        {
            this._loadScriptableCards();

            // Generate decks according to what we plugged. We need a Guid access for easy network
            this.DeckPresets = new Dictionary<Guid, DeckPreset>();
            foreach (var deck in this.AvailableDecks)
            {
                this.DeckPresets.Add(deck.UID, deck);
            }

            CombatManager.Instance.OnCombatStarted += _setupForCombat;
            CombatManager.Instance.OnCombatEnded += _endForCombat;
        }

        private void _loadScriptableCards()
        {
            List<DeckPreset> decks = Resources.LoadAll<DeckPreset>("Presets/Decks").ToList();

            this.DeckPresets = new Dictionary<Guid, DeckPreset>();
            this.ScriptableCards = new Dictionary<Guid, ScriptableCard>();
            this.OwnedCards = new List<ScriptableCard>();

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
        #endregion

        #region COMBAT
        public DraggableCard CardPrefab;

        public DeckPreset ReferenceDeck;

        public List<DraggableCard> DrawPile = new List<DraggableCard>();
        public List<DraggableCard> DiscardPile = new List<DraggableCard>();
        public List<DraggableCard> HandPile = new List<DraggableCard>();
        public List<DraggableCard> ExhaustedPile = new List<DraggableCard>();

        private void _setupForCombat(GridEventData Data)
        {
            GameManager.Instance.SelfPlayer.OnTurnEnded += SelfDrawCard;
            CombatManager.Instance.OnCardEndUse += TryDiscardCard;

            ReferenceDeck = GameManager.Instance.SelfPlayer.Deck;

            foreach (var card in ReferenceDeck.Deck.Cards)
            {
                this.DrawPile.Add(Instantiate(CardPrefab, UIManager.Instance.CardSection.DrawPile.transform));
                this.DrawPile[^1].Init(card);
            }

            for (int i = 0; i < SettingsManager.Instance.CombatPreset.CardsToDrawAtStart; i++)
            {
                DrawCard();
            }
        }

        private void _endForCombat(GridEventData Data)
        {
            GameManager.Instance.SelfPlayer.OnTurnEnded -= SelfDrawCard;
        }

        public void SelfDrawCard(GameEventData Data)
        {
            for (int i = 0; i < SettingsManager.Instance.CombatPreset.CardsToDrawAtStart; i++)
            {
                DrawCard();
            }
        }

        public void TryDiscardCard(CardEventData Data)
        {
            if (!Data.Played)
                return;

            Data.DraggableCard.DiscardToPile();

            this.HandPile.Remove(Data.DraggableCard);
            this.DiscardPile.Add(Data.DraggableCard);
        }

        public void DrawCard(ScriptableCard card)
        {
            this.HandPile.Add(Instantiate(CardPrefab, UIManager.Instance.CardSection.DrawPile.transform));
            this.HandPile[^1].Init(card);
        }

        public void DrawCard()
        {
            this.checkPilesState();
            
            if(this.DrawPile.Count > 0)
            {
                // Get a card from the draw pile --TO--> hand
                this.HandPile.Add(this.DrawPile[0]);
                this.DrawPile.RemoveAt(0);

                this.HandPile[^1].DrawFromPile();
            }

            if (HandPile.Count > 7)
            {
                this.HandPile[^1].DiscardToPile();
                this.HandPile.Remove(this.HandPile[^1]);
            }

            this.checkPilesState();
        }

        protected void checkPilesState()
        {
            if (DrawPile.Count == 0)
            {
                this.ShufflePile(ref this.DiscardPile);

                for (int i = this.DiscardPile.Count - 1; i >= 0; i--)
                {
                    this.DrawPile.Add(this.DiscardPile[i]);
                    this.DiscardPile.RemoveAt(i);
                }
            }
        }

        public void ShufflePile(ref List<DraggableCard> cards)
        {
            cards.Shuffle();
        }

        #endregion
    }
}

