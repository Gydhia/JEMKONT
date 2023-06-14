using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Mechanics
{
    [CreateAssetMenu(fileName = "Craft Recipe", menuName = "DownBelow/Inventory/Craft Preset", order = 1)]
    public class CraftPreset : SerializedScriptableObject
    {
        public string ItemName;
        public Sprite ItemIcon;

        public PlaceableItemPreset ToPlaceItem;


        public Dictionary<ItemPreset, int> CraftRecipe;
    }

}
