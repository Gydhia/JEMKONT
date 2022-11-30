using Jemkont.Entity;
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
        Spirit,
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
                SpiritAlteration => EAlterationType.Spirit,
                BubbledAlteration => EAlterationType.Bubbled,
                MindControlAlteration => EAlterationType.MindControl,
                _ => EAlterationType.Stun,
            };
        }
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
}
