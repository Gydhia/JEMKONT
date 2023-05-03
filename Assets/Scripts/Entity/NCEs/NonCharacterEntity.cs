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
        protected Cell AttachedCell;
        protected int TurnsLeft;
        protected CharacterEntity RefEntity;

        /// <summary>
        /// Defines if the NCE is destroyable via getting "damaged". Does not impact the countdown.
        /// </summary>
        protected bool Destroyable => true;

        public virtual void Init(Cell attachedCell, int TurnsLeft, CharacterEntity RefEntity)
        {
            AttachedCell = attachedCell;
            this.TurnsLeft = TurnsLeft;
            this.RefEntity = RefEntity;
            this.RefEntity.OnTurnBegun += Decrement;
        }

        /// <summary>
        /// To call when the entity gets hit.
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

        public virtual void DestroyEntity()
        {
            AttachedCell.ChangeCellState(CellState.Walkable);
            RefEntity.OnTurnBegun -= Decrement;
            Destroy(this.gameObject);
            //Animation one day :pray:
        }
    }
}