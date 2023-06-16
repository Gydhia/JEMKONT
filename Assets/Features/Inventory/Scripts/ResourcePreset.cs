using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    [CreateAssetMenu(fileName = "ResourcePreset", menuName = "DownBelow/Interactables/Resource", order = 2)]
    public class ResourcePreset : InteractablePreset
    {
        public EClass GatherableBy;

        public ItemPreset ResourceItem;

        public int MinGathering = 1;
        public int MaxGathering = 4;

        public override void Init(Cell attachedCell)
        {
            base.Init(attachedCell);
        }
    }
}