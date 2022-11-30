using Jemkont.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnareAlteration : Alteration {
    public SnareAlteration(int Cooldown) : base(Cooldown) {
    }

    public override void Apply(CharacterEntity entity) {
    }

    public override void Setup(CharacterEntity entity) {
        base.Setup(entity);
    }

    public override void WearsOff(CharacterEntity entity) {
    }
}
