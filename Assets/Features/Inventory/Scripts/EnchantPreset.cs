using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DownBelow.Mechanics
{
    [CreateAssetMenu(fileName = "Enchant Preset", menuName = "DownBelow/Inventory/Enchant Preset", order = 1)]

    public class EnchantPreset : SerializedScriptableObject
    {
        public ItemPreset CostItem;
        public int Cost;

        public Dictionary<EntityStatistics, int> Buffs;


        [Button]
        public void CreateAllEnums()
        {
            if(this.Buffs == null)
            {
                this.Buffs = new Dictionary<EntityStatistics, int>();
            }

            List<EntityStatistics> NonUpgradableStats = 
                new List<EntityStatistics>() { 
                    EntityStatistics.None, 
                    EntityStatistics.Mana 
                };

            foreach (EntityStatistics buff in Enum.GetValues(typeof(EntityStatistics)))
            {
                if (!this.Buffs.ContainsKey(buff) && !NonUpgradableStats.Contains(buff))
                {
                    this.Buffs.Add(buff, 0);
                }
            }
        }
    }
}