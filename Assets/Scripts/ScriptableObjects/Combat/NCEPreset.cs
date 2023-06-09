using DownBelow.Entity;
using DownBelow.GridSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New NCE Preset", menuName = "DownBelow/Entity/NCEPreset")]
public class NCEPreset : SerializedScriptableObject
{
    public NonCharacterEntity entityToSummon;
    [Min(1)]public int Duration;

    public NonCharacterEntity InitNCE(Cell cell, CharacterEntity RefEntity)
    {
        NonCharacterEntity NCEInstance = GameObject.Instantiate(entityToSummon, cell.transform);
        NCEInstance.Init(cell, Duration, RefEntity, this);
        return NCEInstance;
    }

}
