using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="NewDeckPreset.asset",menuName ="Presets/DeckPreset")]
public class DeckPreset : ScriptableObject
{
    public Deck Deck;
    public EClass Class;
    public Deck Copy() {
        return new Deck(Deck);
    }
}
