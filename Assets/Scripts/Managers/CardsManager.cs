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

        public void Init()
        {          
            CombatManager.Instance.OnCombatStarted += _setupForCombat;
            CombatManager.Instance.OnCombatEnded += _endForCombat;
        }

        private void _setupForCombat(GridEventData Data)
        {
            CombatGrid combatGrid = Data.Grid as CombatGrid;

            var allPlayers = combatGrid.GridEntities.Where(e => e is PlayerBehavior player && player != GameManager.RealSelfPlayer && player.Deck != null).Cast<PlayerBehavior>();

            // To keep order, we exclude selfPlayer from the rest so we're sure that he's first
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
        #endregion

        #region TOOLS
        [OdinSerialize()] public Dictionary<EClass, ToolItem> ToolInstances = new();//Je vois des choses

        public List<ToolItem> AvailableTools;
        #endregion

    }
}

