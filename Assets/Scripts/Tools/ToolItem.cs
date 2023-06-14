using DownBelow;
using DownBelow.Entity;
using DownBelow.Mechanics;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
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
}
public enum EClass {
    Miner, Herbalist, Farmer, Fisherman
}
