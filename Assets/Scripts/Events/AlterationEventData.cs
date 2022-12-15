using DownBelow.Entity;
using DownBelow.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DownBelow.Spells.Alterations;

public class AlterationEventData : EventData<AlterationEventData> {
    public CharacterEntity Entity;
    public Alteration Alteration;

    public AlterationEventData(CharacterEntity Entity,Alteration alt) {
        this.Entity = Entity;
        this.Alteration = alt;
    }
}
