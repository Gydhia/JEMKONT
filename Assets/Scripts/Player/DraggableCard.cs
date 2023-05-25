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

        public void Init(ScriptableCard CardReference, UICardsPile Pile)
        {
            this.RefPile = Pile;
            this.Init(CardReference);
        }

        public void Init(ScriptableCard CardReference)
        {
            this.m_RectTransform = this.GetComponent<RectTransform>();
            this.CardVisual ??= this.GetComponent<CardVisual>();

            this.CardReference = CardReference;
            this.CardVisual.Init(CardReference);

            this._subToEvents();

            this.gameObject.SetActive(false);
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
            // TODO : some cards are null which shouldn't happen. Instead of SelectedCard != this, we should remember which one we focused and subs to inputs
            if (SelectedCard == null || SelectedCard != this || this.PinnedToScreen)
                return;
            
            
            this._abortCoroutine(ref this._followCoroutine);
            Debug.Log("Left click on card " + this.name.ToString());

            if (Mouse.current.position.ReadValue().y / Screen.height > this.BottomDeadPercents / 100f)
            {
                this.PinCardToScreen();
                Debug.Log("PIN");
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
            this._followCoroutine = StartCoroutine(this._followCursor());
        }

        private void Hover()
        {
            Debug.Log("Hovered : " + this.name);
            this.m_RectTransform.DOAnchorPosY(HoveredCard._spawnPosition.y + 100f, 0.3f);
        }

        private void UnHover()
        {
            Debug.Log("Unhovered : " + this.name);
            this.m_RectTransform.DOAnchorPosY(HoveredCard._spawnPosition.y - 175f, 0.3f);
        }

        public void PinCardToScreen()
        {
            this.PinnedToScreen = true;
            this.PinnedLeft = true;

            CombatManager.Instance.FireCardBeginUse(this.CardReference);

            this._pinUpdateCoroutine = StartCoroutine(_updatePinnedPosition());
        }

        public void DrawFromPile(UICardsPile fromPile, UICardsPile toPile)
        {
            this.RefPile = toPile;
            this.gameObject.SetActive(true);
            
            this.m_RectTransform.localScale = Vector3.one * 0.2f;
            this.transform.parent = fromPile.transform;
            this.transform.position = Vector3.zero;

            int result = Random.Range(1, 11);

            this.m_RectTransform.DOPunchRotation(Vector3.one * 0.8f, 1.3f, result);
            this.m_RectTransform.DOPunchScale(Vector3.one * 0.8f, 1.3f, result);
            this.m_RectTransform.DOPunchPosition(Vector3.one * 0.8f, 1.3f, result).OnComplete((() =>
            {
                this.m_RectTransform.localScale = Vector3.one;
                this.m_RectTransform.parent = this.RefPile.transform;
                this._spawnPosition = m_RectTransform.position;
                this.m_RectTransform.DOAnchorPosY(this._spawnPosition.y, 0.3f);
            }));
        }

        public void DiscardToHand()
        {
            this._abortCoroutine(ref this._compareCoroutine);
            this._abortCoroutine(ref this._pinUpdateCoroutine);
            this.PinnedToScreen = this.IsDragged = false;
            this.m_RectTransform.SetParent(this.RefPile.transform, false);
            UIManager.Instance.CardSection.UpdateLayoutGroup();
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
            this.m_RectTransform.DOMove(toPile.transform.position, 0.4f)
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

        private void _subToEvents()
        {
            PlayerInputs.player_l_click.canceled += _onLeftClickUp;
        }

        private void _unsubToEvents()
        {
            PlayerInputs.player_l_click.canceled -= _onLeftClickUp;
        }

        private void OnDestroy()
        {
            this._unsubToEvents();
        }
    }

}