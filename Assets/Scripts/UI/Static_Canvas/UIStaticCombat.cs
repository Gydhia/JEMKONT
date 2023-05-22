using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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
            StartCombat.onClick.AddListener(() => NetworkManager.Instance.PlayerAsksToStartCombat());
            LeaveCombat.onClick.AddListener(() => NetworkManager.Instance.PlayerAskToLeaveCombat());

            this.gameObject.SetActive(false);

            GameManager.Instance.OnEnteredGrid += _toggleCombatUI;
            GameManager.Instance.OnEnteredGrid += _updateDropdowns;
            GameManager.Instance.OnExitingGrid += _toggleCombatUI;

            CombatManager.Instance.OnCombatStarted += _toggleDeckSelectionUI;
            CombatManager.Instance.OnCombatEnded += _toggleDeckSelectionUI;
        }

        private void _toggleCombatUI(EntityEventData data) => this.gameObject.SetActive(data.Entity.CurrentGrid is CombatGrid);

        private void _toggleDeckSelectionUI(GridEventData Data) => this.DeckSelection.SetActive(Data.Grid is CombatGrid cGrid && !cGrid.HasStarted);


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
            foreach (var item in GameManager.Instance.SelfPlayer.PlayerSpecialSlots.StorageItems)
            {
                ToolItem toolPreset = item.ItemPreset as ToolItem;

                // +1 since the value 0 is for none
                this.DeckDropdowns[counter].SelfDropdown.value = (ToolsManager.Instance.AvailableTools.IndexOf(toolPreset) + 1);
                this.DeckDropdowns[counter].SelfDropdown.RefreshShownValue();
                this.DeckDropdowns[counter].SelfDropdown.interactable = false;
                counter++;
            }
        }
    }
}