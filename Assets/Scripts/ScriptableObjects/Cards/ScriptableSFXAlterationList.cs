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

    public AlterationSFX GetValue(Alteration alt)
    {
        AlterationSFX altsfx;
        if (alt is BuffAlteration buff)
        {
            altsfx = AlterationsSFX.Find(x => x.Alteration is BuffAlteration buffFound && buff.StatToBuff == buffFound.StatToBuff);
        } else
        {
            altsfx = AlterationsSFX.Find(x => x.Alteration.GetType().Name == alt.GetType().Name);
        }
        return altsfx;
    }

    public bool TryGetValue(Alteration alt, out AlterationSFX SFX)
    {
        SFX = GetValue(alt);
        return SFX != null;
    }
}
public class AlterationSFX
{
    [OdinSerialize]
    public Alteration Alteration;
    public GameObject AlterationSFXPrefab;
    public bool Loop;
}