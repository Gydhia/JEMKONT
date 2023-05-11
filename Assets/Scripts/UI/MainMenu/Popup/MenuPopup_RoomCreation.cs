using DownBelow.Managers;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace DownBelow.UI.Menu
{
    public class MenuPopup_RoomCreation : BaseMenuPopup
    {
        public TMP_InputField RoomInput;
        public Button CreateButton;

        private string _currentName = string.Empty;

        protected void Awake()
        {
            this.CreateButton.interactable = false;
            this.RoomInput.onValueChanged.AddListener((s) => this._updateInputValidation(s));
        }

        private void _updateInputValidation(string newValue)
        {
            this._currentName = newValue;
            this.CreateButton.interactable = !string.IsNullOrEmpty(newValue);
        }
    }
}