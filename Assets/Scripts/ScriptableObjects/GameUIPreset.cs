using DownBelow.UI.Inventory;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameUIPreset", menuName = "DownBelow/BasePresets/GameUIPreset", order = 4)]

public class GameUIPreset : SerializedScriptableObject
{
    public UIInventoryItem ItemPrefab;

    public Sprite ItemCase;

    public int[] SlotsByPlayer;

    [Header("CARDS")]
    public Color AttackColor;
    public Color PowerColor;
    public Color SkillColor;

    public Color ToolAttackColor;
    public Color ToolHoverColor;

    [FoldoutGroup("Combat turns")]
    public Sprite Ally;
    [FoldoutGroup("Combat turns")]
    public Sprite Dead;
    [FoldoutGroup("Combat turns")]
    public Sprite SelectedBackground;
    [FoldoutGroup("Combat turns")]
    public Sprite NormalBackground;

    [FoldoutGroup("Abysses")]
    public Sprite CardSmall;
    [FoldoutGroup("Abysses")]
    public Sprite ResourcesEnergy;

    public Dictionary<EntityStatistics, Sprite> StatisticSprites;
}
