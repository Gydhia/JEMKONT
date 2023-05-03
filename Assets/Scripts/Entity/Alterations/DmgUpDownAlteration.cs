using DownBelow.Spells.Alterations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class DmgUpDownAlteration : BuffAlteration
{
    public DmgUpDownAlteration(int Cooldown,int value) : base(Cooldown,value) {
    }
    public override EntityStatistics StatToBuff() {
        return EntityStatistics.Strength;
    }
}
