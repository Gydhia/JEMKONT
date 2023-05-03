using DownBelow.Entity;
using DownBelow.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells.Alterations {
    public class BubbledAlteration : Alteration {
        public BubbledAlteration(int Cooldown) : base(Cooldown) {
        }
        public override void DecrementAlterationCountdown(EventData data) {
            base.DecrementAlterationCountdown(data);
            if(Duration <= 0) {
                Target.OnHealthRemoved -= DecrementAlterationCountdown;
            }
        }

        public override bool ClassicCountdown => false;
    }
}