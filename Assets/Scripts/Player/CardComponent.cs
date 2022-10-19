using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Jemkont.GridSystem;
using Jemkont.Managers;
using Jemkont.Mechanics;

public class CardComponent : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {

    public ScriptableCard CardData;
    public Image IllustrationImage;
    public Image ShineImage;
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescText;

    [ReadOnly] public bool isInPlacingMode;

    [ReadOnly] public bool isHovered;
    [ReadOnly] public bool isPressed;
    [ReadOnly] public bool isDragged;
    public float dragSensivity = 60f;
    public float transformDragSensitivity = 180f;

    private Vector2 originPosition;
    [ReadOnly]public Cell HoveredCell;
    private bool _hasScaledDown = false;

    private void Start() 
    {
        this.ShineImage.enabled = false;
        this.CostText.text = this.CardData.Cost.ToString();
        this.TitleText.text = this.CardData.Title;
        this.DescText.text = this.CardData.Description;
        originPosition = transform.position;
    }

    public void Update() 
    {
        Vector2 mousePos = Input.mousePosition;
        if (isDragged) {
            this.Drag(mousePos);
        }
        if (isPressed) {
            //Check with sensivity:
            var min = this.MinimumDragPosition();
            var minTransform = this.MinimumDragTransformPosition();
            if (min.x <= mousePos.x || min.y <= mousePos.y) {
                this.isDragged = true;
                CardDraggingSystem.instance.DraggedCard = this;

                if (!this._hasScaledDown && (minTransform.x < mousePos.x || min.y <= mousePos.y))
                {
                    InputManager.Instance.ChangeCursorAppearance(CursorAppearance.Card);
                    StartCoroutine(ScaleDown(0.25f));
                }
            }
        }
    }
    public void Dissapear() {
        //Idfk
        //TODO : MAke card dissapear + FX appears?
        IllustrationImage.SetAlpha(0);
        isInPlacingMode = true;
    }
    public void ReAppear() {
        //In case you cancel the spell or anything.
        //IllustrationImage.SetAlpha(1);
        isInPlacingMode = false;
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
    public void CastSpell(Cell cellToCastSpellOn) {
        // /!\ position might be off.
        StartCoroutine(this.GoToPile(0.28f, UIManager.Instance.CardSection.DiscardPile.transform.position));
        this.ExecuteSpells(cellToCastSpellOn, this.CardData.Spells);
    }
    #region pointerHandlers&Drag
    private Vector2 MinimumDragPosition() {
        //Corresponds to the minimum drag we have to do before the cards begins to get dragged
        Vector2 pos = this.transform.position;
        pos.x += dragSensivity;
        pos.y += dragSensivity;
        return pos;
    }
    private Vector2 MinimumDragTransformPosition()
    {
        //Corresponds to the minimum drag we have to do before the cards begins to get dragged
        Vector2 pos = this.transform.position;
        pos.x += transformDragSensitivity;
        pos.y += transformDragSensitivity;
        return pos;
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
    public void OnPointerEnter(PointerEventData eventData) 
    {
        this.ShineImage.enabled = true;
        isHovered = true;
    }
    public void OnPointerExit(PointerEventData eventData) {
        this.ShineImage.enabled = false;
        isHovered = false;
    }
    public void OnPointerUp(PointerEventData eventData) {
        
        if(isInPlacingMode && HoveredCell != null) {
            //Needs to check if you can cast the spell on this cell too. If you can:
            CastSpell(HoveredCell);
        } else {
            isPressed = false;
            isDragged = false;
            transform.position = originPosition;
            ReAppear();
            InputManager.Instance.ChangeCursorAppearance(CursorAppearance.Idle);
            if (this._hasScaledDown)
                StartCoroutine(ScaleUp(0.25f));
            //Card is going to be reset anyways.
        }
    }
    private void Drag(Vector2 mousePos) {
        Vector2 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x,mousePos.x,0.1f);
        pos.y = Mathf.Lerp(pos.y,mousePos.y,0.1f);
        transform.position = pos;
    }

    public void DrawCardFromPile()
    {
        this.gameObject.SetActive(true);
        this.transform.position = UIManager.Instance.CardSection.DrawPile.transform.position;
        StartCoroutine(GoToHand(0.28f));
    }

    public IEnumerator GoToHand(float time)
    {
        Vector2 target = UIManager.Instance.CardSection.HandPile.transform.position;
        Vector2 basePos = this.transform.position;
        float timer = 0f;

        while (timer < time)
        {
            float value = timer * (1 / time);
            this.transform.localScale = new Vector3(value, value, value);
            this.transform.position = Vector2.Lerp(basePos, target, value);

            timer += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public IEnumerator GoToPile(float time, Vector2 target)
    {
        float timer = 0f;
        Vector2 basePos = this.transform.position;

        while (timer < time)
        {
            float value = timer * (1 / time);
            this.transform.localScale = new Vector3(1 - value, 1 - value, 1 - value);
            this.transform.position = Vector2.Lerp(basePos, target, value);

            timer += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        CombatManager.Instance.DiscardCard(this);
    }

    public IEnumerator ScaleDown(float time)
    {
        this._hasScaledDown = true;

        float timer = time;
        while(timer > 0f)
        {
            float value = timer * (1 / time);
            this.transform.localScale = new Vector3(value, value, value);

            timer -= Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
    public IEnumerator ScaleUp(float time)
    {
        float timer = 0f;
        while (timer < time)
        {
            float value = timer * (1 / time);
            this.transform.localScale = new Vector3(value, value, value);

            timer += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        this._hasScaledDown = false;
            
    }
    #endregion

    #region Spells_handling
    private Jemkont.Spells.Spell[] _currentSpells;

    private Coroutine _spellCor = null;

    internal void ExecuteSpells(Cell target, Jemkont.Spells.Spell[] spells)
    {
        this._currentSpells = spells;
        
        this._spellCor = StartCoroutine(this._waitForSpell(target));
    }
    private IEnumerator _waitForSpell(Jemkont.GridSystem.Cell target)
    {
        for (int i = 0; i < this._currentSpells.Length; i++)
        {
            bool canExecute = true;

            if (this._currentSpells[i].ConditionData != null)
                if (i - 1 >= 0)
                    if (!this._currentSpells[i].ConditionData.Check(this._currentSpells[i - 1].CurrentAction.Result))
                        canExecute = false;

            if (canExecute)
            {
                this._currentSpells[i].CurrentAction = Instantiate(this._currentSpells[i].ActionData, Vector3.zero, Quaternion.identity, CombatManager.Instance.CurrentPlayingEntity.gameObject.transform);
                this._currentSpells[i].ExecuteSpell(CombatManager.Instance.CurrentPlayingEntity, target);
                while (!this._currentSpells[i].CurrentAction.HasEnded)
                {
                    yield return new WaitForSeconds(Time.deltaTime);
                }
            }
        }

        this.gameObject.SetActive(false);
    }
    #endregion
}

