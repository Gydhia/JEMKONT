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
        public Cell AttachedCell;
        protected int TurnsLeft;
        protected CharacterEntity RefEntity;
        protected NonCharacterEntity PrefabRef;
        /// <summary>
        /// Defines if the NCE is destroyable via getting "damaged". Does not impact the countdown.
        /// </summary>
        protected bool Destroyable => true;

        public virtual void Init(Cell AttachedCell, int TurnsLeft, CharacterEntity RefEntity,NonCharacterEntity prefab)
        {
            AttachedCell.AttachedNCE = this;
            this.AttachedCell = AttachedCell;
            this.TurnsLeft = TurnsLeft;
            this.RefEntity = RefEntity;
            this.PrefabRef = prefab;
            this.RefEntity.OnTurnBegun += Decrement;
        }

        /// <summary>
        /// To call when the entity gets hit by a damaging spell. If this is not working, check the example in Spell_Stats.cs, or different references.
        /// </summary>
        public virtual void Hit()
        {
            if(Destroyable) DestroyEntity();
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