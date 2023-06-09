
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using DownBelow.Entity;
using DownBelow.GridSystem;

[CreateAssetMenu(menuName = "DownBelow/Entity/EntityPreset")]
public class EntityPreset : BaseSpawnablePreset
{
    public CharacterEntity Entity;
    public bool IsPNJ = false;

    public EntityStats Statistics;

    public Sprite EntityIcon;

    public override void Init(Cell attachedCell)
    {
        base.Init(attachedCell);

        EnemyEntity newEntity = Instantiate(this.Entity, attachedCell.WorldPosition, Quaternion.identity, attachedCell.RefGrid.transform) as EnemyEntity;
       
        newEntity.IsAlly = IsPNJ && !attachedCell.RefGrid.IsCombatGrid;
        newEntity.EnemyStyle = this;
        newEntity.Init(attachedCell, attachedCell.RefGrid, attachedCell.PositionInGrid.longitude);
        newEntity.SetStatistics(this.Statistics);
        newEntity.gameObject.SetActive(false);

        attachedCell.RefGrid.GridEntities.Add(newEntity);
    }
}
