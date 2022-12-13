using Jemkont.Entity;
using Jemkont.Events;
using MyBox;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
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
        Sleep,
        Bubbled,
        SpeedUpDown,
        Inspiration,
        DmgUpDown,
        MindControl,
    }
    public abstract class Alteration {
        /// <summary>
        /// Key is overriden by value.
        /// </summary>
        public static Dictionary<EAlterationType,Type[]> overrides = new() {
            { EAlterationType.Stun, new Type[0] },
            { EAlterationType.Snare, new Type[2]{ typeof(StunAlteration), typeof(SleepAlteration) } },
            { EAlterationType.Disarmed, new Type[2]{ typeof(StunAlteration), typeof(SleepAlteration) } },
            { EAlterationType.Critical, new Type[0] },
            { EAlterationType.Dodge, new Type[0] },
            { EAlterationType.Camouflage, new Type[2]{ typeof(StunAlteration), typeof(SleepAlteration) } },
            { EAlterationType.Provoke, new Type[0]},
            { EAlterationType.Ephemeral, new Type[0] },
            { EAlterationType.Confusion, new Type[2]{ typeof(StunAlteration), typeof(SleepAlteration) } },
            { EAlterationType.Shattered, new Type[2]{ typeof(StunAlteration), typeof(SleepAlteration) } },
            { EAlterationType.DoT, new Type[2]{ typeof(StunAlteration), typeof(SleepAlteration) } },
            { EAlterationType.Bubbled, new Type[2]{ typeof(StunAlteration), typeof(SleepAlteration) } },
            { EAlterationType.MindControl, new Type[2]{ typeof(StunAlteration), typeof(SleepAlteration) } },
            { EAlterationType.Inspiration, new Type[2]{ typeof(StunAlteration), typeof(SleepAlteration) } },
            { EAlterationType.DmgUpDown, new Type[2]{ typeof(StunAlteration), typeof(SleepAlteration) } },
            { EAlterationType.SpeedUpDown, new Type[2]{ typeof(StunAlteration), typeof(SleepAlteration) } },
            { EAlterationType.Sleep, new Type[2]{ typeof(StunAlteration), typeof(SleepAlteration) } },
        };
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
                InspirationAlteration => EAlterationType.Inspiration,
                DmgUpDownAlteration => EAlterationType.DmgUpDown,
                SpeedUpDownAlteration => EAlterationType.SpeedUpDown,

                _ => throw new System.NotImplementedException($"NEED TO IMPLEMENT ALTERATION CALLED {type}"),
            };
        }
        public abstract EAlterationType ToEnum();
        public int Cooldown;
        public CharacterEntity Target;
        public virtual bool ClassicCountdown { get => true; }
        public virtual void Setup(CharacterEntity entity) {
            Target = entity;
        }
        public virtual void Apply(CharacterEntity entity) {
        }

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
