using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUpDownAlteration : BuffAlteration
{
    public SpeedUpDownAlteration(int Cooldown,int value) : base(Cooldown,value) {
    }

    public override EntityStatistics StatToBuff() {
        return EntityStatistics.Speed;
    }

    
}
