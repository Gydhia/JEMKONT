using DownBelow.Mechanics;
using DownBelow.UI;
using EODE.Wonderland;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.Managers
{
    public class DeckbuildingSystem : _baseManager<DeckbuildingSystem>
    {
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

        private Dictionary<EClass, List<ScriptableCard>> decks;
        private List<ScriptableCard> ActualDeck => decks[DeckDisplayed];

        public EClass DeckDisplayed;

        private int maxCardsDisplayed
        {
            get => SettingsManager.Instance.MaxCollectionCount();
        }

        Dictionary<EClass, Dictionary<ScriptableCard, int>> DecksNumbers = new();
        Dictionary<ScriptableCard, int> DeckNumbers => DecksNumbers[DeckDisplayed];
        List<DraggableCard> CardPool = new();
        const int MAXSAMECARDNUMBER = 3;

        #region ButtonMethods
        public void ShowDeckBuilding()
        {
            DeckDisplayed = GameManager.SelfPlayer.ActiveTools[0].Class;
            decks = new();
            DecksNumbers = new();
            foreach (var item in GameManager.SelfPlayer.ActiveTools)
            {
                DecksNumbers.Add(item.Class, new());
                decks.Add(item.Class, item.DeckPreset.Deck.Cards);
            }
            state = EDeckBuildState.Shown;
            ToggleShowButton(false);
            BG.SetActive(true);
            UIManager.Instance.PlayerInventory.gameObject.SetActive(false);

            //Displaying the actual deck to the left
            DisplayDeck(DeckDisplayed);

            //ShowingMiddleCards;
            ChangeCollectionDisplayed((int)DeckDisplayed);

        }

        public void SaveDeck()
        {
            foreach (var DeckNums in DecksNumbers)
            {
                foreach (var item in DeckNums.Value)
                {
                    for (int i = 0;i < item.Value;i++)
                    {
                        decks[DeckNums.Key].Add(item.Key);
                    }
                }
            }
            foreach (var item in GameManager.SelfPlayer.ActiveTools)
            {
                item.DeckPreset.Deck.Cards = decks[item.Class];
            }
        }

        public void SaveAndExit()
        {
            SaveDeck();
            HideDeckBuilding();
        }

        public void Exit()
        {
            HideDeckBuilding();
        }
        #endregion
        public override void Awake()
        {
            base.Awake();
            Init();
        }
        private void Start()
        {
            ShowDeckBuilding();
        }
        void Init()
        {
            state = EDeckBuildState.CanShow;
            //Instanciating card pool
            for (int i = 0;i < maxCardsDisplayed;i++)
            {
                var go = Instantiate(BigCardPrefab, BigCardsParent).GetComponent<DraggableCard>();
                go.gameObject.SetActive(false);
                CardPool.Add(go);
            }
        }

        /// <summary>
        /// To call when we need to hide the "Deckbuilding menu" button.
        /// </summary>
        public void HideAllUI()
        {
            state = EDeckBuildState.Hidden;
            ToggleShowButton(false);
        }

        void ToggleShowButton(bool on)
        {
            ShowDeckBuildButton.SetActive(on);
        }

        void HideDeckBuilding()
        {
            state = EDeckBuildState.CanShow;
            BG.SetActive(false);
            ToggleShowButton(true);
            UIManager.Instance.PlayerInventory.gameObject.SetActive(true);
        }

        #region ACTUALDECKDRAWING
        public void DisplayDeck(EClass eclass)
        {
            if (decks.TryGetValue(eclass, out List<ScriptableCard> d))
            {
                DisplayDeck(d);
            } else
            {
                //Stay on the previous deck..? display something like "missing the tool" or sum
            }
        }

        public void DisplayDeck(List<ScriptableCard> deck)
        {
            foreach (var item in deck)
            {
                TryAddCopy(item);
            }
            Debug.Log(DeckNumbers.Count);
            RefreshDeckDrawing();
        }

        void RefreshDeckDrawing()
        {
            var count = SmallCardsParent.childCount;
            DeckNumbers.RemoveAll(x => x.Value <= 0);

            for (int i = count - 1;i >= 0;i--)
            {
                Destroy(SmallCardsParent.GetChild(i).gameObject);
            }

            foreach (var pair in DeckNumbers)
            {
                var card = Instantiate(LittleCardPrefab, SmallCardsParent).GetComponent<SmallCardDeckbuilding>();
                card.Init(pair.Key, pair.Value, () => RemoveOneCopy(pair.Key));
            }
        }

        public void TryAddCopy(ScriptableCard card)
        {
            if (DeckNumbers.TryGetValue(card, out var number))
            {
                if (number >= MAXSAMECARDNUMBER)
                {
                    //TODO: ERROR: can't add more than 3 cards
                    Debug.Log($"ERROR: trying to have more than 3 {card.Title} in deck actual deck.");
                    return;
                }
            }
            if (!DeckNumbers.ContainsKey(card))
            {
                DeckNumbers.Add(card, 0);
            }
            DeckNumbers[card]++;
            RefreshDeckDrawing();
        }

        void RemoveOneCopy(ScriptableCard card)
        {
            if (!DeckNumbers.ContainsKey(card)) return;
            else
            {
                DeckNumbers[card]--;
                RefreshDeckDrawing();
            }
        }
        #endregion

        #region ActualCollectionDrawing
        public void ChangeCollectionDisplayed(int intcollection)
        {
            EClass collection = (EClass)intcollection;
            //Fuck les keufs
            CardPool.FindAll(x => x.gameObject.activeSelf).ForEach(card => { card.gameObject.SetActive(false); });

            for (int i = 0;i < SettingsManager.Instance.CollectionCards(collection).Count;i++)
            {
                CardPool[i].gameObject.SetActive(true);
                //CardPool[i].AddToDeckOnClick = collection == SettingsManager.Instance.PlayerClass;
                CardPool[i].Init(SettingsManager.Instance.CollectionCards(collection)[i]);
            }

            DeckDisplayed = collection;
            CollectionHeader.text = $"{Enum.GetName(typeof(EClass), intcollection)} Cards";
        }
        #endregion
    }

}
