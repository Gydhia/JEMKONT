
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Jemkont.Entity;
using Jemkont.GridSystem;

[CreateAssetMenu(menuName = "Entity/EntityPreset")]
public class EntityPreset : BaseSpawnablePreset
{
    public CharacterEntity Entity;
    public bool IsPNJ = false;

    public EntityStats Statistics;

    public override void Init(Cell attachedCell)
    {
        EnemyEntity newEntity = Instantiate(this.Entity, attachedCell.WorldPosition, Quaternion.identity, attachedCell.RefGrid.transform) as EnemyEntity;
       
        newEntity.IsAlly = IsPNJ && !attachedCell.RefGrid.IsCombatGrid;
        newEntity.EnemyStyle = this;
        newEntity.Init(this.Statistics, attachedCell, attachedCell.RefGrid, attachedCell.PositionInGrid.longitude  );
        newEntity.gameObject.SetActive(false);

        attachedCell.RefGrid.GridEntities.Add(newEntity);
    }
}
