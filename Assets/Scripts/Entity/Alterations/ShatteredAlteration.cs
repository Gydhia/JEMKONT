using Jemkont.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class ShatteredAlteration : Alteration {
        public ShatteredAlteration(int Cooldown) : base(Cooldown) {

        }
        public override bool ClassicCountdown => false;
    }
}