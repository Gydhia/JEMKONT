using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.Managers;
using MyBox;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
namespace DownBelow.Spells.Alterations {

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
        public static Dictionary<EAlterationType,EAlterationType[]> overrides = new() {
            { EAlterationType.Stun, new EAlterationType[0] },
            { EAlterationType.Snare, new EAlterationType[0] },
            { EAlterationType.Disarmed, new EAlterationType[0] },
            { EAlterationType.Critical, new EAlterationType[0] },
            { EAlterationType.Dodge, new EAlterationType[0] },
            { EAlterationType.Camouflage, new EAlterationType[0] },
            { EAlterationType.Provoke, new EAlterationType[0]},
            { EAlterationType.Ephemeral, new EAlterationType[0] },
            { EAlterationType.Confusion, new EAlterationType[0] },
            { EAlterationType.Shattered, new EAlterationType[0] },
            { EAlterationType.DoT, new EAlterationType[0] },
            { EAlterationType.Bubbled, new EAlterationType[0] },
            { EAlterationType.MindControl, new EAlterationType[0] },
            { EAlterationType.Inspiration, new EAlterationType[0] },
            { EAlterationType.DmgUpDown, new EAlterationType[0] },
            { EAlterationType.SpeedUpDown, new EAlterationType[0] },
            { EAlterationType.Sleep, new EAlterationType[0] },
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

        [ReadOnly] public Animator InstanciatedFXAnimator;

        public virtual bool ClassicCountdown { get => true; }

        public virtual void Setup(CharacterEntity entity) {
            Target = entity;
            //SetupFx?
            SFXManager.Instance.RefreshAlterationSFX(entity);
        }

        public virtual void Apply(CharacterEntity entity) {
            SFXManager.Instance.RefreshAlterationSFX(entity);
        }

        public virtual void WearsOff(CharacterEntity entity) {
            //FxGoAway?
            SFXManager.Instance.RefreshAlterationSFX(entity);
        }

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

        public override string ToString() {
            string cc = ClassicCountdown ? "\n(Can also decrement with other conditions)" : "";
            return $"{ToEnum()} for {Cooldown} turns.{cc}";
        }
    }
}
