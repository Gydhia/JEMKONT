using Jemkont.Entity;
using Jemkont.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class EphemeralAlteration : Alteration {
        public EphemeralAlteration(int Cooldown) : base(Cooldown) {
        }
        public override void DecrementAlterationCountdown(EventData data) {
            base.DecrementAlterationCountdown(data);
            //TODO: DIE here lol
        }

        public override List<Type> Overridden() {
            return null;
        }

        public override List<Type> Overrides() {
            return null;
        }
    }
}