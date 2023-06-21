using DownBelow.Loading;
using DownBelow.UI.Menu;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
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
        Credits = 6,

        Option = 20,

        Tutorial = 97,
        Close = 98,
        Quit = 99
    }

    public class MenuManager : _baseManager<MenuManager>
    {
        protected GameData.GameDataContainer selectedSave;

        public GameObject PopupHolders;

        public MenuPopup_Lobby UILobby;
        public MenuPopup_Room UIRoom;

        private Dictionary<MenuPopup, BaseMenuPopup> _menuPopups;
        public MenuPopup LastPopup;
        private List<MenuPopup> _popupBuffer = new List<MenuPopup>();
        private bool _isSelectingPopup = false;
        private bool _hadPunConnection = false;

        public bool GoingToHost = false;

        public Button HostButton;
        public Button JoinButton;
        public Button ConnectionButton;

        public TextMeshProUGUI ConnectedText;
        public Image ConnectedDot;

        private void _switchConnectionAspect(Events.GameEventData Data) => this.SwitchConnectionAspect(this._hadPunConnection);
        public void SwitchConnectionAspect(bool connected)
        {
            this._hadPunConnection = connected;
            connected &= NetworkManager.Instance.HasInternet;
            
            this.ConnectedDot.color = connected ? Color.green : Color.red;
            this.ConnectedText.text = connected ? "CONNECTED" : "DISCONNECTED";
            
            this.HostButton.interactable = connected;
            this.JoinButton.interactable = connected;

            this.ConnectionButton.interactable = !connected;
        }

        public override void Awake()
        {
            base.Awake();
            LoadingScreen.Instance.Hide();
        }

        public void OnTryReconnectClick()
        {
            NetworkManager.Instance.TryReconnect();
            SwitchConnectionAspect(this._hadPunConnection);
        }

        public void Init()
        {
            NetworkManager.Instance.OnInternetLost += this._switchConnectionAspect;
            NetworkManager.Instance.OnInternetReached += this._switchConnectionAspect;

            this._initFolders();

            this._menuPopups = new Dictionary<MenuPopup, BaseMenuPopup>();
            foreach (Transform page in this.PopupHolders.transform)
            {
                var fPage = page.GetComponent<BaseMenuPopup>();

                this._menuPopups.Add(fPage.PopupType, fPage);
                this._menuPopups[fPage.PopupType].Init();
            }

            // Only show this the first time
            if (string.IsNullOrEmpty(Photon.Pun.PhotonNetwork.NickName))
            {
                StartCoroutine(SelectPopupDelay(MenuPopup.StateName, 0.5f));
            }
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

            if (this.LastPopup != MenuPopup.None && !this._menuPopups[this.LastPopup].IsHidden)
            {
                this._menuPopups[this.LastPopup].HidePopup();
            }
            else
            {
                this.LastPopup = popup;
                this.ShowNextPopup();
            }

            this.LastPopup = popup;

            if (!this._popupBuffer.Contains(this.LastPopup))
                this._popupBuffer.Add(this.LastPopup);
        }

        public void ShowNextPopup()
        {
            this._menuPopups[this.LastPopup].ShowPopup();
        }

        public void HideCurrentPopup()
        {
            this._menuPopups[this.LastPopup].HidePopup();
            this.LastPopup = MenuPopup.None;
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
                this.SelectPopup(MenuPopup.Room);
            }
            else
            {
                NetworkManager.Instance.ClickOnStart();
            }
        }

        public void StartGame()
        {
            NetworkManager.Instance.ShareSaveThroughRoom();   
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
