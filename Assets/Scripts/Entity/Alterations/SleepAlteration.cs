using DownBelow.Spells.Alterations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepAlteration : Alteration {
    public SleepAlteration(int Cooldown) : base(Cooldown) {
    }
    public override bool ClassicCountdown => false;
    public override EAlterationType ToEnum() {
        return EAlterationType.Sleep;
    }
}
