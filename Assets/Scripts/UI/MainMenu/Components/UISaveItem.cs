using DownBelow.GameData;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI.Menu
{
    public class UISaveItem : MonoBehaviour
    {
        public TextMeshProUGUI SaveVersion;
        public TextMeshProUGUI SaveName;
        public TextMeshProUGUI SaveDate;

        public Button SelfButton;

        protected FileInfo _refFile = null;
        private GameDataContainer _savegame = null;

        private bool _isLoading = false;
        private bool _isLoaded = false;

        public void Update()
        {
            if (this._refFile != null && this._refFile.Exists && this._savegame == null)
            {
                if (this._isLoading || this._isLoaded)
                {
                    return;
                }
                else
                {
                    this._isLoading = true;
                    try
                    {
                        TaskScheduler mainThread = TaskScheduler.FromCurrentSynchronizationContext();
                        Task.Run(() => GameDataContainer.QuickLoad(new System.IO.FileInfo(this._refFile.FullName))).ContinueWith((previousTask) =>
                        {
                            try
                            {
                                bool ok = true;
                                Exception ex = previousTask.Exception;
                                while (previousTask.Exception is AggregateException && ex.InnerException != null)
                                {
                                    ok = false;
                                    ex = ex.InnerException;
                                    Debug.LogError(ex, this);
                                }

                                if (ok)
                                {
                                    this._savegame = previousTask.Result;
                                    if (this._savegame == null)
                                        throw new Exception("Gamefile " + this._refFile.FullName + " is empty");
                                    this.DisplayAfterLoad();
                                }
                            }
                            catch (Exception ex)
                            {
                                 Debug.LogError(ex, this);
                            }
                        }, mainThread);
                        
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex, this);
                    }
                }
            }
        }

        public void Init(FileInfo refFile)
        {
            this._refFile = refFile;
            this.SelfButton.interactable = false;
            this.SelfButton.onClick.AddListener(() => OnClickSave());
        }

        public void DisplayAfterLoad()
        {
            this.SelfButton.interactable = true;

            this.SaveDate.text = _savegame.Data.save_time.ToString();
            this.SaveName.text = _savegame.Data.save_name;
            this.SaveVersion.text = _savegame.Data.game_version;
        }

        public void OnClickDelete()
        {
            // TODO : remove the save from files
        }

        public void OnClickSave()
        {
            MenuManager.Instance.SelectSave(this._savegame);
        }
    }

}