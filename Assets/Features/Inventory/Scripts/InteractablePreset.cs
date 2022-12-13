using Jemkont.GridSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interactable", menuName = "DownBelow/ScriptableObject/Interactable", order = 2)]
public class InteractablePreset : BaseSpawnablePreset
{
    public Interactable ObjectPrefab;
    public Color OutlineColor;

    public override void Init(Cell attachedCell)
    {
        Interactable newInteractable =
            Instantiate(this.ObjectPrefab, attachedCell.WorldPosition, Quaternion.identity, attachedCell.transform);

        newInteractable.Init(this);
    }
}
