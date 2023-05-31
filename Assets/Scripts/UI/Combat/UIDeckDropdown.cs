using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIDeckDropdown : MonoBehaviour
    {
        public TMP_Dropdown SelfDropdown;
        public bool Inited = false;
        private int _lastIndex = 0;

        public void Init()
        {
            this.Inited = true;

            int playersNb = GameManager.Instance.Players.Count;

            TMP_Dropdown.OptionDataList options = new TMP_Dropdown.OptionDataList();
            // empty option
            options.options.Add(new TMP_Dropdown.OptionData());

            this.SelfDropdown.ClearOptions();
            foreach (var deck in CardsManager.Instance.AvailableTools)
            {
                options.options.Add(new TMP_Dropdown.OptionData(deck.InventoryIcon));
            }
            this.SelfDropdown.AddOptions(options.options);

            this.SelfDropdown.onValueChanged.AddListener((i) => this._updateSelectableDecks(i));
        }

        private void _updateSelectableDecks(int index)
        {
            ToolItem tool;
            if (this._lastIndex != 0)
            {
                tool = CardsManager.Instance.AvailableTools.ElementAt(this._lastIndex - 1);

                // Remove the previous selected tool 
                if (GameManager.SelfPlayer.CombatTools.Contains(tool))
                {
                    GameManager.SelfPlayer.CombatTools.Remove(tool);
                }
            }

            this._lastIndex = index;

            // Index 0 is for none
            if (index == 0) { return; }

            tool = CardsManager.Instance.AvailableTools.ElementAt(this._lastIndex - 1);

            // Add the new one
            if (!GameManager.SelfPlayer.CombatTools.Contains(tool))
            {
                GameManager.SelfPlayer.CombatTools.Add(tool);
            }
        }
    }
}