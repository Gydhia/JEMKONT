using DownBelow.Managers;
using DownBelow.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    public void ShufflePile(ref List<DraggableCard> cards)
    {
        cards.Shuffle();
    }
}
