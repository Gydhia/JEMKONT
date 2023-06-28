using DownBelow.Managers;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace DownBelow.UI
{
    public class UIAbyssesSection : MonoBehaviour
    {
        public Transform AbyssItemsHolder;
        public UIAbyssItem AbyssItemPrefab;
        public GameObject SeparatorPrefab;

        public List<UIAbyssItem> AbyssItems = new List<UIAbyssItem>();
        public List<GameObject> Separators = new List<GameObject>();

        public void Init()
        {
            int counter = 1;
            foreach (var aPreset in SettingsManager.Instance.AbyssesPresets)
            {
                this.AbyssItems.Add(Instantiate(this.AbyssItemPrefab, this.AbyssItemsHolder));
                this.AbyssItems[^1].Init(aPreset, counter);

                this.Separators.Add(Instantiate(this.SeparatorPrefab, this.AbyssItemsHolder));
                counter++;
            }

            
        }

        public void OpenPanel()
        {
            foreach (var abyss in this.AbyssItems)
            {
                Destroy(abyss.gameObject);
            }
            this.AbyssItems.Clear();

            foreach (var sep in this.Separators)
            {
                Destroy(sep.gameObject);
            }
            this.Separators.Clear();

            this.Init();
            
            this.gameObject.SetActive(true);
        }

        public void OnClickClose()
        {
            this.gameObject.SetActive(false);
        }
    }
}