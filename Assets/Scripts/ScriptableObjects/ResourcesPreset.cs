using Sirenix.OdinInspector;
using UnityEngine;

namespace DownBelow.GameData
{
    [CreateAssetMenu(fileName = "ResourcesPreset", menuName = "DownBelow/BasePresets/Resources Preset", order = 1)]
    public class ResourcesPreset : SerializedScriptableObject
    {
        // WARNING : The following values are not well handled by code, so make sure that their values are LOGIC between them.

        [FoldoutGroup("Resource QTE"), Range(0.01f, 0.8f), Tooltip("The width in percentage that will takes the zone to hit")]
        public float InteractWidth = 0.2f;
        [FoldoutGroup("Resource QTE"), Range(0.5f, 6f), Tooltip("The time until the zone to hit reach 0")]
        public float DecreaseSpeed = 1.5f;
        [FoldoutGroup("Resource QTE"), Range(0.01f, 6f), Tooltip("The width per second that the cursor will cross")]
        public float CursorCrossTime = 0.3f;
        [FoldoutGroup("Resource QTE"), Range(0f, 1f), Tooltip("The max X position that the cursor's center will be at")]
        public float MinX = 0.6f;
        [FoldoutGroup("Resource QTE"), Range(0f, 1f), Tooltip("The max X position that the cursor's center will be at")]
        public float MaxX = 0.3f;

        [FoldoutGroup("World Resources")]
        public int MaxGatherableResources = 20;
        [FoldoutGroup("World Resources"), Tooltip("When winning over an abyss, how much max resources will be added ?")]
        public int GatherableResourcesPerAbyss = 5;
        

    }
}
