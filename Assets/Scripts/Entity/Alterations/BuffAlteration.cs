using Jemkont.Entity;
using Jemkont.Spells.Alterations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuffAlteration : Alteration
{
    public BuffAlteration(int Cooldown, int value) : base(Cooldown) {
        this.value = value;
    }
    /// <summary>
    /// </summary>
    /// <returns>The statistic to buff. Readonly.</returns>
    public abstract EntityStatistics StatToBuff();
    /// <summary>
    /// </summary>
    /// <returns>The amount to buff. Can be positive or negative, readonly.</returns>
    public int value;
    public override void Setup(CharacterEntity entity) {
        base.Setup(entity);
        entity.ApplyStat(StatToBuff(),value);
    }
    public override void WearsOff(CharacterEntity entity) {
        base.WearsOff(entity);
        entity.ApplyStat(StatToBuff(),-value);
    }
}
