using DownBelow.Entity;
using DownBelow.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells.Alterations {
    public class EphemeralAlteration : Alteration {
        public EphemeralAlteration(int Cooldown) : base(Cooldown) {
        }
        public override void DecrementAlterationCountdown(EventData data) {
            base.DecrementAlterationCountdown(data);
            Target.Die();
        }
        public override EAlterationType ToEnum() {
            return EAlterationType.Ephemeral;
        }
    }
}