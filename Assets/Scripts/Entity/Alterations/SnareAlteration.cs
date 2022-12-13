using Jemkont.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class SnareAlteration : Alteration {
        public SnareAlteration(int Cooldown) : base(Cooldown) {
        }

        public override EAlterationType ToEnum() {
            return EAlterationType.Snare;
        }
    }
}