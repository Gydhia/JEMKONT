using DownBelow.Mechanics;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "DownBelow/Presets/CardList")]
public class ScriptableCardList : SerializedScriptableObject {
    public List<ScriptableCard> AllCards;

    public List<ScriptableCard> CollectionCards(EClass collection) {
        return AllCards.FindAll(x => x.Class == collection);
    }
    public int MaxCollectionCount() {
        int max = 0;
        foreach (var item in Enum.GetValues(typeof(EClass))) {
            max = Mathf.Max(max,CollectionCards((EClass)item).Count);
        }
        return max;
    }
}
