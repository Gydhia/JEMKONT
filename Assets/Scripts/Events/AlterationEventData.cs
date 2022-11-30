using Jemkont.Entity;
using Jemkont.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jemkont.Spells.Alterations;

public class AlterationEventData : EventData<AlterationEventData> {
    public CharacterEntity Entity;
    public Alteration Alteration;

    public AlterationEventData(CharacterEntity Entity,Alteration alt) {
        this.Entity = Entity;
        this.Alteration = alt;
    }
}
