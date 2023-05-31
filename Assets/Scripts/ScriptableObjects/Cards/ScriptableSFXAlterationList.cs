using DownBelow.Spells.Alterations;
using EODE.Wonderland;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="DownBelow/AlterationSFXList")]
public class ScriptableSFXAlterationList : SerializedScriptableObject
{
    public Dictionary<Type, GameObject> AlterationsSFX;
}
