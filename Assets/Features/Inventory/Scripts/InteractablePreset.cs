using DownBelow.GridSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interactable", menuName = "DownBelow/ScriptableObject/Interactables/InteractablePreset", order = 2)]
public class InteractablePreset : BaseSpawnablePreset
{
    public Interactable ObjectPrefab;
    public Color OutlineColor;

    public Animation InteractAnimation;

    public override void Init(Cell attachedCell)
    {
        Interactable newInteractable =
            Instantiate(this.ObjectPrefab, attachedCell.transform);

        newInteractable.transform.position = attachedCell.WorldPosition;
        newInteractable.Init(this, attachedCell);
        attachedCell.AttachInteractable(newInteractable);
    }
}
