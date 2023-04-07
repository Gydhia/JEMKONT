using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Mechanics;
using System.Threading.Tasks;
using DG.Tweening;
using DownBelow.Events;
using Sirenix.Utilities;

public class CardComponent : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler,
    IPointerExitHandler
{
    public ScriptableCard CardData;
    public Image IllustrationImage;
    public Image ShineImage;

    public TextMeshProUGUI CostText;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescText;

    public bool isInPlacingMode;

    public bool isHovered;
    public bool isPressed;
    public bool isDragged;
    public bool canBeHovered = true;

    public float dragSensivity = 60f;
    public float transformDragSensitivity = 180f;
    public RectTransform ContentRectTransform;

    private RectTransform _thisRectTransform;
    private bool _hasScaledDown = false;
    private Vector2 _originPosition;
    public float _hoverCoeffcient = 1.8f;
    private HorizontalLayoutGroup _layoutGroup;
    private Vector3 _scale;
    private Vector2 maxAnchors;
    private Vector2 minAnchors;

    private Vector4 previousAnchors;
    private int childOrder;

    private bool hasBeenInit = false;

    public Action<CardComponent> OnHover;
    public Action<CardComponent> OnStopHover;

    public void Init(ScriptableCard cardData)
    {
        this.CardData = cardData;

        this.ShineImage.enabled = false;
        this.CostText.text = CardData.Cost.ToString();
        this.TitleText.text = CardData.Title;
        this.DescText.text = CardData.Description;
        this.IllustrationImage.sprite = CardData.IllustrationImage;
        this._thisRectTransform = this.GetComponent<RectTransform>();
        this._originPosition = _thisRectTransform.anchoredPosition;
        this._scale = this.transform.localScale;
        this.maxAnchors = this._thisRectTransform.anchorMax;
        this.maxAnchors = this._thisRectTransform.anchorMin;
        switch (cardData.CardType)
        {
            case CardType.Attack:
                ShineImage.color = SettingsManager.Instance.GameUIPreset.AttackColor;
                break;
            case CardType.Power:
                ShineImage.color = SettingsManager.Instance.GameUIPreset.PowerColor;
                break;
            case CardType.Skill:
                ShineImage.color = SettingsManager.Instance.GameUIPreset.SkillColor;
                break;
            default:
                break;
        }
    }

    public void ApplyCost(int much)
    {
        if (this.CardData)
            this.CardData.Cost += much;
        else
            Debug.LogError("NO CARD SETUP");
    }

    public void Burn()
    {
        // TODO: Find a better way than destroying after 2s
        Destroy(this.gameObject, 2f);
    }

    public void Update()
    {
        if (!GameManager.Instance.SelfPlayer.IsPlayingEntity)
            return;

        Vector2 mousePos = Input.mousePosition;
        if (isDragged)
        {
            this.Drag(mousePos);
        }

        if (isPressed)
        {
            if (this.CardData.Cost > CombatManager.Instance.CurrentPlayingEntity.Mana)
                return;

            //Check with sensivity:
            var min = this.MinimumDragPosition();
            var minTransform = this.MinimumDragTransformPosition();
            if (min.x <= mousePos.x || min.y <= mousePos.y)
            {
                this.isDragged = true;

                if (!this._hasScaledDown && (minTransform.x < mousePos.x || min.y <= mousePos.y))
                {
                    CombatManager.Instance.OnCardEndDrag += _processEndDrag;
                    CombatManager.Instance.FireCardBeginDrag(this.CardData);
                    StartCoroutine(ScaleDown(0.25f));
                }
            }
        }
    }

    private void _processEndDrag(CardEventData Data)
    {
        CombatManager.Instance.OnCardEndDrag -= _processEndDrag;

        if (Data.Played)
        {
            StartCoroutine(GoToPile(0.28f, UIManager.Instance.CardSection.DiscardPile.transform.position));
            this.Burn();
        }
        else
        {
            isPressed = false;
            isDragged = false;

            GoBackToHand();
        }
    }

    public void ReAppear()
    {
        //In case you cancel the spell or anything.
        //IllustrationImage.SetAlpha(1);
        isInPlacingMode = false;
    }

    #region pointerHandlers&Drag

    private Vector2 MinimumDragPosition()
    {
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

    private void RefreshLayoutGroup()
    {
        _layoutGroup.enabled = false;
        _layoutGroup.enabled = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //System design
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPressed || isHovered)
        {
            //Selected?
            isPressed = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.ShineImage.enabled = true;
        if (!isHovered && canBeHovered)
        {
            Hover();
            isHovered = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.ShineImage.enabled = false;
        if (isHovered)
        {
            StopHover();
            isHovered = false;
        }
    }

    private void Hover()
    {
        OnHover?.Invoke(this);

        previousAnchors = new Vector4(this.ContentRectTransform.anchorMin.x, this.ContentRectTransform.anchorMax.x,
            this.ContentRectTransform.anchorMin.y, this.ContentRectTransform.anchorMax.y);

        _layoutGroup.enabled = false;

        this.transform.SetSiblingIndex(_layoutGroup.transform.childCount - 1);

        this.ContentRectTransform.DOAnchorMax(new Vector2(maxAnchors.x + 0.3f, maxAnchors.y + 0.3f), 0.3f)
            .SetEase(Ease.OutQuad);

        this.ContentRectTransform.DOAnchorMin(new Vector2(maxAnchors.x - 0.3f, maxAnchors.y - 0.3f), 0.3f)
            .SetEase(Ease.OutQuad);

        this.ContentRectTransform.DOAnchorPosY(0.2f, 0.3f).SetEase(Ease.OutQuad);
    }

    private void StopHover()
    {
        this.transform.SetSiblingIndex(childOrder);

        _layoutGroup.enabled = true;

        this.ContentRectTransform.DOAnchorMax(new Vector2(previousAnchors.y, previousAnchors.w), 0.3f)
            .SetEase(Ease.OutQuad);

        this.ContentRectTransform.DOAnchorMin(new Vector2(previousAnchors.x, previousAnchors.z), 0.3f)
            .SetEase(Ease.OutQuad).OnComplete((() =>
            {
                previousAnchors = new Vector4(this.ContentRectTransform.anchorMin.x,
                    this.ContentRectTransform.anchorMax.x, this.ContentRectTransform.anchorMin.y,
                    this.ContentRectTransform.anchorMax.y);
                OnStopHover?.Invoke(this);
            }));


        // this.ContentRectTransform.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutQuad).OnComplete((() =>  ));
    }

    private void Drag(Vector2 mousePos)
    {
        Vector2 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, mousePos.x, 0.1f);
        pos.y = Mathf.Lerp(pos.y, mousePos.y, 0.1f);
        transform.position = pos;
    }

    public void DrawCardFromPile()
    {
        this.gameObject.SetActive(true);
        this.transform.position = UIManager.Instance.CardSection.DrawPile.transform.position;
        StartCoroutine(GoToHand(0.28f));
    }

    public void GoBackToHand()
    {
        _thisRectTransform.DOAnchorPos(_originPosition, 0f);

        ReAppear();
        InputManager.Instance.ChangeCursorAppearance(CursorAppearance.Idle);
        if (this._hasScaledDown)
            StartCoroutine(ScaleUp(0.25f));
        RefreshLayoutGroup();
    }

    public IEnumerator GoToHand(float time)
    {
        Vector2 target = UIManager.Instance.CardSection.CardsHolder.transform.position;
        Vector2 basePos = this.transform.position;

        this.transform.parent = UIManager.Instance.CardSection.CardsHolder.transform;
        float timer = 0f;

        while (timer < time)
        {
            float value = timer * (1 / time);
            this.transform.localScale = new Vector3(value, value, value);
            this.transform.position = Vector2.Lerp(basePos, target, value);

            timer += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (!hasBeenInit)
        {
            _layoutGroup = this.transform.parent.GetComponent<HorizontalLayoutGroup>();

            for (int i = 0; i < _layoutGroup.transform.childCount; i++)
            {
                if (_layoutGroup.transform.GetChild(i) == this.transform)
                {
                    this.childOrder = i;
                    i = _layoutGroup.transform.childCount;
                }
            }

            RefreshLayoutGroup();
            hasBeenInit = true;
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
        this.transform.parent = UIManager.Instance.CardSection.DiscardPile.transform;
    }

    public IEnumerator ScaleDown(float time)
    {
        this._hasScaledDown = true;
        GetComponentsInChildren<Image>().ForEach(i => i.gameObject.SetActive(false));
        float timer = time;
        while (timer > 0f)
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
        GetComponentsInChildren<Image>(true).ForEach(i => i.gameObject.SetActive(true));
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

    private DownBelow.Spells.Spell[] _currentSpells;

    private Coroutine _spellCor = null;

    #endregion
}