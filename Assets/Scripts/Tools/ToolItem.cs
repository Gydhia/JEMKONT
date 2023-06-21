using DownBelow;
using DownBelow.Entity;
using DownBelow.Managers;
using DownBelow.Mechanics;
using Photon.Pun.UtilityScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

[System.Serializable]
    [CreateAssetMenu(fileName = "Tool", menuName = "DownBelow/Interactables/ToolItem", order = 1)]
public class ToolItem : ItemPreset
{
    public DeckPreset DeckPreset;
    public EClass Class;
    public PlayerBehavior ActualPlayer;
    public Color ToolRefColor;
    public Sprite FightIcon;
    public string GatherAnim;

    public List<EnchantPreset> ToolEnchants;
    public int CurrentLevel = 0;

    public Dictionary<EntityStatistics, int> CurrentEnchantBuffs = new Dictionary<EntityStatistics, int>();

    public Texture2D CharacterTexture;

    public virtual void WorldAction() 
    {
        switch (Class) {
            //Fuck every each one of you, fuck polymorphism, fuck joe biden, fuck macron, fuck putin, embrace monkey
            case EClass.Fisherman:
                break;
            case EClass.Farmer:
                break;
            case EClass.Herbalist:
                break;
            case EClass.Miner:
                break;
        }
    }
   
    public List<EntityStatistics> GetEnchantedStats()
    {
        var statsList = new List<EntityStatistics>();

        foreach (var enchant in this.ToolEnchants)
        {
            foreach (var stat in enchant.Buffs)
            {
                // We only consider a stat used if it's above 1
                if(stat.Value > 0 && !statsList.Contains(stat.Key))
                {
                    statsList.Add(stat.Key);
                }
            }
        }

        if (statsList.Contains(EntityStatistics.None))
        {
            statsList.Remove(EntityStatistics.None);
        }

        return statsList;
    }

    public int GetStatsSum(EntityStatistics statistic, int level)
    {
        int statSum = 0;
        for (int i = 0; i < level; i++)
        {
            if (this.ToolEnchants[i].Buffs.ContainsKey(statistic))
            {
                statSum += this.ToolEnchants[i].Buffs[statistic];
            }
        }

        return statSum;
    }

    public int GetStatAtUpperLevel(EntityStatistics stat)
    {
        // The first enchant level of enchantement is the index [0]
        if (this.CurrentLevel >= this.ToolEnchants.Count || !this.ToolEnchants[this.CurrentLevel].Buffs.ContainsKey(stat))
            return 0;

        return this.ToolEnchants[this.CurrentLevel].Buffs[stat];
    }

    public void UpgradeLevel(bool fromInit = false)
    {
        this.CurrentLevel++;

        // -1 to not be out of range since the level isn't accuracyly linked
        var enchantPreset = this.ToolEnchants[CurrentLevel - 1];

        if (!fromInit)
        {
            NetworkManager.Instance.RPC_RespondGiftOrRemovePlayerItem(GameManager.RealSelfPlayer.UID, enchantPreset.CostItem.UID.ToString(), -enchantPreset.Cost);
        }

        foreach (var stat in enchantPreset.Buffs)
        {
            if (!this.CurrentEnchantBuffs.ContainsKey(stat.Key))
                this.CurrentEnchantBuffs.Add(stat.Key, this.GetStatsSum(stat.Key, this.CurrentLevel));
            else
                this.CurrentEnchantBuffs[stat.Key] = this.GetStatsSum(stat.Key, this.CurrentLevel);
        }
    }


    public void Reset()
    {
        this.CurrentLevel = 0;
        this.ActualPlayer = null;
        this.CurrentEnchantBuffs.Clear();
    }

    public void SetData(ToolData data)
    {
        this.DeckPreset.Deck.Cards.Clear();
        foreach (var cardID in data.DeckCards)
        {
            this.DeckPreset.Deck.Cards.Add(DownBelow.Managers.SettingsManager.Instance.ScriptableCards[cardID]);
        }

        for (int i = 0; i < data.EnchantLevel; i++)
        {
            this.UpgradeLevel(true);
        }
    }

    public ToolData GetData()
    {
        return new ToolData(this);
    }
}
public enum EClass {
    Miner, Herbalist, Farmer, Fisherman
}

[Serializable]
public struct ToolData
{
    [DataMember]
    public Guid UID { get; set; }
    [DataMember]
    public int EnchantLevel { get; set; }
    [DataMember]
    public Guid[] DeckCards { get; set; }


    public ToolData(ToolItem item)
    {
        this.UID = item.UID;
        this.EnchantLevel = item.CurrentLevel;
        this.DeckCards = item.DeckPreset.Deck.Cards.Select(c => c.UID).ToArray();
    }
}
