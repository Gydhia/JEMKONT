using DownBelow.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells.Alterations {
    public class DisarmedAlteration : Alteration {
        public DisarmedAlteration(int Cooldown) : base(Cooldown) {
            //TODO : disable autoattacks (when we've coded them)
        }
        public override EAlterationType ToEnum() {
            return EAlterationType.Disarmed;
        }
    }
}