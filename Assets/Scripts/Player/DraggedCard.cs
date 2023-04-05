using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggedCard : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
{
    public static DraggedCard SelectedCard;
    public static DraggedCard HoveredCard;

    // In %, which part of the bottom screen won't Pin the card ?
    public int PinUpdatePercents = 33;
    public int BottomDeadPercents = 20;
    public float DistanceToDrag = 5f;
    public float FollowSensivity = 0.12f;

    public bool IsDragged = false;
    public bool PinnedToScreen = false;
    public bool PinnedLeft = true;

    private RectTransform m_RectTransform;
    // Cached when card clicked. Reference 
    private Vector2 _startDragPos;

    private Coroutine _followCoroutine = null;
    private Coroutine _compareCoroutine = null;
    private Coroutine _pinUpdateCoroutine = null;

    private void Start()
    {
        this.m_RectTransform = this.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(1))
        {
            this._onRightClick();
        }
        if (Input.GetMouseButtonUp(0))
        {
            this._onLeftClickUp();
        }
    }

    public void Init()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            this._onLeftClickDown();
    }

    // Player is trying to drag the card
    private void _onLeftClickDown()
    {
        if(HoveredCard == this)
        {
            SelectedCard = this;
            this._startDragPos = Input.mousePosition;

            this._compareCoroutine = StartCoroutine(this._compareDistanceToStartFollow());
        }
    }

    // Player released the card.
    private void _onLeftClickUp()
    {
        if(this._followCoroutine != null)
            StopCoroutine(this._followCoroutine);
        this._followCoroutine = null;

        if (Input.mousePosition.y / Screen.height > this.BottomDeadPercents / 100f)
        {
            this.PinCardToScreen();
        }
        else
        {
            this.IsDragged = false;
            this._onRightClick();
        }
    }

    private void _onRightClick()
    {
        if(this._pinUpdateCoroutine != null)
            StopCoroutine(this._pinUpdateCoroutine);
        this.GoBackToPile();
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

        if(this._compareCoroutine != null)
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

    public void GoBackToPile()
    {
        SelectedCard = null;
    }

    private IEnumerator _compareDistanceToStartFollow()
    {
        while (!this.IsDragged)
        {
            if(Vector2.Distance(this._startDragPos, Input.mousePosition) > this.DistanceToDrag)
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
                Mathf.Lerp(lastPos.x, Input.mousePosition.x, timer / this.FollowSensivity),
                Mathf.Lerp(lastPos.y, Input.mousePosition.y, timer / this.FollowSensivity)
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
        while (true)
        {
            yield return null;

            if(this.PinnedLeft ?
                Input.mousePosition.x / Screen.width < this.PinUpdatePercents / 100f :
                Input.mousePosition.x / Screen.width > 1f - (this.PinUpdatePercents / 100f))
            {
                // Temporary values, create anchors later on
                this.PinnedLeft = !this.PinnedLeft;
                this.m_RectTransform.position = new Vector3(
                    this.PinnedLeft ? Screen.width / 10f : Screen.width - (Screen.width / 10f),
                    0f,
                    Screen.height / 10f
                );
            }
        }
    }
}
