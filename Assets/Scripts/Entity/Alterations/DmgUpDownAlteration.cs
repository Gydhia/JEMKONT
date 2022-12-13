using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DmgUpDownAlteration : BuffAlteration
{
    public DmgUpDownAlteration(int Cooldown,int value) : base(Cooldown,value) {
    }
    public override EntityStatistics StatToBuff() {
        return EntityStatistics.Strength;
    }

    
}
