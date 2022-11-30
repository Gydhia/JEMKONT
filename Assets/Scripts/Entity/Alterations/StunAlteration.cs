using Jemkont.Entity;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public class StunAlteration : Alteration
{
    public StunAlteration(int Cooldown) : base(Cooldown) {
    }

    public override void Apply(CharacterEntity entity) {
        //None
    }

    public override void Setup(CharacterEntity entity) {
        base.Setup(entity);
    }

    public override void WearsOff(CharacterEntity entity) {
    }
}
