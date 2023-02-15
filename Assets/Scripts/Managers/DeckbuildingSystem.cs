using DownBelow.Mechanics;
using EODE.Wonderland;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
        public TextMeshProUGUI CollectionHeader;
        [BoxGroup("UI")]
        public Transform SmallCardsParent;
        [BoxGroup("UI")]
        public Transform BigCardsParent;

        private Deck ActualDeck { get => SettingsManager.Instance.PlayerDeck; set => SettingsManager.Instance.PlayerDeck = value; }

        [BoxGroup("Presets")]
        public ScriptableCardList CardList;
        [BoxGroup("Presets")]
        public ECollection CollectionToDisplay;
        private int maxCardsDisplayed {
            get => CardList.MaxCollectionCount();
        }

        Dictionary<ScriptableCard, int> DeckNumbers = new();
        List<CardComponent> CardPool = new();
        const int MAXSAMECARDNUMBER = 3;

        public override void Awake() {
            base.Awake();
            Init();
        }
        private void Start() {
            ShowDeckBuilding();
        }
        void Init() {
            //Instanciating card pool
            for (int i = 0;i < maxCardsDisplayed;i++) {
                var go = Instantiate(BigCardPrefab, BigCardsParent).GetComponent<CardComponent>();
                go.gameObject.SetActive(false);
                CardPool.Add(go);
            }
            for (int i = 0;i < DeckSelectingButtons.Length;i++) {
                Button item = DeckSelectingButtons[i];
                item.onClick.RemoveAllListeners();
                item.GetComponentInChildren<TextMeshProUGUI>().text = ((ECollection)i).ToString();
            }
        }
        public void ShowDeckBuilding() {
            gameObject.SetActive(true);
            UIManager.Instance.PlayerInventory.gameObject.SetActive(false);
            //Displaying the actual deck to the left
            DisplayDeck(ActualDeck);

            //ShowingMiddleCards;
            ChangeCollectionDisplayed((int)CollectionToDisplay);

        }

        public void SaveDeck() {
            foreach (var item in DeckNumbers) {
                ActualDeck.Cards.Clear();
                for (int i = 0;i < item.Value;i++) {
                    ActualDeck.Cards.Add(item.Key);
                }
            }
        }

        private void HideDeckBuilding() {
            gameObject.SetActive(true);
            UIManager.Instance.PlayerInventory.gameObject.SetActive(true);
        }

        #region ACTUALDECKDRAWING
        public void DisplayDeck(Deck deck) {
            ActualDeck = deck;
            foreach (var item in deck.Cards) {
                TryAddCopy(item);
            }
            Debug.Log(DeckNumbers.Count);
            RefreshDeckDrawing();
        }
        ECollection classToCollection(EClass PlayerClass) {
            int Collection = (int)PlayerClass;
            return (ECollection)Collection;
        }
        void RefreshDeckDrawing() {
            var count = SmallCardsParent.childCount;
            DeckNumbers.RemoveAll(x => x.Value <= 0);
           // DeckNumbers.RemoveAll(x => (x.Key.Collection != ECollection.Common && x.Key.Collection != classToCollection(SettingsManager.Instance.PlayerClass)));
            for (int i = count - 1;i >= 0;i--) {
                Destroy(SmallCardsParent.GetChild(i).gameObject);
            }
            foreach (KeyValuePair<ScriptableCard, int> pair in DeckNumbers) {
                var card = Instantiate(LittleCardPrefab, SmallCardsParent).GetComponent<SmallCardDeckbuilding>();
                card.Init(pair.Key, pair.Value, () => RemoveOneCopy(pair.Key));
            }
        }

        public void TryAddCopy(ScriptableCard card) {
            if (DeckNumbers.TryGetValue(card, out var number)) {
                if (number >= MAXSAMECARDNUMBER) {
                    //TODO: ERROR: can't add more than 3 cards
                    Debug.LogError($"ERROR: trying to have more than 3 {card.Title} in deck actual deck.");
                    return;
                }
            }
            if (!DeckNumbers.ContainsKey(card)) {
                DeckNumbers.Add(card, 0);
            }
            DeckNumbers[card]++;
            RefreshDeckDrawing();
        }

        void RemoveOneCopy(ScriptableCard card) {
            if (!DeckNumbers.ContainsKey(card)) return; else { 
                DeckNumbers[card]--;
                RefreshDeckDrawing();
            }
        }
        #endregion

        #region ActualCollectionDrawing
        public void ChangeCollectionDisplayed(int intcollection) {
            ECollection collection = (ECollection)intcollection;
            //Fuck les keufs
            CardPool.FindAll(x => x.gameObject.activeSelf).ForEach(card => { card.gameObject.SetActive(false); });

            for (int i = 0;i < CardList.CollectionCards(collection).Count;i++) {
                CardPool[i].gameObject.SetActive(true);
                CardPool[i].Init(CardList.CollectionCards(collection)[i]);
            }

            CollectionToDisplay = collection;
            CollectionHeader.text = $"{Enum.GetName(typeof(ECollection), collection)} Cards";
        }
        #endregion
    }

}
