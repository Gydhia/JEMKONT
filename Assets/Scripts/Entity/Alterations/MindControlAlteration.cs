using Jemkont.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class MindControlAlteration : Alteration {
        public MindControlAlteration(int Cooldown) : base(Cooldown) {
            //TODO: MVP+1
        }
        public override EAlterationType ToEnum() {
            return EAlterationType.MindControl;
        }

    }
}