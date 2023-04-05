using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace DownBelow.UI
{
    public class DraggedCard : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
    {
        public static DraggedCard SelectedCard;

        private static DraggedCard _hoveredCard;
        public static DraggedCard HoveredCard
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

        private Coroutine _followCoroutine = null;
        private Coroutine _compareCoroutine = null;
        private Coroutine _pinUpdateCoroutine = null;

        private void Start()
        {
            this.m_RectTransform = this.GetComponent<RectTransform>();
            this.CardVisual ??= this.GetComponent<CardVisual>();

            this._subToEvents();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                this._onLeftClickDown();
        }

        // Player is trying to drag the card
        private void _onLeftClickDown(InputAction.CallbackContext ctx) => this._onLeftClickDown();
        private void _onLeftClickDown()
        {
            if (HoveredCard == this)
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
            if (SelectedCard != this)
                return;

            if (this._followCoroutine != null)
                StopCoroutine(this._followCoroutine);
            this._followCoroutine = null;

            if (Mouse.current.position.ReadValue().y / Screen.height > this.BottomDeadPercents / 100f)
            {
                this.PinCardToScreen();
            }
            else
            {
                this.IsDragged = false;
                this._onRightClick();
            }
        }

        private void _onRightClick(InputAction.CallbackContext ctx) => this._onRightClick();
        private void _onRightClick()
        {
            if (this._pinUpdateCoroutine != null)
                StopCoroutine(this._pinUpdateCoroutine);
            this.DiscardToPile();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HoveredCard = this;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (HoveredCard == this)
                HoveredCard = null;
        }

        public void StartDrag()
        {
            this.IsDragged = true;

            if (this._compareCoroutine != null)
                StopCoroutine(this._compareCoroutine);
            this._compareCoroutine = null;

            this._followCoroutine = StartCoroutine(this._followCursor());
        }
        public void PinCardToScreen()
        {
            this.PinnedToScreen = true;
            this.PinnedLeft = true;

            this._pinUpdateCoroutine = StartCoroutine(_updatePinnedPosition());
        }

        public void DrawFromPile()
        {

        }

        public void DiscardToPile()
        {
            SelectedCard = null;
        }

        public void Burn()
        {
            // TEMPORARY
            Destroy(this.gameObject, 2f);
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

        private void _pinInverse()
        {
            this.PinnedLeft = !this.PinnedLeft;

            this.m_RectTransform.parent = this.PinnedLeft ?
                UIManager.Instance.CombatSection.LeftPin :
                UIManager.Instance.CombatSection.RightPin;

            this.m_RectTransform.localPosition = Vector2.zero;
        }

        private void _subToEvents()
        {
            PlayerInputs.player_l_click.performed += _onLeftClickDown;
            PlayerInputs.player_l_click.canceled += _onLeftClickUp;

            PlayerInputs.player_r_click.canceled += _onRightClick;
        }

        private void _unsubToEvents()
        {
            PlayerInputs.player_l_click.performed -= _onLeftClickDown;
            PlayerInputs.player_l_click.canceled -= _onLeftClickUp;

            PlayerInputs.player_r_click.canceled -= _onRightClick;
        }

        private void OnDestroy()
        {
            this._unsubToEvents();
        }
    }

}