using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI.Menu
{
    [RequireComponent(typeof(Button))]
    public class MenuButtonRedirect : MonoBehaviour
    {
        public MenuPopup PopupToGo = MenuPopup.None;
        private Button _selfButton;

        void Awake()
        {
            if(this.TryGetComponent<Button>(out this._selfButton))
            {
                switch (this.PopupToGo)
                {
                    case MenuPopup.Close:
                        this._selfButton.onClick.AddListener(() => MenuManager.Instance.HideCurrentPopup());
                        break;
                    default:
                        this._selfButton.onClick.AddListener(() => MenuManager.Instance.SelectPopup(this.PopupToGo));
                        break;
                }
            }
        }

       
    }
}
