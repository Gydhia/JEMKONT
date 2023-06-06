using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.Managers;

using Sirenix.Serialization;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
namespace DownBelow.Spells.Alterations
{

    [Serializable]
    public abstract class Alteration
    {
        /*// <summary>
        /// Key is overriden by value.
        /// </summary>
        public static Dictionary<EAlterationType,EAlterationType[]> overrides = new() {
            { EAlterationType.Stun, new EAlterationType[0] },
            { EAlterationType.Snare, new EAlterationType[0] },
            { EAlterationType.Shattered, new EAlterationType[0] },
            { EAlterationType.DoT, new EAlterationType[0] },
            { EAlterationType.Bubbled, new EAlterationType[0] },
            { EAlterationType.DmgUpDown, new EAlterationType[0] },
            { EAlterationType.SpeedUpDown, new EAlterationType[0] },
            { EAlterationType.Sleep, new EAlterationType[0] },
        };
        //*/

        public int Duration;

        private bool Infinite = false;

        [HideInInspector] public CharacterEntity Target;

        [HideInInspector] public Animator InstanciatedFXAnimator;

        public virtual bool ClassicCountdown { get => true; }

        /// <summary>
        /// Called when an alteration is put on the entity passed.
        /// </summary>
        /// <param name="entity">The affected entity.</param>
        public virtual void Setup(CharacterEntity entity)
        {
            Target = entity;
            //SetupFx?
            SFXManager.Instance.RefreshAlterationSFX(entity);
            entity.OnDeath += Unsubbing;
        }

        protected virtual void Unsubbing(EntityEventData Data)
        {
            Target.OnTurnEnded -= DecrementAlterationCountdown; //TODO: call this when you die.
        }

        /// <summary>
        /// Called every time an alteration has an effect. 
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Apply(CharacterEntity entity)
        {
            SFXManager.Instance.RefreshAlterationSFX(entity);
        }

        /// <summary>
        /// Called when an alteration wears off.
        /// </summary>
        /// <param name="entity"></param>
        public virtual void WearsOff(CharacterEntity entity)
        {
            //FxGoAway?
            SFXManager.Instance.RefreshAlterationSFX(entity);
            Unsubbing(new EntityEventData(this.Target));
            Target.Alterations.Remove(this);
        }

        /// <summary>
        /// Called when an alteration ticks down.
        /// </summary>
        /// <param name="data"></param>
        public virtual void DecrementAlterationCountdown(Events.EventData data)
        {
            if (Infinite) return;
            Duration--;
            if (Duration <= 0 && ClassicCountdown)
            {
                WearsOff(Target);
            }
        }

        public Alteration(int Duration)
        {
            this.Duration = Duration;
            if(Duration == -1)
            {
                Infinite= true;
            }
        }

        public override string ToString()
        {
            string cc = ClassicCountdown ? "\n(Can also decrement with other conditions)" : "";
            return $"{this.GetType().Name} for {Duration} turns.{cc}";
        }
    }
}
