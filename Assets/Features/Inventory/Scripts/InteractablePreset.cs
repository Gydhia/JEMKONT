using DownBelow.GridSystem;
using DownBelow.Managers;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interactable", menuName = "DownBelow/Interactables/InteractablePreset", order = 2)]
public class InteractablePreset : BaseSpawnablePreset
{
    public Interactable ObjectPrefab;
    public Color OutlineColor;

    public Animation InteractAnimation;

    [Tooltip("-1 for infinite uses, > 0 for restrictions")]
    public int Durability = -1;
    public ParticleSystem DestroySFX;

    public override void Init(Cell attachedCell)
    {
        base.Init(attachedCell);

        Interactable newInteractable =
            Instantiate(this.ObjectPrefab, attachedCell.transform);

        newInteractable.transform.position = attachedCell.WorldPosition;
        newInteractable.Init(this, attachedCell);
        attachedCell.AttachInteractable(newInteractable);
    }
}
