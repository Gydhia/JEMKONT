using DownBelow.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells.Alterations {
    public class DoTAlteration : Alteration {
        public int Damage;
        public DoTAlteration(int Cooldown,int Damage) : base(Cooldown) {
            this.Cooldown = Cooldown;
            this.Damage = Damage;
        }
        public override void Apply(CharacterEntity entity) {
            entity.ApplyStat(EntityStatistics.Health,-Damage,false);
        }
        public override EAlterationType ToEnum() {
            return EAlterationType.DoT;
        }
    }
}