using Jemkont.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class CamouflageAlteration : Alteration {
        public CamouflageAlteration(int Cooldown) : base(Cooldown) {
        }
        public override bool ClassicCountdown => false;
    }
}