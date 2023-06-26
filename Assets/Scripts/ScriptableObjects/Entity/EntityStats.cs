using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DownBelow/Entity/Statistics")]
public class EntityStats : SerializedScriptableObject
{
    public int Health => this.BaseHealth + this.BuffedHealth;
    public int Shield => this.BaseShield + this.BuffedShield;

    public int Strength => this.BaseStrength + this.BuffedStrength;
     
    public int Mana => this.BaseMana + this.BuffedMana;
    public int Speed => this.BaseSpeed + this.BuffedSpeed;

    public int Defense => this.BaseDefense + this.BuffedDefense;
    public int Range => this.BaseRange + this.BuffedRange;

    public int BaseHealth;
    public int BaseShield;

    public int BaseStrength;

    public int BaseMana;
    public int BaseSpeed;

    public int BaseDefense;
    public int BaseRange;


    [ReadOnly]
    public int BuffedHealth;
    [ReadOnly]
    public int BuffedShield;

    [ReadOnly]
    public int BuffedStrength;

    [ReadOnly]
    public int BuffedMana;
    [ReadOnly]
    public int BuffedSpeed;

    [ReadOnly]
    public int BuffedDefense;
    [ReadOnly]
    public int BuffedRange;

    public int GetStatistic(EntityStatistics stat)
    {
        switch (stat)
        {
            case EntityStatistics.Health:
                return this.Health;
            case EntityStatistics.Mana:
                return this.Mana;
            case EntityStatistics.Speed:
                return this.Speed;
            case EntityStatistics.Strength:
                return this.Strength;
            case EntityStatistics.Defense:
                return this.Defense;
            case EntityStatistics.Range:
                return this.Range;
        }

        return -1;
    }


    public void UpdateBuffed(ToolItem RefTool)
    {
        foreach (var buff in RefTool.CurrentEnchantBuffs)
        {
            switch (buff.Key)
            {
                case EntityStatistics.Health:
                    this.BuffedHealth = buff.Value;
                    break;
                case EntityStatistics.Mana:
                    this.BuffedMana = buff.Value;
                    break;
                case EntityStatistics.Speed:
                    this.BuffedSpeed = buff.Value;
                    break;
                case EntityStatistics.Strength:
                    this.BuffedStrength = buff.Value;
                    break;
                case EntityStatistics.Defense:
                    this.BuffedDefense = buff.Value;
                    break;
                case EntityStatistics.Range:
                    this.BuffedRange = buff.Value;
                    break;
                default:
                    break;
            }
        }
    }
}
