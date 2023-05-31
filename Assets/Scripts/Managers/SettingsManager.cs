using DownBelow.GameData;
using DownBelow.Mechanics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DownBelow.Managers
{
    public class SettingsManager : _baseManager<SettingsManager>
    {
        public GridsPreset GridsPreset;
        public InputPreset InputPreset;
        public CombatPreset CombatPreset;
        public GameUIPreset GameUIPreset;
        public SoundPreset SoundPreset;

        public Deck PlayerDeck;
        public EClass PlayerClass;


        public Dictionary<Guid, BaseSpawnablePreset> SpawnablesPresets;
        public Dictionary<Guid, ItemPreset> ItemsPresets;

        public Dictionary<Guid, DeckPreset> DeckPresets;
        public Dictionary<Guid, ScriptableCard> ScriptableCards;

        public Dictionary<Guid, ToolItem> ToolPresets;

        public List<ScriptableCard> OwnedCards;

        public void Init()
        {
            this.LoadResources();
        }

        public void LoadResources()
        {
            this.OwnedCards = new List<ScriptableCard>();

            this.LoadGridsRelative();
            this.LoadToolsRelative();
        }


        public void LoadGridsRelative()
        {
            var spawnablesPresets = Resources.LoadAll<BaseSpawnablePreset>("Presets").ToList();
            var itemsPresets = Resources.LoadAll<ItemPreset>("Presets/Inventory/Items");

            this.SpawnablesPresets = new Dictionary<Guid, BaseSpawnablePreset>();
            this.ItemsPresets = new Dictionary<Guid, ItemPreset>();

            foreach (var spawnable in spawnablesPresets)
                this.SpawnablesPresets.Add(spawnable.UID, spawnable);
            foreach (var item in itemsPresets)
                this.ItemsPresets.Add(item.UID, item);
        }

        public void LoadToolsRelative()
        {
            // Generate decks according to what we plugged. We need a Guid access for easy network
            this.DeckPresets = new Dictionary<Guid, DeckPreset>();
            this.ScriptableCards = new Dictionary<Guid, ScriptableCard>();
            foreach (var deck in CardsManager.Instance.AvailableDecks)
            {
                this.DeckPresets.Add(deck.UID, deck);

                foreach (var card in deck.Deck.Cards)
                {
                    if (!this.ScriptableCards.ContainsKey(card.UID))
                        this.ScriptableCards.Add(card.UID, card);
                }
            }

            this.ToolPresets = new Dictionary<Guid, ToolItem>();
            foreach (var tool in CardsManager.Instance.AvailableTools)
            {
                this.ToolPresets.Add(tool.UID, tool);
            }
        }
    }

}
