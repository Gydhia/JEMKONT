using DownBelow.Managers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

namespace DownBelow.UI
{
    public class UIEnchantItem : MonoBehaviour
    {
        private ToolItem _refTool;

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
            this._refTool = refTool;

            this.ToolName.text = refTool.ItemName;
            this.ToolImage.sprite = refTool.InventoryIcon;

            int level = refTool.CurrentLevel;

            var upgradableStats = refTool.GetEnchantedStats();

            foreach (var buff in upgradableStats)
            {
                this.EnchantRaws.Add(Instantiate(this.EnchantRawPrefab, this.EnchantRawsHolder));
                this.EnchantRaws[^1].Refresh(buff, refTool.GetStatsSum(buff, level), refTool.GetStatAtUpperLevel(buff));
            }

            this.RefreshCanCraft();
        }

        public void RefreshCanCraft()
        {
            int level = this._refTool.CurrentLevel;

            bool hasResources = GameManager.RealSelfPlayer.PlayerInventory.HasResources(this._refTool.ToolEnchants[level].CostItem, this._refTool.ToolEnchants[level].Cost);

            Level.text = level.ToString() + " / " + this._refTool.ToolEnchants.Count.ToString();
            Cost.text = this._refTool.ToolEnchants[level].Cost.ToString() + " Herbs";
            Cost.color = hasResources ? Color.green : Color.red;

            this.UpgradeButton.interactable = hasResources;
        }

        public void OnClickUpgrade()
        {
        }
    }
}