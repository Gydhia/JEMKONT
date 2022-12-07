using Jemkont.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class SnareAlteration : Alteration {
        public SnareAlteration(int Cooldown) : base(Cooldown) {
        }

        public override List<Type> Overridden() {
            return new List<Type>() {
                typeof(StunAlteration)
            };
        }

        public override List<Type> Overrides() {
            return null;
            //TODO: Add Slows
        }
    }
}