using System;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DownBelow.Mechanics;
using System.Linq;
using System.Security.AccessControl;
using DG.Tweening;
using Random = UnityEngine.Random;
using DownBelow.Events;

namespace DownBelow.UI
{
    public class DraggableCard : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
    {
        public static DraggableCard SelectedCard;

        private static DraggableCard _hoveredCard;
        public static DraggableCard HoveredCard
        {
            get { return _hoveredCard; }
            set
            {
                _hoveredCard?.CardVisual.Unhover();
                _hoveredCard = value;
                _hoveredCard?.CardVisual.Hover();
            }
        }

        public CardVisual CardVisual;
        public ScriptableCard CardReference;

        public UICardsPile RefPile;

        // In %, which part of the bottom screen won't Pin the card ?
        public int PinUpdatePercents = 33;
        public int BottomDeadPercents = 20;
        public float DistanceToDrag = 5f;
        public float FollowSensivity = 0.12f;

        public bool AnyTargeting;
        public bool IsDragged = false;
        public bool PinnedToScreen = false;
        public bool PinnedLeft = false;

        private RectTransform m_RectTransform;
        // Cached when card clicked. Reference 
        private Vector2 _startDragPos;
        // TODO: Temporary, later on it'll be handled by the card pile
        private Vector2 _spawnPosition;

        private bool _isDestroying = false;
        private Coroutine _followCoroutine = null;
        private Coroutine _compareCoroutine = null;
        private Coroutine _pinUpdateCoroutine = null;

        private int _childOrder;
        private Sequence _flipSequence;
        public void Init(ScriptableCard CardReference, UICardsPile Pile)
        {
            this.Init(CardReference);

            this.RefPile = Pile;
            this.m_RectTransform.pivot = this.RefPile.CardPivot;
        }

