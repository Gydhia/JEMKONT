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

        public void Init()
        {
            this.Inited = true;

            int playersNb = GameManager.Instance.Players.Count;

            TMP_Dropdown.OptionDataList options = new TMP_Dropdown.OptionDataList();
            // empty option
            options.options.Add(new TMP_Dropdown.OptionData());

            this.SelfDropdown.ClearOptions();
            foreach (var deck in ToolsManager.Instance.AvailableTools)
            {
                options.options.Add(new TMP_Dropdown.OptionData(deck.InventoryIcon));
            }
            this.SelfDropdown.AddOptions(options.options);

            this.SelfDropdown.onValueChanged.AddListener((i) => this._updateSelectableDecks(i));
        }

        private void _updateSelectableDecks(int index)
        {

        }
    }
}