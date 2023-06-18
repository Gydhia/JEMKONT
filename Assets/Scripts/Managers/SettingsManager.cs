using DownBelow.GameData;
using DownBelow.Mechanics;
using EasyTransition;
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
        public ResourcesPreset ResourcesPreset;

        public TransitionSettings BaseTransitionSettings;

        public Deck PlayerDeck;
        public EClass PlayerClass;


        public Dictionary<Guid, BaseSpawnablePreset> SpawnablesPresets;
        public Dictionary<Guid, ItemPreset> ItemsPresets;

        public Dictionary<Guid, DeckPreset> DeckPresets;
        public Dictionary<Guid, ScriptableCard> ScriptableCards;

        public Dictionary<Guid, ToolItem> ToolPresets;

        public List<ScriptableCard> OwnedCards;


        public List<AbyssPreset> AbyssesPresets;
        public List<CraftPreset> CraftRepices;

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

            try
            {
                if (spawnablesPresets != null)
                {
                    foreach (var spawnable in spawnablesPresets)
                        this.SpawnablesPresets.Add(spawnable.UID, spawnable);
                }
                if (itemsPresets != null)
                {
                    foreach (var item in itemsPresets)
                        this.ItemsPresets.Add(item.UID, item);
                }
            }
            catch
            {
                Debug.LogError("COME ON, you have 2 Presets with the same UID. CHANGE THEIR NAME");
            }
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
        /// <summary>
        /// returns all the cards of the given collection.
        /// </summary>
        /// <param name="collection">the given collection.</param>
        /// <returns>all the cards of the given collection.</returns>
        public List<ScriptableCard> CollectionCards(EClass collection)
        {
            return ScriptableCards.Where(x => x.Value.Class == collection).Select(x=>x.Value).ToList();
        }

        public List<ScriptableCard> OwnedClassCards(EClass classs)
        {
            return OwnedCards.FindAll(x => x.Class == classs);
        }

        public int MaxCollectionCount()
        {
            int max = 0;
            foreach (var item in Enum.GetValues(typeof(EClass)))
            {
                max = Mathf.Max(max, CollectionCards((EClass)item).Count);
            }
            return max;
        }
    }

}
