using DownBelow.Entity;
using DownBelow.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells.Alterations {
    public class ShatteredAlteration : Alteration {
        public ShatteredAlteration(int Cooldown) : base(Cooldown) {

        }
        public override void DecrementAlterationCountdown(EventData data) {
            if (data is SpellEventData spelldata && spelldata.Value <= 0) {
                return;
            }
            base.DecrementAlterationCountdown(data);
        }
        public override bool ClassicCountdown => false;

    }
}