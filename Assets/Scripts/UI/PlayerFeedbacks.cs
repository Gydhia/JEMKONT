using DownBelow.Events;
using DownBelow.GridSystem;
using EODE.Wonderland;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Tilemaps;
using UnityEngine;

namespace DownBelow.Entity
{
    public class PlayerFeedbacks : EntityFeedbacks
    {
        [SerializeField] private TextMeshProUGUI _pseudo;

        public override void Init(GameEventData Data)
        {
            this._pseudo.text = this._entity.EntityName;
            this._pseudo.gameObject.SetActive(!this._entity.CurrentGrid.IsCombatGrid && !((PlayerBehavior)this._entity).IsFake);
            base.Init(Data);
        }

        protected override void SetupForCombat(GridEventData data)
        {
            base.SetupForCombat(data);

            this._pseudo.gameObject.SetActive(false);
        }

        protected override void SetupForFarm(GridEventData data)
        {
            base.SetupForFarm(data);

            this._pseudo.gameObject.SetActive(true);
        }
    }
}
