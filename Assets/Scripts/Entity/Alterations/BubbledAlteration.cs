using Jemkont.Entity;
using Jemkont.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class BubbledAlteration : Alteration {
        public BubbledAlteration(int Cooldown) : base(Cooldown) {
        }
        public override void DecrementAlterationCountdown(EventData data) {
            base.DecrementAlterationCountdown(data);
            if(Cooldown <= 0) {
                Target.OnHealthRemoved -= DecrementAlterationCountdown;
            }
        }

        public override List<Type> Overrides() {
            return null;
        }

        public override List<Type> Overridden() {
            return null;
        }

        public override bool ClassicCountdown => false;
    }
}