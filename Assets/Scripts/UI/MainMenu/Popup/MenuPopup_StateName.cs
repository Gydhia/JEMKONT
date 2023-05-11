using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI.Menu
{
    public class MenuPopup_StateName : BaseMenuPopup
    {
        public TMP_InputField PlayerNameInput;
        public Button ValidationPlayerNameButton;

        private string playerName;

        private void Start()
        {
            this.ValidationPlayerNameButton.interactable = false;
            this.PlayerNameInput.onValueChanged.AddListener((s) => this._updatePlayerName(s));
            this.ValidationPlayerNameButton.onClick.AddListener(() => this.changePlayerName());
        }

        protected void _updatePlayerName(string value)
        {
            ValidationPlayerNameButton.interactable = !string.IsNullOrEmpty(value);
            this.name = value;
        }

        protected void changePlayerName()
        {
            NetworkManager.Instance.UpdateOwnerName(name);
            this.HidePopup();
        }
    }
}