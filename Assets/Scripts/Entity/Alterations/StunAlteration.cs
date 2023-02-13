using DownBelow.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace DownBelow.Spells.Alterations {
    public class StunAlteration : Alteration {
        public StunAlteration(int Cooldown) : base(Cooldown) {
        }

        public override EAlterationType ToEnum() {
            return EAlterationType.Stun;
        }
    }
}