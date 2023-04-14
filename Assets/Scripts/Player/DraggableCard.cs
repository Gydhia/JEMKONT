using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DownBelow.Mechanics;
using System.Linq;
using DG.Tweening;

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

        // In %, which part of the bottom screen won't Pin the card ?
        public int PinUpdatePercents = 33;
        public int BottomDeadPercents = 20;
        public float DistanceToDrag = 5f;
        public float FollowSensivity = 0.12f;

        public bool IsDragged = false;
        public bool PinnedToScreen = false;
        public bool PinnedLeft = false;
        public bool PinnedForMultipleActions = false;

        private RectTransform m_RectTransform;
        // Cached when card clicked. Reference 
        private Vector2 _startDragPos;
        // TODO: Temporary, later on it'll be handled by the card pile
        private Vector2 _spawnPosition;

        private Coroutine _followCoroutine = null;
        private Coroutine _compareCoroutine = null;
        private Coroutine _pinUpdateCoroutine = null;

        public void Init(ScriptableCard CardReference)
        {
            this.m_RectTransform = this.GetComponent<RectTransform>();
            this.CardVisual ??= this.GetComponent<CardVisual>();

            this.CardReference = CardReference;
            this.CardVisual.Init(CardReference);

           // this.m_RectTransform.localPosition= new Vector2(Random.Range(50, 1500) ,m_RectTransform.position.y);
            this._spawnPosition = m_RectTransform.position;  
            this.m_RectTransform.DOLocalMoveY(this._spawnPosition.y, 0.3f);
            this.PinnedForMultipleActions = this.CardReference.Spells.Where(s => s.RequiresTargetting).Count() > 1;

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
            // Forbid the card drag if currently using another one
            if (HoveredCard == this && SelectedCard == null)
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
            if (SelectedCard != this || this.PinnedToScreen)
                return;

            this._abortCoroutine(ref this._followCoroutine);
            Debug.Log("Left click on card " + this.name.ToString());

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
            this.DiscardToPile();
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
          //  UIManager.Instance.CardSection.UpdateLayoutGroup();
            
            // Do we require to pin the card for 2 or more inputs ?
            if (this.PinnedForMultipleActions)
            {
                this.IsDragged = true;
                this._followCoroutine = StartCoroutine(this._followCursor());
            }
            else
            {
                // TODO : Instead of brutally pinning it right away, make it go to the cursor then fade to pin
                this.PinCardToScreen();
            }
        }

        private void Hover()
        {
            HoveredCard.m_RectTransform.DOLocalMoveY(HoveredCard._spawnPosition.y + 100f, 0.3f);
        }

        private void UnHover()
        {
            HoveredCard.m_RectTransform.DOLocalMoveY(HoveredCard._spawnPosition.y, 0.3f);
        }
        
        public void PinCardToScreen()
        {

            Debug.Log("Pinned card to screen " + this.name.ToString());
            this.PinnedToScreen = true;
            this.PinnedLeft = true;

            CombatManager.Instance.FireCardBeginUse(this.CardReference);

            this._pinUpdateCoroutine = StartCoroutine(_updatePinnedPosition());
        }

        public void DrawFromPile()
        {

        }

        public void DiscardToPile()
        {
            this._abortCoroutine(ref this._pinUpdateCoroutine);
            this.PinnedToScreen = this.IsDragged = false;
            this.m_RectTransform.parent = UIManager.Instance.CardSection.CardsHolder.transform;
            
            

            // TODO : smoothly go back
          //  this.m_RectTransform.position = this._spawnPosition;
            this.m_RectTransform.DOMove(this._spawnPosition, 0.3f).SetEase(Ease.OutQuad);
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