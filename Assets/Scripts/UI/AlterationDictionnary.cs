using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DownBelow.Spells.Alterations;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Alteration Dictionnary", menuName = "DownBelow/AlterationDictionnary", order = 1)]
public class AlterationDictionnary : SerializedScriptableObject
{
    public Dictionary<EAlterationType, Sprite> DictionaryAlterations => _alterationDictionnary;
    
    [SerializeField]
    private Dictionary<EAlterationType, Sprite> _alterationDictionnary = new Dictionary<EAlterationType, Sprite>();
}
