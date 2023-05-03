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
        public TMP_InputField SaveNameInput;

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

            B_AskNewSave.onClick.AddListener(() => this.HideAskNewSave());

        }

        public override void HidePopup()
        {
            this.B_AskNewSave.gameObject.SetActive(true);

            base.HidePopup();
        }

        protected void HideAskNewSave()
        {
            this.B_AskNewSave.gameObject.SetActive(false);
        }

        public void CreateNewSave()
        {
            var gamedatacontainer = GameManager.MakeBaseGame(this.SaveNameInput.text);

            MenuManager.Instance.SelectSave(gamedatacontainer);
        }
    }
}