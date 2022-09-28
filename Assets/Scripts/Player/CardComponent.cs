using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
public class CardComponent : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {

    public ScriptableCard CardData;
    public Image IllustrationImage;
    public TextMeshPro CostText;
    public TextMeshPro TitleText;
    public TextMeshPro DescText;

    [ReadOnly] public bool isHovered;
    [ReadOnly] public bool isPressed;
    [ReadOnly] public bool isDragged;
    public float dragSensivity = 60f;

    private Vector2 originPosition;
    private void Start() {
        originPosition = transform.position;
    }
    public void Update() {
        Vector2 mousePos = Input.mousePosition;
        if (isDragged) {
            Drag(mousePos);
        }
        if (isPressed) {
            //Check with sensivity:
            var min = this.MinimumDragPosition();
            if (min.x <= mousePos.x || min.y <= mousePos.y) {
                isDragged = true;
            }
        }
    }
    private Vector2 MinimumDragPosition() {
        //Corresponds to the minimum drag we have to do before the cards begins to get dragged
        Vector2 pos = this.transform.position;
        pos.x += dragSensivity;
        pos.y += dragSensivity;
        return pos;
    }
    public void Hydrate(ScriptableCard CardData) {
        this.CardData = CardData;
        this.IllustrationImage.sprite = CardData.IllustrationImage;
        this.CostText.text = CardData.Cost.ToString();
        this.TitleText.text = CardData.Title;
        this.DescText.text = CardData.Description;
    }
    public void Hydrate() {
        //CardData is already the card we want:
        this.IllustrationImage.sprite = this.CardData.IllustrationImage;
        this.CostText.text = this.CardData.Cost.ToString();
        this.TitleText.text = this.CardData.Title;
        this.DescText.text = this.CardData.Description;
    }

    public void OnPointerClick(PointerEventData eventData) {
        //System design
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (isPressed || isHovered) {
            //Selected?
            isPressed = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovered = false;
    }

    private void Drag(Vector2 mousePos) {
        Vector2 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x,mousePos.x,0.1f);
        pos.y = Mathf.Lerp(pos.y,mousePos.y,0.1f);
        transform.position = pos;
    }
    public void OnPointerUp(PointerEventData eventData) {
        isPressed = false;
        isDragged = false;
        transform.position = originPosition;
    }
}
