using DownBelow.GridSystem;
using DownBelow.Managers;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public abstract class PendularAction : EntityAction
    {
        public PendularAction(CharacterEntity RefEntity, Cell TargetCell) 
            : base(RefEntity, TargetCell)
        {
        }

        public void Init(int Ticks)
        {
            this.requiredTicks = Ticks;
        }

        protected int currentTick = 0;
        protected int requiredTicks;

        protected int failedTicks = 0;
        protected int succeededTicks = 0;

        /// <summary>
        /// Used on the action to notify everyone through network of its process
        /// </summary>
        /// <param name="result"></param>
        public virtual void NotifyTick(bool result)
        {
            NetworkManager.Instance.EntityAskToTickAction(this, result);
        }
        
        /// <summary>
        /// Used locally, after being notified, that the action must be ticked forward
        /// </summary>
        public virtual void LocalTick(bool result)
        {
            this.currentTick++;

            if (result) { succeededTicks++; }
            else { failedTicks++; }
            
            if(this.currentTick >= this.requiredTicks)
            {
                this.OnFinalTick();
            }
        }

        protected abstract void OnFinalTick();

        public override object[] GetDatas()
        {
            return new object[1] { this.requiredTicks };
        }

        public override void SetDatas(object[] Datas)
        {
            this.requiredTicks = int.Parse(Datas[0].ToString());
        }
    }
}