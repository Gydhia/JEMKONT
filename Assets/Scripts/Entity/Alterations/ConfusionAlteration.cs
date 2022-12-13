using Jemkont.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class ConfusionAlteration : Alteration {
        public ConfusionAlteration(int Cooldown) : base(Cooldown) {
        }
        public override EAlterationType ToEnum() {
            return EAlterationType.Confusion;
        }
        //DONE!
        //TODO: when enemies can attack, make them. Done?
    }
}
