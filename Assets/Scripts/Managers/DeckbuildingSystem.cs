using DownBelow.Managers;
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
using Utility.SLayout;

namespace DownBelow.UI
{
    public class DeckbuildingSystem : MonoBehaviour
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
        [BoxGroup("UI")] 
        public SVerticalLayoutGroup CurrentDeckLayoutGroup;

        private EDeckBuildState state;

        private Dictionary<EClass, List<ScriptableCard>> decks;
        private List<ScriptableCard> ActualDeck => decks[DeckDisplayed];

        public EClass DeckDisplayed;

        private int maxCardsDisplayed
        {
            get => SettingsManager.Instance.MaxCollectionCount();
        }

        Dictionary<EClass, Dictionary<ScriptableCard, int>> DecksNumbers = new();
        Dictionary<ScriptableCard, int> ActualDeckNumbers => DecksNumbers[DeckDisplayed];
        List<CardVisual> CardPool = new();
        const int MAXSAMECARDNUMBER = 3;

        #region ButtonMethods
        public void ShowDeckBuilding()
        {
            if (GameManager.SelfPlayer.ActiveTools == null || GameManager.SelfPlayer.ActiveTools.Count == 0)
            {
                return;
            }
            DeckDisplayed = GameManager.SelfPlayer.ActiveTools[0].Class;
            decks = new();
            DecksNumbers = new();
            foreach (var item in GameManager.SelfPlayer.ActiveTools)
            {
                DecksNumbers.Add(item.Class, new());
                decks.Add(item.Class, item.DeckPreset.Deck.Cards);
            }
            foreach (var deck in decks)
            {
                foreach (var card in deck.Value)
                {
                    TryAddCopyInDeck(card, deck.Key, false);
                }
            }
            state = EDeckBuildState.Shown;
            ToggleShowButton(false);
            BG.SetActive(true);
            UIManager.Instance.PlayerInventory.gameObject.SetActive(false);

            //Show cards and deck;
            ChangeCollectionDisplayed((int)DeckDisplayed);

        }

        public void SaveDeck()
        {
            foreach (var DeckNums in DecksNumbers)
            {
                decks[DeckNums.Key].Clear();
                foreach (var item in DeckNums.Value)
                {
                    for (int i = 0;i < item.Value;i++)
                    {
                        decks[DeckNums.Key].Add(item.Key);
                    }
                }
            }
            foreach (ToolItem item in GameManager.SelfPlayer.ActiveTools)
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

        private void CannotDeckbuild(Events.GridEventData Data) => HideAllUI();
        private void CanDeckbuild(Events.GridEventData Data) => HideDeckBuilding();

        public void Init()
        {
            CombatManager.Instance.OnCombatStarted += CannotDeckbuild;
            CombatManager.Instance.OnCombatEnded += CanDeckbuild;

            state = EDeckBuildState.CanShow;
            //Instanciating card pool
            for (int i = 0;i < maxCardsDisplayed;i++)
            {
                var go = Instantiate(BigCardPrefab, BigCardsParent);
                go.SetActive(false);
                CardPool.Add(go.GetComponent<CardVisual>());
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
        public void DisplayDeck(EClass eclass, bool refreshDrawing = true)
        {

            if (DecksNumbers[eclass].Values.Count == 0 && decks.TryGetValue(eclass, out List<ScriptableCard> d))
            {
                DisplayDeck(d, refreshDrawing);
            } else if (DecksNumbers[eclass].Values.Count != 0)
            {
                RefreshDeckDrawing();
            } else
            {
                //Stay on the previous deck..? display something like "missing the tool" or sum
            }
        }

        public void DisplayDeck(List<ScriptableCard> deck, bool refreshDrawing = true)
        {
            foreach (var item in deck)
            {
                TryAddCopy(item);
            }
            Debug.Log(ActualDeckNumbers.Count);
            if (refreshDrawing)
            {
                RefreshDeckDrawing();
            }
        }

        public void TryAddCopyInDeck(ScriptableCard card, EClass DeckClass, bool forceRedraw = false)
        {
            if (DecksNumbers[DeckClass].TryGetValue(card, out var number))
            {
                if (number >= MAXSAMECARDNUMBER)
                {
                    //TODO: ERROR: can't add more than 3 cards
                    Debug.Log($"ERROR: trying to have more than {MAXSAMECARDNUMBER} {card.Title} in deck of the {DeckDisplayed}.");
                    return;
                }
            }
            if (!DecksNumbers[DeckClass].ContainsKey(card))
            {
                DecksNumbers[DeckClass].Add(card, 0);
            }
            DecksNumbers[DeckClass][card]++;
            if (forceRedraw)
            {
                RefreshDeckDrawing();
            }
        }

        public void TryAddCopy(ScriptableCard card, bool forceRedraw = false)
        {
            TryAddCopyInDeck(card, DeckDisplayed, forceRedraw);
        }

        void RefreshDeckDrawing()
        {
            var count = SmallCardsParent.childCount;
            ActualDeckNumbers.RemoveAll(x => x.Value <= 0);
            //TODO: do a pool? Instantiate a pool of [MAXNBCARDSINDECK] prefabs, deactivate the ones u don't need, finito
            for (int i = count - 1;i >= 0;i--)
            {
                Destroy(SmallCardsParent.GetChild(i).gameObject);
            }

            foreach (var pair in ActualDeckNumbers)
            {
                var card = Instantiate(LittleCardPrefab, SmallCardsParent).GetComponent<SmallCardDeckbuilding>();
                card.Init(pair.Key, pair.Value, () => RemoveOneCopy(pair.Key));
            }
        }


        void RemoveOneCopy(ScriptableCard card)
        {
            if (!ActualDeckNumbers.ContainsKey(card)) return;
            else
            {
                ActualDeckNumbers[card]--;
                RefreshDeckDrawing();
            }
        }
        #endregion

        #region ActualCollectionDrawing
        public void ChangeCollectionDisplayed(int intcollection)
        {
            if (!GameManager.SelfPlayer.ActiveTools.Any(x => x.Class == ((EClass)intcollection)))
            {
                return;
            }
            EClass collection = (EClass)intcollection;
            //Fuck les keufs
            CardPool.FindAll(x => x.gameObject.activeSelf).ForEach(card => { card.gameObject.SetActive(false); });

            for (int i = 0;i < SettingsManager.Instance.OwnedClassCards(collection).Count;i++)
            {
                CardPool[i].gameObject.SetActive(true);
                CardPool[i].Init(SettingsManager.Instance.OwnedClassCards(collection)[i]);
            }

            DeckDisplayed = collection;
            CollectionHeader.text = $"{Enum.GetName(typeof(EClass), intcollection)} Cards";
            DisplayDeck(collection);
        }
        #endregion
    }

}
