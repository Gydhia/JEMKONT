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
            // TODO : replug it but not this method
            //LeaveCombat.onClick.AddListener(() => NetworkManager.Instance.PlayerAskToLeaveCombat());

            this.gameObject.SetActive(false);

            GameManager.Instance.OnEnteredGrid += _toggleCombatUI;
            GameManager.Instance.OnEnteredGrid += _updateDropdowns;
            GameManager.Instance.OnExitingGrid += _toggleCombatUI;

            CombatManager.Instance.OnCombatStarted += _toggleDeckSelectionUI;
            CombatManager.Instance.OnCombatEnded += _toggleDeckSelectionUI;
        }

        private void _toggleCombatUI(EntityEventData Data) 
        {
            if (Data.Entity != GameManager.RealSelfPlayer)
                return;

            bool inCombatGrid = Data.Entity.CurrentGrid.IsCombatGrid;
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
            if (!GameManager.RealSelfPlayer.CurrentGrid.IsCombatGrid)
                return;

            if (!this.DeckDropdowns[0].Inited)
            {
                foreach (var dropdown in this.DeckDropdowns)
                {
                    dropdown.Init();
                }
            }

            int counter = 0;
            foreach (var tool in GameManager.RealSelfPlayer.CombatTools)
            {
                // +1 since the value 0 is for none
                this.DeckDropdowns[counter].gameObject.SetActive(true);

                this.DeckDropdowns[counter].SelfDropdown.SetValueWithoutNotify(CardsManager.Instance.AvailableTools.IndexOf(tool) + 1);
                this.DeckDropdowns[counter].SelfDropdown.RefreshShownValue();
                this.DeckDropdowns[counter].SelfDropdown.interactable = false;
                counter++;
            }

            for (int i = counter; i < CardsManager.Instance.AvailableTools.Count; i++)
            {
                this.DeckDropdowns[i].gameObject.SetActive(false);
            }
        }

        public void OnClickResetDebugBtn()
        {
            if(DraggableCard.SelectedCard != null)
            {
                DraggableCard.SelectedCard.RefreshCardValues();
            }
        }
    }
}