using DownBelow.Managers;
using DownBelow.Mechanics;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIEnchantItem : MonoBehaviour
    {
        public UIEnchantRaw EnchantRawPrefab;
        public Transform EnchantRawsHolder;

        public List<UIEnchantRaw> EnchantRaws = new List<UIEnchantRaw>();

        public Button UpgradeButton;
        public TextMeshProUGUI ToolName;
        public Image ToolImage;
        public TextMeshProUGUI Cost;
        public TextMeshProUGUI Level;

        public void Init(ToolItem refTool)
        {
            this.ToolName.text = refTool.ItemName;
            this.ToolImage.sprite = refTool.InventoryIcon;

            int level = refTool.CurrentLevel;

            var upgradableStats = refTool.GetEnchantedStats();

            foreach (var buff in upgradableStats)
            {
                this.EnchantRaws.Add(Instantiate(this.EnchantRawPrefab, this.EnchantRawsHolder));
                this.EnchantRaws[^1].Refresh(buff, refTool.GetStatsSum(buff, level), refTool.GetStatAtUpperLevel(buff));
            }
            bool hasResources = GameManager.RealSelfPlayer.PlayerInventory.HasResources(refTool.ToolEnchants[level].CostItem, refTool.ToolEnchants[level].Cost);

            Level.text = level.ToString() + " / " + refTool.ToolEnchants.Count.ToString();
            Cost.text = refTool.ToolEnchants[level].Cost.ToString() + " Herbs";
            Cost.color = hasResources ? Color.green : Color.red;

            this.UpgradeButton.interactable = hasResources;


        }

        public void RefreshCanCraft()
        {
            //bool hasAllResources = true;
            //foreach (var item in this.EnchantItems)
            //{
            //    bool hasResource = GameManager.RealSelfPlayer.PlayerInventory.HasResources(item.Preset, item.Quantity);
            //    item.AmountText.color = hasResource ? Color.white : Color.red;

            //    hasAllResources &= hasResource;
            //}

            //this.UpgradeButton.interactable = hasAllResources;
        }

        public void OnClickUpgrade()
        {

        }
    }
}