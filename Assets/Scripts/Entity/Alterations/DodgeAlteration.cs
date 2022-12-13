using Jemkont.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class DodgeAlteration : Alteration {
        public DodgeAlteration(int Cooldown) : base(Cooldown) {
        }
        public override bool ClassicCountdown => false;
    }
}