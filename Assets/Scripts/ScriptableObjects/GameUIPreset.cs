using DownBelow.UI.Inventory;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameUIPreset", menuName = "DownBelow/Presets/GameUIPreset", order = 4)]

public class GameUIPreset : SerializedScriptableObject
{
    public UIInventoryItem ItemPrefab;

    public Sprite ItemCase;

    public int[] SlotsByPlayer;
}
