using DownBelow.Mechanics;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmallCardDeckbuilding : MonoBehaviour {
    public TextMeshProUGUI[] texts;

    private UnityEngine.Events.UnityAction LeftClick;
    private ScriptableCard card;

    private void Update() {
        if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Input.mousePosition)) {
            if (LeftClick != null && Input.GetMouseButtonDown(0)) {
                LeftClick();
            }
            if (card != null && Input.GetMouseButtonDown(1)) {
                RightClick();
            }
        }

    }
    public void Init(ScriptableCard card, int number, UnityEngine.Events.UnityAction call) {
        texts[0].text = number.ToString();
        texts[1].text = card.Title.ToString();
        texts[2].text = card.Cost.ToString();
        LeftClick = call;
    }
    /// <summary>
    /// TODO: on est d'accord que qd tu dis "Right clicking a card makes it take a larger portion
    /// of the screen and shows additional information."
    /// c'est basiquement afficher une popup de la carte, genre littéralement la même carte qu'on joue IG?
    /// Yep c'est ca, en un peu plus zoomé, comme dans hearthstone si tu vois
    /// </summary>
    public void RightClick() {
        Debug.Log("PopupCard");
    }
}
