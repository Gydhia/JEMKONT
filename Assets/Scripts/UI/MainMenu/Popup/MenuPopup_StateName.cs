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
        public Action OnPlayerNameValidated;

        public TMP_InputField PlayerNameInput;
        [SerializeField] 
        private Button _validationPlayerNameButton;

        private void Start()
        {
            this.PlayerNameInput.onValueChanged.AddListener(x => this.processNameUpdate(x));
        }

        protected void processNameUpdate(string newName)
        {
            NetworkManager.Instance.UpdateOwnerName(this.PlayerNameInput.text);
            this.HidePopup();
        }
    }
}