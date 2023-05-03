using DownBelow.UI.Menu;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.Managers
{
    public enum MenuPopup
    {
        None = 0,
        Play = 1,
        Host = 2,
        Join = 3,
        Option = 4,
        StateName = 5
    }

    public class MenuManager : _baseManager<MenuManager>
    {
        public GameObject PopupHolders;

        private Dictionary<MenuPopup, BaseMenuPopup> _menuPopups;
        private MenuPopup _lastPopup;
        private List<MenuPopup> _popupBuffer;


        private void Start()
        {
            this._menuPopups = new Dictionary<MenuPopup, BaseMenuPopup>();
            foreach (Transform page in this.PopupHolders.transform)
            {
                var fPage = page.GetComponent<BaseMenuPopup>();

                this._menuPopups.Add(fPage.PopupType, fPage);
            }

            this.SelectPopup(MenuPopup.StateName);
        }

        public void SelectPopup(MenuPopup popup)
        {
            if (this._lastPopup == popup)
                return;

            if (this._lastPopup != MenuPopup.None)
            {
                this._menuPopups[this._lastPopup].HidePopup();
            }
            else
            {
                this._lastPopup = popup;
                this.ShowNextSection();
            }

            this._lastPopup = popup;

            if (!this._popupBuffer.Contains(this._lastPopup))
                this._popupBuffer.Add(this._lastPopup);
        }

        public void ShowNextSection()
        {
            this._menuPopups[this._lastPopup].ShowPopup();
        }

        public void OnClickQuit()
        {
            Application.Quit();
        }
    }
}
