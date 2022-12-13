using Jemkont.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class CriticalAlteration : Alteration {
        public override bool ClassicCountdown { get => false; }
        public CriticalAlteration(int Cooldown) : base(Cooldown) {
        }

        public override List<Type> Overrides() {
            return null;
        }

        public override List<Type> Overridden() {
            return new List<Type>() {
            typeof(DisarmedAlteration)
            };
        }
    }
}