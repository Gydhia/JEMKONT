using DownBelow.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells.Alterations {
    public class InspirationAlteration : Alteration {
        public InspirationAlteration(int Cooldown) : base(Cooldown) {
        }
        public override void Setup(CharacterEntity entity) {
            base.Setup(entity);
            //TODO: inpspirèd;
        }
        public override EAlterationType ToEnum() {
            return EAlterationType.Inspiration;
        }
    }
}