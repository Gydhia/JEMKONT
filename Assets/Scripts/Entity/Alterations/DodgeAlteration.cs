using DownBelow.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells.Alterations {
    public class DodgeAlteration : Alteration {
        public DodgeAlteration(int Cooldown) : base(Cooldown) {
        }
        public override bool ClassicCountdown => false;
        public override EAlterationType ToEnum() {
            return EAlterationType.Dodge;
        }
    }
}