        public void Init(ScriptableCard CardReference)
        {
            this.m_RectTransform = this.GetComponent<RectTransform>();
            this.CardVisual ??= this.GetComponent<CardVisual>();

            this.CardReference = CardReference;
            this.CardVisual.Init(CardReference);

            this.AnyTargeting = this.CardReference.Spells.Any(s => s.Data.RequiresTargetting);

            this.gameObject.SetActive(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                this._onLeftClickDown();
        }

        // Player is trying to drag the card
        private void _onLeftClickDown()
        {
            if (GameManager.SelfPlayer.Mana < this.CardReference.Cost)
            {
                GameManager.SelfPlayer.FireMissingMana();
                return;
            }

            if (!GameManager.SelfPlayer.IsPlayingEntity || !this.RefPile.AuthorizeHover)
            {
                return;
            }

            // Forbid the card drag if currently using another one
            if (HoveredCard == this && SelectedCard == null && !this._isDestroying)
            {
                SelectedCard = this;
                this._startDragPos = Mouse.current.position.ReadValue();

                this._compareCoroutine = StartCoroutine(this._compareDistanceToStartFollow());
            }
        }

        // Player released the card.
        private void _onLeftClickUp(InputAction.CallbackContext ctx) => this._onLeftClickUp();

        private void _onLeftClickUp()
        {
            PlayerInputs.player_l_click.canceled -= _onLeftClickUp;
            PlayerInputs.player_r_click.canceled -= _onRightClick;

            // We left clicked while draggin the card in the dead zone, so cancel
            if (AnyTargeting || (Mouse.current.position.ReadValue().y / Screen.height < this.BottomDeadPercents / 100f))
            {
                this.DiscardToHand();
            }
            else
            {
                _flipSequence = DOTween.Sequence();
                
                _flipSequence.Append(this.m_RectTransform.DOPunchScale(new Vector3(1.0001f, 1.0001f, 1.0001f), 0.4f)
                    .SetEase(Ease.OutQuad));
                
                _flipSequence.Append(this.m_RectTransform.DOPunchRotation(new Vector3(0, 180, 0), 0.2f)
                    .SetEase(Ease.OutQuad).OnComplete(
                        () =>
                        {
                            CardVisual.ReverseCard();
                        }));
                _flipSequence.Append(this.m_RectTransform.DOPunchRotation(new Vector3(0, 180, 0), 0.2f)
                    .SetEase(Ease.OutQuad).OnComplete(
                        () =>
                        {
                            CardVisual.ReverseCard();
                        }));
                _flipSequence.Append(this.m_RectTransform.DOPunchRotation(new Vector3(0, 180, 0), 0.2f)
                    .SetEase(Ease.OutQuad).OnComplete(
                        () =>
                        {
                            CardVisual.ReverseCard();
                            this.PlayNotTargetCard();
                        }));

                _flipSequence.SetLoops(0);
                
                _flipSequence.Restart();
            }
        }

        private void _onRightClick(InputAction.CallbackContext ctx) => this._onRightClick();
        private void _onRightClick()
        {
            this._abortCoroutine(ref _followCoroutine);

            PlayerInputs.player_l_click.canceled -= _onLeftClickUp;
            PlayerInputs.player_r_click.canceled -= _onRightClick;

            CombatManager.Instance.AbortUsedSpell(new CellEventData(GameManager.SelfPlayer.EntityCell));
        }

        private void UpdateCardToPlayable()
        {
            // For spells that requires targeting, we'll pin the card
            PlayerInputs.player_l_click.canceled -= _onLeftClickUp;

            this._abortCoroutine(ref this._followCoroutine);

            if (Mouse.current.position.ReadValue().y / Screen.height > this.BottomDeadPercents / 100f)
            {
                this.PinCardToScreen();
            }
            else
            {
                this.DiscardToHand();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HoveredCard = this;
            HoveredCard.Hover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (HoveredCard == this)
            {
                HoveredCard.UnHover();
                HoveredCard = null;
            }
        }

        public void StartDrag()
        {
            this._abortCoroutine(ref this._compareCoroutine);

            this.IsDragged = true;

            PlayerInputs.player_l_click.canceled += _onLeftClickUp;
            PlayerInputs.player_r_click.canceled += _onRightClick;

            this._followCoroutine = StartCoroutine(this._followCursor());
        }


        private void Hover()
        {
            if (!this.RefPile.AuthorizeHover)
                return;

            UIManager.Instance.CardSection.SetAllLayoutGroups(false);
            _childOrder = this.m_RectTransform.GetSiblingIndex();
            this.m_RectTransform.SetSiblingIndex(this.m_RectTransform.childCount - 1);
            this.m_RectTransform.DOAnchorPosY(HoveredCard._spawnPosition.y + 100f, 0.3f);
        }

        private void UnHover()
        {
            if (!this.RefPile.AuthorizeHover)
                return;

            this.m_RectTransform.DOAnchorPosY(HoveredCard._spawnPosition.y - 175f, 0.3f);
            this.m_RectTransform.SetSiblingIndex(_childOrder);
            UIManager.Instance.CardSection.SetAllLayoutGroups(true);
        }

        public void PinCardToScreen()
        {
            this.PinnedToScreen = true;
            this.PinnedLeft = true;

            CombatManager.Instance.FireCardBeginUse(this.CardReference);

            this._pinUpdateCoroutine = StartCoroutine(_updatePinnedPosition());
        }

        public void PlayNotTargetCard()
        {
            this._abortCoroutine(ref this._followCoroutine);
            CombatManager.Instance.FireCardBeginUse(this.CardReference);
            InputManager.Instance.ChangeCursorAppearance(CursorAppearance.Idle);
        }

        public void DrawFromPile(UICardsPile fromPile, UICardsPile toPile)
        {
            this.RefPile = toPile;
            this.gameObject.SetActive(true);

            this.m_RectTransform.localScale = Vector3.one * 0.2f;
            this.transform.parent = fromPile.VisualMoveTarget;
            this.transform.position = Vector3.zero;

            int result = Random.Range(1, 11);

            this.m_RectTransform.DOPunchRotation(Vector3.one * 0.8f, 1.3f, result);
            this.m_RectTransform.DOPunchScale(Vector3.one * 0.8f, 1.3f, result);
            this.m_RectTransform.DOPunchPosition(Vector3.one * 0.8f, 1.3f, result).OnComplete((() =>
            {
                this.m_RectTransform.localScale = Vector3.one;
                this.m_RectTransform.parent = this.RefPile.CardsHolder;
                this._spawnPosition = m_RectTransform.position;
                this.m_RectTransform.DOAnchorPosY(this._spawnPosition.y, 0.3f);

                // FOR CARDS OVERVIEW, we're resetting this
                this.m_RectTransform.pivot = this.RefPile.CardPivot;
            }));
        }

        public void DiscardToHand()
        {
            PlayerInputs.player_r_click.canceled -= _onRightClick;

            this._abortCoroutine(ref this._compareCoroutine);
            this._abortCoroutine(ref this._pinUpdateCoroutine);
            this.PinnedToScreen = this.IsDragged = false;
            this.m_RectTransform.SetParent(this.RefPile.CardsHolder, false);
            UIManager.Instance.CardSection.SetAllLayoutGroups(false);
            UIManager.Instance.CardSection.SetAllLayoutGroups(true);
            this.m_RectTransform.DOAnchorPos(this._spawnPosition, 0.3f).SetEase(Ease.OutQuad);
            
            SelectedCard = null;
        }

        public void DiscardToPile(UICardsPile toPile)
        {
            this._isDestroying = true;
            SelectedCard = null;

            this._abortCoroutine(ref _followCoroutine);
            this._abortCoroutine(ref _compareCoroutine);
            this._abortCoroutine(ref _pinUpdateCoroutine);

            this.m_RectTransform.DOPunchRotation(Vector3.one * 0.8f, .4f, 3);
            this.m_RectTransform.DOPunchScale(Vector3.one * 0.8f, .4f, 3);
            this.m_RectTransform.DOScale(0.2f, .4f);
            this.m_RectTransform.DOMove(toPile.VisualMoveTarget.position, 0.4f)
                .OnComplete(() => this.Burn());
        }

        public void Burn()
        {
            this.gameObject.SetActive(false);
        }

        private IEnumerator _compareDistanceToStartFollow()
        {
            while (!this.IsDragged)
            {
                // TODO : make this % / responsive. 
                if (Vector2.Distance(this._startDragPos, Mouse.current.position.ReadValue()) > this.DistanceToDrag)
                {
                    this.StartDrag();
                }

                yield return null;
            }
        }

        private IEnumerator _followCursor()
        {
            Vector2 lastPos = this._startDragPos;
            float timer = 0f;

            while (true)
            {
                yield return null;
                timer += Time.unscaledDeltaTime;

                Vector2 newPos = new Vector2(
                    Mathf.Lerp(lastPos.x, Mouse.current.position.ReadValue().x, timer / this.FollowSensivity),
                    Mathf.Lerp(lastPos.y, Mouse.current.position.ReadValue().y, timer / this.FollowSensivity)
                );

                if (newPos != lastPos)
                {
                    lastPos = newPos;
                    timer = 0f;
                }

                this.m_RectTransform.position = newPos;
                
                if (this.AnyTargeting && Mouse.current.position.ReadValue().y / Screen.height > this.BottomDeadPercents / 100f)
                {
                    UpdateCardToPlayable();
                }
            }
        }
        private IEnumerator _updatePinnedPosition()
        {
            this._pinInverse();

            while (true)
            {
                yield return null;

                if (this.PinnedLeft ?
                    Mouse.current.position.ReadValue().x / Screen.width < this.PinUpdatePercents / 100f :
                    Mouse.current.position.ReadValue().x / Screen.width > 1f - (this.PinUpdatePercents / 100f))
                {
                    this._pinInverse();
                }
            }
        }

        private void _abortCoroutine(ref Coroutine coroutine)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = null;
        }

        private void _pinInverse()
        {
            this.PinnedLeft = !this.PinnedLeft;

            this.m_RectTransform.parent = this.PinnedLeft ?
                UIManager.Instance.CombatSection.LeftPin :
                UIManager.Instance.CombatSection.RightPin;

            this.m_RectTransform.DOLocalMove(Vector2.zero, 0.3f).SetEase(Ease.InOutQuint);
        }

        private void OnDestroy()
        {
            PlayerInputs.player_l_click.canceled -= _onLeftClickUp;
            PlayerInputs.player_l_click.canceled -= _onRightClick;
        }
    }

}