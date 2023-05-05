using DownBelow.UI.Menu;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.Managers
{
    public enum MenuPopup
    {
        None = 0,
        StateName = 1,
        SaveSelection = 2,
        RoomCreation = 3,
        Lobby = 4,
        Room = 5,

        Option = 20,

        Close = 98,
        Quit = 99
    }

    public class MenuManager : _baseManager<MenuManager>
    {
        protected GameData.GameDataContainer selectedSave;

        public GameObject PopupHolders;

        private Dictionary<MenuPopup, BaseMenuPopup> _menuPopups;
        private MenuPopup _lastPopup;
        private List<MenuPopup> _popupBuffer = new List<MenuPopup>();
        private bool _isSelectingPopup = false;

        private bool _goingToHost = false;
        public bool GoingToHost
        {
            get { return this._goingToHost; }
            set
            {
                this._goingToHost = value;
                NetworkManager.Instance.SwitchConnectionState(value);
            }
        }

        public void Init()
        {
            this._initFolders();

            this._menuPopups = new Dictionary<MenuPopup, BaseMenuPopup>();
            foreach (Transform page in this.PopupHolders.transform)
            {
                var fPage = page.GetComponent<BaseMenuPopup>();

                this._menuPopups.Add(fPage.PopupType, fPage);
                this._menuPopups[fPage.PopupType].Init();
            }

            StartCoroutine(SelectPopupDelay(MenuPopup.StateName, 0.5f));
        }

        public IEnumerator SelectPopupDelay(MenuPopup popup, float time)
        {
            this._isSelectingPopup = true;

            yield return new WaitForSeconds(time);

            this._isSelectingPopup = false;
            this.SelectPopup(popup);
        }

        public void SelectPopup(MenuPopup popup)
        {
            // In case we're trying to show a popup with delay
            if (this._isSelectingPopup)
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
            GameData.Game.RefGameDataContainer = selectedSave;

            // Means that we've selected a game for the lobby
            if (this.GoingToHost)
            {
                this.SelectPopup(MenuPopup.Lobby);
            }
            else
            {
                this.StartGame(true);
            }
        }

        public void StartGame(bool solo)
        {
            if (solo)
            {
                NetworkManager.Instance.ClickOnStart();
            }
        }

        private void _initFolders()
        {
            var path = Application.persistentDataPath + "/save/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
