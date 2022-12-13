using DownBelow.Events;
using DownBelow.Inventory;
using DownBelow.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Managers
{
    public class UIManager : _baseManager<UIManager>
    {
        public UIStaticTurnSection TurnSection;
        public UIPlayerInfos PlayerInfos;
        public UICardSection CardSection;

        public UIPlayerInventory PlayerInventory;
        public UIStorage Storage;

        public void Init()
        {
            this.TurnSection.gameObject.SetActive(false);
            this.PlayerInfos.gameObject.SetActive(false);
            this.CardSection.gameObject.SetActive(false);

            CombatManager.Instance.OnCombatStarted += this.SetupCombatInterface;
        }

        public void SetupCombatInterface(GridEventData Data)
        {
            if (Data.Grid.IsCombatGrid) {
                this._setupCombatInterface();
            } else {
                this._setupOutOfCombatInterface();
            }
        }

        private void _setupCombatInterface()
        {
            this.TurnSection.gameObject.SetActive(true);
            this.PlayerInfos.gameObject.SetActive(true);
            this.CardSection.gameObject.SetActive(true);
        }
        private void _setupOutOfCombatInterface()
        {
            this.TurnSection.gameObject.SetActive(false);
            this.PlayerInfos.gameObject.SetActive(false);
            this.CardSection.gameObject.SetActive(false);
        }
    }
}

