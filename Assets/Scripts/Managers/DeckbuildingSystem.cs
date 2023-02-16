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
        private enum EDeckBuildState { Shown, CanShow, Hidden }//Shown => we're dblding, CanShow => the "Show Deckbuilding" button is shown, Hidden => even this button is hidden

        [BoxGroup("Prefabs")]
        public GameObject BigCardPrefab;
        [BoxGroup("Prefabs")]
        public GameObject LittleCardPrefab;

        [BoxGroup("UI")]
        public Button[] DeckSelectingButtons;
        [BoxGroup("UI")]
        public GameObject ShowDeckBuildButton;
        [BoxGroup("UI")]
        public TextMeshProUGUI CollectionHeader;
        [BoxGroup("UI")]
        public Transform SmallCardsParent;
        [BoxGroup("UI")]
        public Transform BigCardsParent;
        [BoxGroup("UI")]
        public GameObject BG;

        private EDeckBuildState state;
        private Deck ActualDeck { get => SettingsManager.Instance.PlayerDeck; set => SettingsManager.Instance.PlayerDeck = value; }

        [BoxGroup("Presets")]
        public ScriptableCardList CardList;
        [BoxGroup("Presets")]
        public EClass CollectionToDisplay;
        private int maxCardsDisplayed {
            get => CardList.MaxCollectionCount();
        }

        Dictionary<ScriptableCard, int> DeckNumbers = new();
        List<CardComponent> CardPool = new();
        const int MAXSAMECARDNUMBER = 3;

        #region ButtonMethods
        public void ShowDeckBuilding() {
            state = EDeckBuildState.Shown;
            ToggleShowButton(false);
            BG.SetActive(true);
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

        public void SaveAndExit() {
            SaveDeck();
            HideDeckBuilding();
        }

        public void Exit() {
            HideDeckBuilding();
        }
        #endregion
        public override void Awake() {
            base.Awake();
            Init();
        }
        private void Start() {
           //ShowDeckBuilding();
        }
        void Init() {
            state = EDeckBuildState.CanShow;
            //Instanciating card pool
            for (int i = 0;i < maxCardsDisplayed;i++) {
                var go = Instantiate(BigCardPrefab, BigCardsParent).GetComponent<CardComponent>();
                go.gameObject.SetActive(false);
                CardPool.Add(go);
            }
        }

        
        void HideAllUI() {
            state = EDeckBuildState.Hidden;
            ToggleShowButton(false);
        }

        void ToggleShowButton(bool on) {
            ShowDeckBuildButton.SetActive(on);
        }

        void HideDeckBuilding() {
            state = EDeckBuildState.CanShow;
            BG.SetActive(false);
            ToggleShowButton(true);
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

        void RefreshDeckDrawing() {
            var count = SmallCardsParent.childCount;
            DeckNumbers.RemoveAll(x => x.Value <= 0);
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
            EClass collection = (EClass)intcollection;
            //Fuck les keufs
            CardPool.FindAll(x => x.gameObject.activeSelf).ForEach(card => { card.gameObject.SetActive(false); });

            for (int i = 0;i < CardList.CollectionCards(collection).Count;i++) {
                CardPool[i].gameObject.SetActive(true);
                CardPool[i].AddToDeckOnClick = collection == SettingsManager.Instance.PlayerClass;
                CardPool[i].Init(CardList.CollectionCards(collection)[i]);
            }

            CollectionToDisplay = collection;
            CollectionHeader.text = $"{Enum.GetName(typeof(EClass), intcollection)} Cards";
        }
        #endregion
    }

}
