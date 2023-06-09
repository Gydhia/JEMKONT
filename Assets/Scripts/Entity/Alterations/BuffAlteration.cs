using DownBelow.Entity;
using DownBelow.Spells.Alterations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffAlteration : Alteration
{
    public BuffAlteration(int Cooldown, int value, EntityStatistics statToBuff) : base(Cooldown)
    {
        this.value = value;
        this.StatToBuff = statToBuff;
    }
    /// <summary>
    /// </summary>
    /// <returns>The statistic to buff. Readonly.</returns>
    public EntityStatistics StatToBuff;
    /// <summary>
    /// </summary>
    /// <returns>The amount to buff. Can be positive or negative, readonly.</returns>
    public int value;
    public override void Setup(CharacterEntity entity)
    {
        base.Setup(entity);
        entity.ApplyStat(StatToBuff, value);
    }
    public override void WearsOff(CharacterEntity entity)
    {
        base.WearsOff(entity);
        entity.ApplyStat(StatToBuff, -value);
    }
}
