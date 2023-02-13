using DownBelow.Spells.Alterations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="DownBelow/AlterationSFXList")]
public class ScriptableSFXAlterationList : SerializedScriptableObject
{
    public Dictionary<EAlterationType, GameObject> AlterationsSFX;
}
