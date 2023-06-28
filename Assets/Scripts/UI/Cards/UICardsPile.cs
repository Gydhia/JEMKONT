using System;
using DownBelow.Managers;
using DownBelow.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.SLayout;

public class UICardsPile : MonoBehaviour
{
    public PileType PileType;
    public int MaxStackedCards = -1;
    public List<DraggableCard> Cards;

    public SHorizontalLayoutGroup HLayoutGroup;
    public bool AdaptSpacing = false;
    [ShowIf("AdaptSpacing")]
    public float MinSpacing = -500;
    [ShowIf("AdaptSpacing")]
    public float MaxSpacing = -200;

    [Tooltip("When a card arrive in this pile, what pivot should it have ?")]
    public Vector2 CardPivot = new Vector2(0.5f, 0.5f);
    [Tooltip("If we can hover cards in this pile")]
    public bool AuthorizeHover = false;

    public TextMeshProUGUI CardsNumber;

    public Transform VisualMoveTarget;

    public Transform CardsHolder;

    public UICardsHolder _cardsHolder;
    
    public GameObject OverviewContent;
    public bool UnorganizeCards;
    
    private void Update()
    {
        if(this.CardsNumber != null)
        {
            this.CardsNumber.text = this.Cards.Count.ToString();
        }
        if (AdaptSpacing)
        {
            float newSpacing = Mathf.Lerp(MinSpacing, MaxSpacing, Cards.Count / (float)this.MaxStackedCards);
            HLayoutGroup.spacing = newSpacing;
        }
    }

    public void ShufflePile(string UID)
    {
        this.Cards.Shuffle(UID);
        AkSoundEngine.PostEvent("Play_SSFX_DeckShuffle", AudioHolder.Instance.gameObject);
    }

    public void ClickOnPile()
    {
        if (!_cardsHolder.IsPileOpen)
        {
            this.OverviewContent.SetActive(!this.OverviewContent.activeSelf);
            _cardsHolder.IsPileOpen = this.OverviewContent.activeInHierarchy;
        }
        else
        {
            if (this.OverviewContent.activeInHierarchy)
            {
                this.OverviewContent.SetActive(!this.OverviewContent.activeSelf);
                _cardsHolder.IsPileOpen = this.OverviewContent.activeInHierarchy;
            }
        }
            

        if (this.UnorganizeCards)
        {
            this._randomPlaceCards();
        }
    }

    private void _randomPlaceCards()
    {

    }
}
