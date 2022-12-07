using Jemkont.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace Jemkont.Spells.Alterations {
    public class StunAlteration : Alteration {
        public StunAlteration(int Cooldown) : base(Cooldown) {
        }

        public override List<Type> Overridden() {
            return null;
        }

        public override List<Type> Overrides() {
            return new List<Type>() {
                typeof(SnareAlteration),
                typeof(DisarmedAlteration),
                typeof(CriticalAlteration),
                typeof(DodgeAlteration),
                typeof(CamouflageAlteration),
                typeof(ProvokeAlteration),
                typeof(EphemeralAlteration),
                typeof(ConfusionAlteration),
                typeof(ShatteredAlteration),
                typeof(DoTAlteration),
                typeof(BubbledAlteration),
                typeof(MindControlAlteration),
            };
        }
    }
}