using DownBelow.Mechanics;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "DownBelow/Cards/CardList")]
public class ScriptableCardList : SerializedScriptableObject {
    public List<ScriptableCard> AllCards;
}
