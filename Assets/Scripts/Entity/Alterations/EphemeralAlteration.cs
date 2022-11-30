using Jemkont.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EphemeralAlteration : Alteration {
    public EphemeralAlteration(int Cooldown) : base(Cooldown) {
    }
    public override void DecrementAlterationCountdown(GameEventData data) {
        base.DecrementAlterationCountdown(data);
        //TODO: DIE here lol
    }
}
