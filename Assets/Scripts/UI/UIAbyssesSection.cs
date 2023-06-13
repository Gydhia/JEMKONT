using DownBelow.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.UI
{
    public class UIAbyssesSection : MonoBehaviour
    {
        public Transform AbyssItemsHolder;
        public UIAbyssItem AbyssItemPrefab;
        public GameObject SeparatorPrefab;

        public List<UIAbyssItem> AbyssItems = new List<UIAbyssItem>();

        public void Init()
        {
            foreach (var aPreset in SettingsManager.Instance.AbyssesPresets)
            {
                this.AbyssItems.Add(Instantiate(this.AbyssItemPrefab, this.AbyssItemsHolder));
                this.AbyssItems[^1].Init(aPreset);

                Instantiate(this.SeparatorPrefab, this.AbyssItemsHolder);
            }
        }

        public void OpenPanel()
        {
            this.gameObject.SetActive(true);
        }

        public void OnClickClose()
        {
            this.gameObject.SetActive(false);
        }
    }
}