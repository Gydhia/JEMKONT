using DownBelow.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells.Alterations {
    public class CriticalAlteration : Alteration {
        public override bool ClassicCountdown { get => false; }
        public CriticalAlteration(int Cooldown) : base(Cooldown) {
        }
        public override EAlterationType ToEnum() {
            return EAlterationType.Critical;
        }
    }
}