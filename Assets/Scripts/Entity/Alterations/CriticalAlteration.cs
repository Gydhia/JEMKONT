using Jemkont.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class CriticalAlteration : Alteration {
        public override bool ClassicCountdown { get => false; }
        public CriticalAlteration(int Cooldown) : base(Cooldown) {
        }
        public override void Setup(CharacterEntity entity) {
            base.Setup(entity);
        }
    }
}