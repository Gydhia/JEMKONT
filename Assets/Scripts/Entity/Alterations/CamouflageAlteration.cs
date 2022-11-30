using Jemkont.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamouflageAlteration : Alteration {
    public CamouflageAlteration(int Cooldown) : base(Cooldown) {
    }
    public override bool ClassicCountdown => false;
}
