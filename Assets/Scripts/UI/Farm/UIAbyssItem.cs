using DownBelow.Managers;
using DownBelow.Mechanics;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIAbyssItem : MonoBehaviour
    {
        public Transform RewardsHolder;
        public UIRewardItem RewardItemPrefab;

        public Image Lock;
        public Image Laurel;
        public TextMeshProUGUI Level;

        [ReadOnly]
        public List<UIRewardItem> RewardItems = new List<UIRewardItem>();
        [ReadOnly]
        public string TargetGrid;

        public void Init(AbyssPreset preset)
        {
            this.TargetGrid = preset.TargetGrid;

            if(preset.GiftedCards != null)
            {
                this.RewardItems.Add(Instantiate(this.RewardItemPrefab, this.RewardsHolder));
                this.RewardItems[^1].Init(preset.IsCleared, preset.GiftedCards.Count.ToString(), SettingsManager.Instance.GameUIPreset.CardSmall);
            }

            if (preset.RefillResources)
            {
                this.RewardItems.Add(Instantiate(this.RewardItemPrefab, this.RewardsHolder));
                this.RewardItems[^1].Init(preset.IsCleared, "max", SettingsManager.Instance.GameUIPreset.ResourcesEnergy);
            }

            this.RewardItems.Add(Instantiate(this.RewardItemPrefab, this.RewardsHolder));
            this.RewardItems[^1].Init(preset.IsCleared, ("+" + preset.MaxResourcesUpgrade), SettingsManager.Instance.GameUIPreset.ResourcesEnergy);
        }

        public void OnClickLevel()
        {
            UIManager.Instance.AbyssesSection.gameObject.SetActive(false);
            GameManager.RealSelfPlayer.TeleportToGrid(this.TargetGrid);
        }
    }
}