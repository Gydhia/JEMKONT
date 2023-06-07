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


    public TextMeshProUGUI CardsNumber;

    public Transform VisualMoveTarget;

    public Transform CardsHolder;
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
    }

    public void ClickOnPile()
    {
        this.OverviewContent.SetActive(!this.OverviewContent.activeSelf);

        if (this.UnorganizeCards)
        {
            this._randomPlaceCards();
        }
    }

    private void _randomPlaceCards()
    {

    }
}
