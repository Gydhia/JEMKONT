using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Mechanics
{
    [CreateAssetMenu(fileName = "CellTrigger", menuName = "DownBelow/Grid/CellTrigger", order = 2)]
    public class CellTriggerPreset : BaseSpawnablePreset
    {
        public DialogPreset DialogPreset;
        public TriggerCell TriggerCellPrefab;

        public override void Init(Cell attachedCell)
        {
            base.Init(attachedCell);

            var triggerCell = Instantiate(this.TriggerCellPrefab, attachedCell.transform);
            triggerCell.Init(DialogPreset, attachedCell);

        }
    }
}
