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
    [Min(1)] public int Duration;
    public bool randomRotation;

    public NonCharacterEntity InitNCE(Cell cell, CharacterEntity RefEntity)
    {
        Quaternion rot = Quaternion.identity;
        if (randomRotation)
        {
            //Random for the rotation of the NCe, c pour faire joli pas besoin de synchro les random?
            float u = Random.Range(0, 1);
            float v = Random.Range(0, 1);
            float w = Random.Range(0, 1);
            rot = new(Mathf.Sqrt(1 - u) * Mathf.Sin(2 * Mathf.PI * v), Mathf.Sqrt(1 - u) * Mathf.Cos(2 * Mathf.PI * v), Mathf.Sqrt(u) * Mathf.Sin(2 * Mathf.PI * w), Mathf.Sqrt(u) * Mathf.Cos(2 * Mathf.PI * w));
        }
        NonCharacterEntity NCEInstance = GameObject.Instantiate(entityToSummon, Vector3.zero, rot, cell.transform);
        NCEInstance.Init(cell, Duration, RefEntity, this);
        return NCEInstance;
    }

}
