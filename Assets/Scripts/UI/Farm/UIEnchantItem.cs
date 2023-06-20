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

            this.RefreshStats();
            this.RefreshCanCraft();
        }

        public void RefreshStats()
        {
            this.ToolName.text = _refTool.ItemName;
            this.ToolImage.sprite = _refTool.InventoryIcon;

            int level = _refTool.CurrentLevel;

            var upgradableStats = _refTool.GetEnchantedStats();

            for (int i = 0; i < upgradableStats.Count; i++)
            {
                if(EnchantRaws.Count <= i)
                {
                    this.EnchantRaws.Add(Instantiate(this.EnchantRawPrefab, this.EnchantRawsHolder));
                }
                this.EnchantRaws[i].Refresh(upgradableStats[i], _refTool.GetStatsSum(upgradableStats[i], level), _refTool.GetStatAtUpperLevel(upgradableStats[i]));
            }
        }

        public void RefreshCanCraft()
        {
            int level = this._refTool.CurrentLevel;

            bool maxLevel = level >= this._refTool.ToolEnchants.Count;

            bool hasResources = maxLevel ?
                false :
                GameManager.RealSelfPlayer.PlayerInventory.HasResources(this._refTool.ToolEnchants[level].CostItem, this._refTool.ToolEnchants[level].Cost);

            Level.text = level.ToString() + " / " + this._refTool.ToolEnchants.Count.ToString();
            Cost.text = maxLevel ? "MAXED" : this._refTool.ToolEnchants[level].Cost.ToString() + " Herbs";
            Cost.color = hasResources ? Color.green : Color.red;

            this.UpgradeButton.interactable = hasResources;
        }

        public void OnClickUpgrade()
        {
            this._refTool.UpgradeLevel();

            this.RefreshCanCraft();
            this.RefreshStats();
        }
    }
}