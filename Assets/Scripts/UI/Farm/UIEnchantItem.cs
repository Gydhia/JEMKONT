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
        public ToolItem RefTool;

        public UIEnchantRaw EnchantRawPrefab;
        public Transform EnchantRawsHolder;

        public List<UIEnchantRaw> EnchantRaws = new List<UIEnchantRaw>();

        public Button UpgradeButton;
        public TextMeshProUGUI ToolName;
        public Image ToolImage;
        public Image LockImage;
        public TextMeshProUGUI Cost;
        public TextMeshProUGUI Level;

        public void Init(ToolItem refTool)
        {
            this.RefTool = refTool;

            this.RefreshStats();
            this.RefreshCanCraft();
        }

        public void RefreshStats()
        {
            this.ToolName.text = RefTool.ItemName;
            this.ToolImage.sprite = RefTool.InventoryIcon;

            int level = RefTool.CurrentLevel;

            var upgradableStats = RefTool.GetEnchantedStats();

            for (int i = 0; i < upgradableStats.Count; i++)
            {
                if(EnchantRaws.Count <= i)
                {
                    this.EnchantRaws.Add(Instantiate(this.EnchantRawPrefab, this.EnchantRawsHolder));
                }
                this.EnchantRaws[i].Refresh(upgradableStats[i], RefTool.GetStatsSum(upgradableStats[i], level), RefTool.GetStatAtUpperLevel(upgradableStats[i]));
            }
        }

        public void RefreshCanCraft()
        {
            int level = this.RefTool.CurrentLevel;

            bool maxLevel = level >= this.RefTool.ToolEnchants.Count;

            bool hasResources = maxLevel ?
                false :
                GameManager.RealSelfPlayer.PlayerInventory.HasResources(this.RefTool.ToolEnchants[level].CostItem, this.RefTool.ToolEnchants[level].Cost);

            LockImage.gameObject.SetActive(this.RefTool.ActualPlayer != GameManager.RealSelfPlayer);

            Level.text = level.ToString() + " / " + this.RefTool.ToolEnchants.Count.ToString();
            Cost.text = maxLevel ? "MAXED" : this.RefTool.ToolEnchants[level].Cost.ToString() + " Herbs";
            Cost.color = hasResources ? Color.green : Color.red;

            this.UpgradeButton.interactable = hasResources;
        }

        public void OnClickUpgrade()
        {
            NetworkManager.Instance.ApplyInteractableDurability(UIManager.Instance.EnchantSection.CurrentTable);

            this.RefTool.UpgradeLevel();

            this.RefreshCanCraft();
            this.RefreshStats();
        }
    }
}