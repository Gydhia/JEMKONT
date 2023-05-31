using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    [CreateAssetMenu(fileName = "ResourcePreset", menuName = "DownBelow/ScriptableObject/Interactables/Resource", order = 2)]
    public class ResourcePreset : InteractablePreset
    {
        public EClass GatherableBy;

        public ItemPreset ResourceItem;

        public GameObject DroppedObject;
        public MeshRenderer OnceGatheredObject;

        public int MinGathering = 1;
        public int MaxGathering = 4;

        public float TimeToGather = 3.2f;

        public override void Init(Cell attachedCell)
        {
            base.Init(attachedCell);
        }
    }
}