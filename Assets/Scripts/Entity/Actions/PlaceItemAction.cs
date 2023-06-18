using DownBelow.GridSystem;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;

namespace DownBelow.Entity
{
    public class PlaceItemAction : EntityAction
    {
        public PlaceableItem ItemPreset;

        public PlaceItemAction(CharacterEntity RefEntity, Cell TargetCell) 
            : base(RefEntity, TargetCell)
        {
        }

        public void Init(PlaceableItem itemPreset)
        {
            this.ItemPreset = itemPreset;
        }

        public override void ExecuteAction()
        {
            this.ItemPreset.PlaceLocal(this.TargetCell);
            this.EndAction();
        }

        public override object[] GetDatas()
        {
            return new object[1] { this.ItemPreset.UID };
        }

        public override void SetDatas(object[] Datas)
        {
            this.ItemPreset = SettingsManager.Instance.ItemsPresets[Guid.Parse(Datas[0].ToString())] as PlaceableItem;
        }
    }
}
