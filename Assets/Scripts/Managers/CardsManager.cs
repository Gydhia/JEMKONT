using DownBelow.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
namespace DownBelow.Managers
{
    [ShowOdinSerializedPropertiesInInspector]
    public class CardsManager : SerializedMonoBehaviour
    {
        public static CardsManager Instance;
        private void Awake() {
            Instance= this;
        }
        public DeckPreset[] DeckPresets;
        public Dictionary<string,ScriptableCard> ScriptableCards;
        private void OnValidate() {
            Dictionary<string,ScriptableCard> NewScriptableCards = new();
            foreach (KeyValuePair<string,ScriptableCard> item in ScriptableCards) {
                NewScriptableCards.Add(item.Value.name,item.Value);
                //TODO: change name by GUID?
            }
            ScriptableCards = NewScriptableCards;
        }
    }
}

