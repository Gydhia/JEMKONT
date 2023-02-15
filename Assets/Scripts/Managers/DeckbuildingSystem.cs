using DownBelow.Mechanics;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.Managers {
    public class DeckbuildingSystem : _baseManager<DeckbuildingSystem> {

        [BoxGroup("Prefabs")]
        public GameObject BigCardPrefab;
        [BoxGroup("Prefabs")]
        public GameObject LittleCardPrefab;

        [BoxGroup("UI")]
        public Button[] DeckSelectingButtons;
        [BoxGroup("UI")]
        public Transform SmallCardsParent;
        [BoxGroup("UI")]
        public Transform BigCardsParent;

        [BoxGroup("Data")]
        public DeckScriptable ActualDeck;

        [BoxGroup("Presets")]
        public ScriptableCardCollection CardCollections;
        public ECollection CollectionToDisplay;
        private int maxCardsDisplayed {
            get {
                int max = 0;
                foreach (var item in CardCollections.Collections) {
                    max = Math.Max(item.Value.Length,max); 
                }
                return max;
            }
        }

        Dictionary<ScriptableCard, int> DeckNumbers = new();
        List<CardComponent> CardPool = new();
        const int MAXSAMECARDNUMBER = 3;


        private void Start() {
            //Displaying the actual deck to the left
            DisplayDeck(ActualDeck.Deck);

            //Instanciating card pool
            for (int i = 0;i < maxCardsDisplayed;i++) {
                var go = Instantiate(BigCardPrefab, BigCardsParent).GetComponent<CardComponent>();
                go.gameObject.SetActive(false);
                CardPool.Add(go);
            }
        }
        #region ACTUALDECKDRAWING
        public void DisplayDeck(Deck deck) {
            ActualDeck.Deck = deck;
            foreach (var item in deck.Cards) {
                TryAddCard(item);
            }
            Debug.Log(DeckNumbers.Count);
            RefreshDeckDrawing();
        }

        void RefreshDeckDrawing() {
            var count = SmallCardsParent.childCount;
            for (int i = count - 1;i >= 0;i--) {
                Destroy(SmallCardsParent.GetChild(i).gameObject);
            }
            foreach (KeyValuePair<ScriptableCard, int> pair in DeckNumbers) {
                var card = Instantiate(LittleCardPrefab, SmallCardsParent).GetComponent<SmallCardDeckbuilding>();
                card.Init(pair.Key, pair.Value, () => RemoveOneCopy(pair.Key));
            }
        }

        void TryAddCard(ScriptableCard card) {
            if (DeckNumbers.TryGetValue(card, out var number)) {
                if (number >= MAXSAMECARDNUMBER) {
                    //TODO: ERROR: can't add more than 3 cards
                    Debug.LogError($"ERROR: trying to have more than 3 {card.Title} in deck {ActualDeck.name}");
                    return;
                }
            }
            if (!DeckNumbers.ContainsKey(card)) {
                DeckNumbers.Add(card, 0);
            }
            DeckNumbers[card]++;
        }

        void RemoveOneCopy(ScriptableCard card) {
            if (!DeckNumbers.TryGetValue(card, out var number)) {
                if (number >= MAXSAMECARDNUMBER) {
                    //TODO: ERROR: can't add more than 3 cards
                    Debug.LogError($"ERROR: trying to have more than 3 {card.Title} in deck {ActualDeck.name}");
                    return;
                }
            }
            if (DeckNumbers.ContainsKey(card)) {
                DeckNumbers[card]--;
            }
        }
        #endregion

        #region ActualCollectionDrawing
        public void ChangeCollectionDisplayed(ECollection collection) {
            for (int i = CardCollections.Collections[CollectionToDisplay].Length; i < CardCollections.Collections[collection].Length;i++) {

            }
            CollectionToDisplay = collection;
        }
        #endregion
    }

}
