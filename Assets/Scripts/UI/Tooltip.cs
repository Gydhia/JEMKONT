using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class Tooltip : MonoBehaviour, IPointerExitHandler
    {
        public TMPro.TextMeshProUGUI Text;
        public TMPro.TextMeshProUGUI Title;
        public RectTransform RectTransform;
        public ContentSizeFitter SelfSizeFitter;
        public Image Background;
        private DateTime? _hidetime = null;

        public void Awake()
        {
            if (this.RectTransform == null)
                this.RectTransform = this.GetComponent<RectTransform>();
            if (this.SelfSizeFitter == null)
                this.SelfSizeFitter = this.GetComponent<ContentSizeFitter>();
            this.gameObject.SetActive(false);
        }
        public void Update()
        {
            if (_hidetime.HasValue && DateTime.Now > _hidetime.Value)
                this.Hide();
        }
        public void SetAdditional(string Title, int TitleSize)
        {
            if (!string.IsNullOrEmpty(Title))
            {
                this.Title.text = Title;
                this.Title.fontSize = TitleSize;
                this.Title.gameObject.SetActive(true);
            }
            else
                this.Title.gameObject.SetActive(false);
        }

        /// <summary>
        /// Sets the content of the Tooltip.
        /// </summary>
        /// <param name="ScreenPosition">Position on screen. Usually calculated around the mouse</param>
        /// <param name="AnchorPivot">Where to place the pivot of the tooltip</param>
        /// <param name="Size">Sets the Size delta.</param>
        /// <param name="Text"></param>
        /// <param name="Duration"></param>
        /// <param name="BackgroundColor"></param>
        /// <param name="AutoSize">Optional. If set to true, adjusts the tooltip to its horizontal size.</param>
        public void Set(Vector2 ScreenPosition, Vector2? AnchorPivot = null, Vector2? Size = null, string Text = null, double Duration = 8, Color? BackgroundColor = null, bool AutoSize = false)
        {
            this.Text.text = Text;

            if (this.SelfSizeFitter != null)
                this.SelfSizeFitter.horizontalFit = AutoSize ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;

            if (AnchorPivot.HasValue)
                this.RectTransform.pivot = AnchorPivot.Value;

            this.RectTransform.anchoredPosition = ScreenPosition;
            if (Size.HasValue)
                this.RectTransform.sizeDelta = Size.Value;
            // fit to screen
            if (this.RectTransform.rect.xMax > Screen.width)
                RectTransform.position -= Vector3.right * (Screen.width - this.RectTransform.rect.xMax);
            if (this.RectTransform.rect.yMax > Screen.height)
                RectTransform.position -= Vector3.up * (Screen.height - this.RectTransform.rect.yMax);

            // Set color if specified
            if (this.Background != null && BackgroundColor.HasValue)
            {
                this.Background.color = BackgroundColor.Value;
            }

            this.gameObject.SetActive(true);
            _hidetime = DateTime.Now.AddSeconds(Duration);
        }


        public void Hide()
        {
            this.gameObject.SetActive(false);
            this._hidetime = null;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            this.Hide();
        }
    }

    public enum TootipSide
    {
        Top,
        Right,
        Bottom,
        Left,
        LeftLow,
        RightLow,
        Over,
        LeftHigh
    }
}
