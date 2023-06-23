using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public class EndTurnAction : EntityAction
    {
        public EndTurnAction(CharacterEntity RefEntity, Cell TargetCell) 
            : base(RefEntity, TargetCell)
        {
        }

        public override void ExecuteAction()
        {
            Debug.LogWarning("ENDED TURN : " + this.RefEntity);
            NetworkManager.Instance.PlayerAsksEndTurn();
            this.EndAction();
        }

        public override object[] GetDatas() { return new object[0]; }

        public override void SetDatas(object[] Datas) { }
    }
}