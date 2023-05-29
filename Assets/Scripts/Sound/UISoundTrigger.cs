using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DownBelow.Sound
{
    [RequireComponent(typeof(EventTrigger))]
    public class UISoundTrigger : MonoBehaviour
    {
        public EventTrigger TriggerHolder;
        private Selectable _selectable;
        private bool _isDragging = false;
        private bool _isSlider = false;

        public InputActionReference ClickEvent;

        public AK.Wwise.Event ClickSound;
        public AK.Wwise.Event HoverSound;
        public AK.Wwise.Event ValueChangedSound;

        private bool _inited = false;

        private void Start()
        {
            this._init();
        }

        /// <summary>
        /// To use when triggers have been cleared before so we need to set these again
        /// </summary>
        public void ForceReset()
        {
            this._inited = false;
            this._init();
        }
        private void _init()
        {
            if (this._inited)
                return;
            this._inited = true;

            if (this._selectable == null)
                this._selectable = this.GetComponent<Selectable>();

            if (this.TriggerHolder == null)
                this.TriggerHolder = this.GetComponent<EventTrigger>();

            if (this._selectable == null || this.TriggerHolder == null)
            {
                Debug.LogWarning("A SoundTrigger used for Selectable has been attached to a non-selectable component : " + this.name);
                return;
            }

            if (this.ValueChangedSound.WwiseObjectReference != null)
            {
                this._isSlider = true;

                EventTrigger.Entry beginentry = new EventTrigger.Entry();
                beginentry.eventID = EventTriggerType.Drag;
                beginentry.callback.AddListener((data) => { OnDragBegin((PointerEventData)data); });

                EventTrigger.Entry endentry = new EventTrigger.Entry();
                endentry.eventID = EventTriggerType.EndDrag;
                endentry.callback.AddListener((data) => { OnDragEnd((PointerEventData)data); });

                this.TriggerHolder.triggers.Add(beginentry);
                this.TriggerHolder.triggers.Add(endentry);

                if (this._selectable != null)
                {
                    if (this._selectable.GetType() == typeof(Slider))
                    {
                        ((Slider)this._selectable).onValueChanged.AddListener((value) => this.OnSliderValueChanged(value));
                    }
                }
            }
            if (this.ClickSound != null)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((data) => { OnPointerClick((PointerEventData)data); });

                this.TriggerHolder.triggers.Add(entry);

                if (ClickEvent != null)
                {
                    ClickEvent.action.performed += this._onPointerClick;
                }
            }
            if (this.HoverSound != null)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((data) => { OnPointerHover((PointerEventData)data); });

                this.TriggerHolder.triggers.Add(entry);
            }
        }

        private void _onPointerClick(InputAction.CallbackContext ctx) => OnPointerClick(null);
        public void OnPointerClick(PointerEventData eventData)
        {
            if (this.ClickSound != null && this._selectable.interactable)
                this.ClickSound.Post(AudioHolder.Instance.gameObject);
        }

        public void OnPointerHover(PointerEventData eventData)
        {
            // If we're dragging, do not play the over sound again
            if (this._isSlider && this._isDragging)
                return;

            if (this.HoverSound != null && this._selectable.interactable)
                this.HoverSound.Post(AudioHolder.Instance.gameObject);
        }
        public void OnDragBegin(PointerEventData data)
        {
            this._isDragging = true;
        }
        public void OnDragEnd(PointerEventData data)
        {
            this._isDragging = false;
        }
        public void OnSliderValueChanged(float value)
        {
            if (this.ValueChangedSound != null && this._selectable.interactable)
                this.ValueChangedSound.Post(AudioHolder.Instance.gameObject);
        }
    }
}
