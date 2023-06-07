using DownBelow.Spells.Alterations;
using EODE.Wonderland;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "DownBelow/AlterationSFXList")]
public class ScriptableSFXAlterationList : SerializedScriptableObject
{
    public List<AlterationSFX> AlterationsSFX;

    public GameObject GetValue(Alteration alt)
    {
        if (alt is BuffAlteration buff)
        {
            return AlterationsSFX.Find(x => x.Alteration is BuffAlteration buffFound && buff.StatToBuff == buffFound.StatToBuff).AlterationSFXPrefab;
        } else
        {
            return AlterationsSFX.Find(x => x.GetType() == alt.GetType()).AlterationSFXPrefab;
        }
    }

    public bool TryGetValue(Alteration alt, out GameObject prefab)
    {
        prefab = GetValue(alt);
        return prefab != null;
    }
}
public class AlterationSFX
{
    [OdinSerialize]
    public Alteration Alteration;
    public GameObject AlterationSFXPrefab;
}