using Jemkont.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgeAlteration : Alteration {
    public DodgeAlteration(int Cooldown) : base(Cooldown) {
    }
    public override bool ClassicCountdown => false;
}
