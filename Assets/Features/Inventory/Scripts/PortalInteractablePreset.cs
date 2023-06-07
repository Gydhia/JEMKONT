using DownBelow.Managers;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interactable", menuName = "DownBelow/Interactables/Portal Preset", order = 2)]
public class PortalInteractablePreset : InteractablePreset
{
    [ValueDropdown("GetSavedGrids")]
    public string TargetGrid;
    private IEnumerable<string> GetSavedGrids()
    {
        GridManager.Instance.LoadGridsFromJSON();
        return GridManager.Instance.SavedGrids.Keys;
    }
}
