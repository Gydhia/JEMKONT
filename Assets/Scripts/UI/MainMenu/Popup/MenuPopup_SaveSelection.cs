using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI.Menu
{
    public class MenuPopup_SaveSelection : BaseMenuPopup
    {
        public GameObject SavesHolder;
        public UISaveItem SaveItemPrefab;

        public Button B_AskNewSave;
        public Button B_CreateNewSave;
        public TMP_InputField I_SaveNameInput;

        protected string SaveName = string.Empty;

        public override void Init()
        {
            base.Init();

            DirectoryInfo folder = new System.IO.DirectoryInfo(Application.persistentDataPath + "/save/");
            FileInfo[] saves = folder.GetFiles("*.dbw");

            foreach (FileInfo gamefile in saves.OrderBy(file => file.LastWriteTime))
            {
                UISaveItem game_button = GameObject.Instantiate<UISaveItem>(this.SaveItemPrefab, this.SavesHolder.transform);
                game_button.Init(gamefile);
                game_button.transform.SetAsFirstSibling();
            }

            this.I_SaveNameInput.onValueChanged.AddListener((s) => this.updateSaveName(s));
            this.B_AskNewSave.onClick.AddListener(() => this.hideAskNewSave());
            this.B_CreateNewSave.onClick.AddListener(() => this.CreateNewSave());
        }

        public override void HidePopup()
        {
            this.B_AskNewSave.gameObject.SetActive(true);

            base.HidePopup();
        }

        protected void hideAskNewSave()
        {
            this.B_AskNewSave.gameObject.SetActive(false);
        }

        protected void updateSaveName(string value)
        {
            this.B_CreateNewSave.interactable = !string.IsNullOrEmpty(value);
            this.SaveName = value;
        }

        public void CreateNewSave()
        {
            this.B_CreateNewSave.interactable = false;
            var gamedatacontainer = GameManager.MakeBaseGame(this.SaveName);

            MenuManager.Instance.SelectSave(gamedatacontainer);
        }
    }
}