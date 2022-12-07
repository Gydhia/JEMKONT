using Jemkont.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class DisarmedAlteration : Alteration {
        public DisarmedAlteration(int Cooldown) : base(Cooldown) {
            //TODO : disable autoattacks (when we've coded them)
        }

        public override List<Type> Overridden() {
            return new List<Type>() {
                typeof(StunAlteration)
            };
        }

        public override List<Type> Overrides() {
            return new List<Type>() {
                typeof(ProvokeAlteration)
            };
        }
    }
}