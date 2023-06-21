using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIStaticEscape : MonoBehaviour
    {
        public Button SaveButton;

        public MenuPopup_Options OptionsPopup;

        private void OnEnable()
        {
            // Only host can save HIS game
            this.SaveButton.interactable = Photon.Pun.PhotonNetwork.IsMasterClient;
            this.OptionsPopup.Init();
        }

        public void OnClickResume()
        {
            UIManager.Instance.SwitchEscapeState();
        }

        public void OnClickSave()
        {
            GameManager.Instance.Save(GameData.Game.RefGameDataContainer.SaveName);
        }

        public void OnClickOptions()
        {
            this.OptionsPopup.ShowPopup();
        }

        public void OnClickBackToMenu()
        {
            NetworkManager.Instance.LoadScene(LevelName.MainMenu);
        }
    }
}