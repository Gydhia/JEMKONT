using Jemkont.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Spells.Alterations {
    public class DoTAlteration : Alteration {
        public int Damage;
        public DoTAlteration(int Cooldown,int Damage) : base(Cooldown) {
            this.Cooldown = Cooldown;
            this.Damage = Damage;
        }
        public override void Apply(CharacterEntity entity) {
            entity.ApplyHealth(-Damage,false);
        }

        public override List<Type> Overridden() {
            return null;
        }

        public override List<Type> Overrides() {
            return null;
        }
        //DONE!
    }
}