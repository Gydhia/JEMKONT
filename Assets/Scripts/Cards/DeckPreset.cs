using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="NewDeckPreset.asset",menuName ="Presets/DeckPreset")]
public class DeckPreset : SerializedScriptableObject
{
    [ReadOnly]
    public Guid UID;
    [OnValueChanged("_updateUID")]
    public string Name;

    public EClass Class;

    public Deck Deck;


    public Deck Copy() {
        return new Deck(Deck);
    }

    private void _updateUID()
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(this.Name));
            this.UID = new Guid(hash);
        }
    }
}
