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
using Sirenix.Serialization;
using DownBelow.GridSystem;
using DownBelow.Entity;
using System.Diagnostics;

namespace DownBelow.Managers
{
    public enum PileType
    {
        Draw = 1,
        Discard = 2,
        Hand = 3,
        Exausth = 4,

        All = 5
    }

    [ShowOdinSerializedPropertiesInInspector]
    public class CardsManager : _baseManager<CardsManager>
    {
        #region DATAS
        public DraggableCard CardPrefab;

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

            this.ToolPresets = new Dictionary<Guid, ToolItem>();
            foreach (var tool in this.AvailableTools)
            {
                this.ToolPresets.Add(tool.UID, tool);
            }

            CombatManager.Instance.OnCombatStarted += _setupForCombat;
            CombatManager.Instance.OnCombatEnded += _endForCombat;
        }

        private void _setupForCombat(GridEventData Data)
        {
            CombatGrid combatGrid = Data.Grid as CombatGrid;

            var allPlayers = combatGrid.GridEntities.Where(e => e is PlayerBehavior player && player.IsFake && player.Deck != null).Cast<PlayerBehavior>();

            // To keep order
            GameManager.RealSelfPlayer.Deck.SetupForCombat(UIManager.Instance.CardSection.CardsHolders[0]);
            int counter = 1;
            foreach (PlayerBehavior player in allPlayers)
            {
                player.Deck.SetupForCombat(UIManager.Instance.CardSection.CardsHolders[counter]);
                counter++;
            }
        }

        private void _endForCombat(GridEventData Data)
        {
            foreach (var deck in this.AvailableDecks)
            {
                deck.EndForCombat();
            }
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

        #region TOOLS
        [OdinSerialize()] public Dictionary<EClass, ToolItem> ToolInstances = new();//Je vois des choses

        public List<ToolItem> AvailableTools;

        [HideInInspector]
        public Dictionary<Guid, ToolItem> ToolPresets;

        public void AddToInstance(ToolItem toolToAdd)
        {
            if (ToolInstances.TryGetValue(toolToAdd.Class, out ToolItem tool))
            {
                toolToAdd.DeckPreset = tool.DeckPreset; 
                toolToAdd.Class = tool.Class;
                //This might be shitty, we'll see afterwards.
                //TODO: Photon?
            }
            else
            {
                ToolInstances.Add(toolToAdd.Class, toolToAdd);
            }
        }
        #endregion

        #region COMBAT
        


        #endregion
    }
}

