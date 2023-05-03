using DownBelow.UI.Menu;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.Managers
{
    public enum MenuPopup
    {
        /*
        StateName,
        SaveSelection,
        RoomCreation,
        Room,
        Lobbies
        */
        None = 0,
        StateName = 1,
        Saves = 2,
        Host = 3,
        Join = 4,
        Lobby = 5,

        Option = 20,

        Close = 99
    }

    public class MenuManager : _baseManager<MenuManager>
    {
        protected GameData.GameDataContainer selectedSave;

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
                this._menuPopups[fPage.PopupType].Init();
            }

            this.SelectPopup(MenuPopup.StateName);
        }

        public void SelectPopup(MenuPopup popup)
        {
            if (this._lastPopup == popup)
                return;

            if (this._lastPopup != MenuPopup.None && !this._menuPopups[this._lastPopup].IsHidden)
            {
                this._menuPopups[this._lastPopup].HidePopup();
            }
            else
            {
                this._lastPopup = popup;
                this.ShowNextPopup();
            }

            this._lastPopup = popup;

            if (!this._popupBuffer.Contains(this._lastPopup))
                this._popupBuffer.Add(this._lastPopup);
        }

        public void ShowNextPopup()
        {
            this._menuPopups[this._lastPopup].ShowPopup();
        }

        public void HideCurrentPopup()
        {
            this._menuPopups[this._lastPopup].HidePopup();
            this._lastPopup = MenuPopup.None;
        }

        public void OnClickQuit()
        {
            Application.Quit();
        }

        public void SelectSave(GameData.GameDataContainer selectedSave)
        {
            this.selectedSave = selectedSave;

            // Means that we've selected a game for the lobby
            if (_popupBuffer.Contains(MenuPopup.Host))
            {
                this.SelectPopup(MenuPopup.Lobby);
            }
            else
            {
                this.StartGame();
            }
        }

        public void StartGame()
        {

        }
    }
}
