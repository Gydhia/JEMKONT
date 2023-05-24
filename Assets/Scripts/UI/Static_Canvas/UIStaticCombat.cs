using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIStaticCombat : MonoBehaviour
    {
        public RectTransform LeftPin;
        public RectTransform RightPin;

        public Button StartCombat;
        public Button LeaveCombat;

        public GameObject DeckSelection;
        public List<UIDeckDropdown> DeckDropdowns;

        public void Init()
        {
            StartCombat.gameObject.SetActive(false);
            LeaveCombat.gameObject.SetActive(false);

            StartCombat.onClick.AddListener(() => NetworkManager.Instance.PlayerAsksToStartCombat());
            LeaveCombat.onClick.AddListener(() => NetworkManager.Instance.PlayerAskToLeaveCombat());

            this.gameObject.SetActive(false);

            GameManager.Instance.OnEnteredGrid += _toggleCombatUI;
            GameManager.Instance.OnEnteredGrid += _updateDropdowns;
            GameManager.Instance.OnExitingGrid += _toggleCombatUI;

            CombatManager.Instance.OnCombatStarted += _toggleDeckSelectionUI;
            CombatManager.Instance.OnCombatEnded += _toggleDeckSelectionUI;
        }

        private void _toggleCombatUI(EntityEventData data) 
        {
            bool inCombatGrid = data.Entity.CurrentGrid is CombatGrid;
            this.gameObject.SetActive(inCombatGrid);
            this.StartCombat.gameObject.SetActive(inCombatGrid); 
            this.LeaveCombat.gameObject.SetActive(inCombatGrid);
        }
        private void _toggleDeckSelectionUI(GridEventData Data) 
        {
            this.DeckSelection.SetActive(Data.Grid is CombatGrid cGrid && !cGrid.HasStarted);
            this.StartCombat.gameObject.SetActive(false);
            this.LeaveCombat.gameObject.SetActive(false);
        } 


        private void _updateDropdowns(EntityEventData data)
        {
            if (!this.DeckDropdowns[0].Inited)
            {
                foreach (var dropdown in this.DeckDropdowns)
                {
                    dropdown.Init();
                }
            }

            int counter = 0;
            foreach (var item in GameManager.SelfPlayer.PlayerSpecialSlots.StorageItems)
            {
                ToolItem toolPreset = item.ItemPreset as ToolItem;

                // +1 since the value 0 is for none
                this.DeckDropdowns[counter].SelfDropdown.value = (CardsManager.Instance.AvailableTools.IndexOf(toolPreset) + 1);
                this.DeckDropdowns[counter].SelfDropdown.RefreshShownValue();
                this.DeckDropdowns[counter].SelfDropdown.interactable = false;
                counter++;
            }
           
            foreach (var player in CombatManager.Instance.PlayersInGrid.Where(p => p != GameManager.SelfPlayer))
            {
                foreach (var playerTool in player.PlayerSpecialSlots.StorageItems)
                {
                    this.DeckDropdowns[counter].gameObject.SetActive(false);
                    counter++;
                }                    
            }

            // Slots remaining
            if(counter < 4)
            {
                int remaining = 4 - counter;
                int playersInGrid = CombatManager.Instance.PlayersInGrid.Count;

                for (int i = 0; i < remaining;)
                {
                    if(playersInGrid == 0)
                    {
                        Debug.LogError("There are no players in grid even after entering, fix the callstack with events");
                        break;
                    }

                    for (int j = 0; j < playersInGrid; j++)
                    {
                        if (CombatManager.Instance.PlayersInGrid[j] == GameManager.SelfPlayer)
                        {
                            this.DeckDropdowns[counter].SelfDropdown.value = counter + 1;
                            this.DeckDropdowns[counter].SelfDropdown.RefreshShownValue();
                            this.DeckDropdowns[counter].SelfDropdown.interactable = false;

                            counter++;
                        }

                        i++;
                    }
                }
                
            }
        }
    }
}