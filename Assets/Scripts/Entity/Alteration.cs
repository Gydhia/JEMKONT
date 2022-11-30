using Jemkont.Entity;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Alteration
{
    //SHOW
    public int Cooldown;
    public CharacterEntity Target;
    public virtual bool ClassicCountdown { get => true; }
    public virtual void Setup(CharacterEntity entity) {
        Target = entity;
    }
    public virtual void Apply(CharacterEntity entity) {
    }
    public virtual void WearsOff(CharacterEntity entity) { }
    public virtual void DecrementAlterationCountdown(GameEventData data) {
        Cooldown--;
        if (Cooldown <= 0) {
            Target.Alterations.Remove(this);
            WearsOff(Target);
            Target.OnTurnEnded -= DecrementAlterationCountdown; //TODO: call this when you die.
        }
    }
    public Alteration(int Cooldown) {
        this.Cooldown = Cooldown;
    }
}
