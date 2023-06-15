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
        AlterationSFX altsfx;
        if (alt is BuffAlteration buff)
        {
            altsfx = AlterationsSFX.Find(x => x.Alteration is BuffAlteration buffFound && buff.StatToBuff == buffFound.StatToBuff);
        } else
        {
            altsfx = AlterationsSFX.Find(x => x.GetType() == alt.GetType());
        }
        return altsfx?.AlterationSFXPrefab;
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