using DownBelow.Events;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Entity
{
    public abstract class NonCharacterEntity : MonoBehaviour
    {
        //Protected by default, feel free to change, i don't really care.
        public CellState AffectingState;

        [HideInInspector] public Cell AttachedCell;
        protected int TurnsLeft;
        protected CharacterEntity RefEntity;
        protected NCEPreset PresetRef;
        /// <summary>
        /// Defines if the NCE is destroyable via getting "damaged". Does not impact the countdown.
        /// </summary>
        protected bool Destroyable => true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AttachedCell"></param>
        /// <param name="TurnsLeft"></param>
        /// <param name="RefEntity"></param>
        /// <param name="preset">GROOOOOOOOOOOOSSE flemme de renommer en "preset" sur tout les constructeurs, mais ouai ça a changé</param>
        public virtual void Init(Cell AttachedCell, int TurnsLeft, CharacterEntity RefEntity, NCEPreset preset)
        {
            AttachedCell.AttachedNCE = this;
            this.AttachedCell = AttachedCell;
            this.AttachedCell.ChangeCellState(AffectingState, true);
            this.TurnsLeft = TurnsLeft;
            this.RefEntity = RefEntity;
            this.PresetRef = preset;
            this.RefEntity.OnTurnBegun += Decrement;
        }

        /// <summary>
        /// To call when the entity gets hit by a damaging spell. If this is not working, check the example in Spell_Stats.cs, or different references.
        /// </summary>
        public virtual void Hit()
        {
            if (Destroyable) DestroyEntity();
        }

        public virtual void Decrement(GameEventData data)
        {
            TurnsLeft--;
            if (TurnsLeft <= -1)
            {
                DestroyEntity();
            }
        }

        /// <summary>
        /// Calling Destroy(this.gameObject) is the base. Consider calling it after whatever you're adding just in case.
        /// </summary>
        public virtual void DestroyEntity()
        {
            AttachedCell.AttachedNCE = null;
            AttachedCell.ChangeCellState(CellState.Walkable);
            RefEntity.OnTurnBegun -= Decrement;
            Destroy(this.gameObject);
            //Animation one day :pray:
        }
    }
}