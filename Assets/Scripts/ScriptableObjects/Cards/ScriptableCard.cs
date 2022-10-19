using Jemkont.Spells;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum EChipType { Melee, Ranged, Special }

[CreateAssetMenu(menuName ="Card")]
public class  ScriptableCard : ScriptableObject {
    public string Title;
    public int Cost;
    [TextArea] public string Description;
    public Sprite IllustrationImage;
    public GameObject Spell;

    public List<Spell> Spells;
}
public class CardComparer : IComparer<ScriptableCard> {
    public int Compare(ScriptableCard x, ScriptableCard y) {
        return x.Title.CompareTo(y.Title);
    }
}
