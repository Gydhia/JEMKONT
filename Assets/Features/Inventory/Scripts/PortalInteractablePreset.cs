using DownBelow.Managers;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interactable", menuName = "DownBelow/Interactables/Portal Preset", order = 2)]
public class PortalInteractablePreset : InteractablePreset
{
    [Tooltip("If true, will open panel to chose an abyss. If false, returns to farm land")]
    public bool ToCombat;
}
