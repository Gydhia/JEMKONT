using Jemkont.Entity;
using Jemkont.Events;
using MyBox;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace Jemkont.Spells.Alterations {

    public enum EAlterationType {
        Stun,
        Snare,
        Disarmed,
        Critical,
        Dodge,
        Camouflage,
        Provoke,
        Ephemeral,
        Confusion,
        Shattered,
        DoT,
        Bubbled,
        MindControl
    }
    public abstract class Alteration {
        public static EAlterationType TypeToEnum<T>() {
            T type = default;
            return type switch {
                StunAlteration => EAlterationType.Stun,
                SnareAlteration => EAlterationType.Snare,
                DisarmedAlteration => EAlterationType.Disarmed,
                CriticalAlteration => EAlterationType.Critical,
                DodgeAlteration => EAlterationType.Dodge,
                CamouflageAlteration => EAlterationType.Camouflage,
                ProvokeAlteration => EAlterationType.Provoke,
                EphemeralAlteration => EAlterationType.Ephemeral,
                ConfusionAlteration => EAlterationType.Confusion,
                ShatteredAlteration => EAlterationType.Shattered,
                DoTAlteration => EAlterationType.DoT,
                BubbledAlteration => EAlterationType.Bubbled,
                MindControlAlteration => EAlterationType.MindControl,
                _ => EAlterationType.Stun, //I like the taste of RISK
            };
        }
        public int Cooldown;
        public CharacterEntity Target;
        public virtual bool ClassicCountdown { get => true; }
        public virtual void Setup(CharacterEntity entity) {
            Target = entity;
        }
        public virtual void Apply(CharacterEntity entity) {
        }
        public abstract List<Type> Overrides();
        public abstract List<Type> Overridden();
        public virtual void WearsOff(CharacterEntity entity) { }
        public virtual void DecrementAlterationCountdown(Events.EventData data) {
            
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
}
