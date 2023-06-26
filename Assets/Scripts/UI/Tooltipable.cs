using DownBelow.Managers;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DownBelow.UI
{
    public class Tooltipable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        [Header("Tooltip settings")]
        [Tooltip("If true, the tooltip size will adjust to the text")]
        public bool AutoSize = false;

        [Tooltip("Side from which the tooltip will appear")]
        public TootipSide Side;
        [Tooltip("Distance in pixels from the hovered element, on the side set up there")]
        public float distance = 6;
        [Tooltip("Distance in pixels on the other axis from the given side (right or up)")]
        public float perp_distance = 0;
        [Tooltip("Text to show")]
        public string Text;
        [Tooltip("Title to diplay over the text")]
        public string Title;
        [Tooltip("Overrides the title size")]
        public int TitleSize = 36;
        [Tooltip("Target width of the tooltipable")]
        public float MinWidth = 200;

        [Tooltip("Delay in seconds to wait before showing the tooltip")]
        public float delay = 0;


        public RectTransform UIRect;
        private Coroutine _showCoroutine;

        public void Start()
        {
            this.UIRect = this.GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            this._showCoroutine = StartCoroutine(this.OnPointerEnterAsync());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (this._showCoroutine != null)
                StopCoroutine(this._showCoroutine);

            if (UIManager.Instance.UITooltip != null)
                UIManager.Instance.UITooltip.Hide();
        }

        public IEnumerator OnPointerEnterAsync()
        {
            if (this == null)
                yield break;

            if (string.IsNullOrEmpty(this.Title) && string.IsNullOrEmpty(this.Text))
                yield break;

            if (this.delay > 0)
                yield return new WaitForSeconds(this.delay);

            // the object may have been destroyed meanwhile
            if (this != null)
            {
               
                Rect rect_absolute = new Rect(this.UIRect.position.x + this.UIRect.rect.x, this.UIRect.position.y + this.UIRect.rect.y, this.UIRect.rect.width, this.UIRect.rect.height);

                Vector2 pivot = Vector2.zero, position = Vector2.zero;
                Vector2 size = new Vector2(this.MinWidth, 1);
                switch (this.Side)
                {
                    case TootipSide.Top:
                        pivot = new Vector2(0.5f, 0f);
                        position = rect_absolute.center
                            + Vector2.up * (distance + rect_absolute.height / 2f)
                            + Vector2.right * perp_distance;
                        break;
                    case TootipSide.Bottom:
                        pivot = new Vector2(0.5f, 1f);
                        position = rect_absolute.center
                            + Vector2.down * (distance + rect_absolute.height / 2f)
                            + Vector2.right * perp_distance;
                        break;
                    case TootipSide.Left:
                        pivot = new Vector2(1f, 0.5f);
                        position = rect_absolute.center
                            + Vector2.left * (distance + rect_absolute.width / 2f)
                            + Vector2.up * perp_distance;
                        break;
                    case TootipSide.Right:
                        pivot = new Vector2(0f, 0.5f);
                        position = rect_absolute.center
                            + Vector2.right * (distance + rect_absolute.width / 2f)
                            + Vector2.up * perp_distance;
                        break;
                    case TootipSide.RightLow:
                        pivot = new Vector2(0f, 1f);
                        position = rect_absolute.center
                            + Vector2.right * (distance + rect_absolute.width / 2f)
                            + Vector2.up * (distance + rect_absolute.height / 2f)
                            + Vector2.up * perp_distance;
                        break;
                    case TootipSide.LeftLow:
                        pivot = new Vector2(1f, 1f);
                        position = rect_absolute.center
                            + Vector2.left * (distance + rect_absolute.width / 2f)
                            + Vector2.up * (distance + rect_absolute.height / 2f)
                            + Vector2.up * perp_distance;
                        break;
                    case TootipSide.LeftHigh:
                        pivot = new Vector2(0f, 0f);
                        position = rect_absolute.center
                            + Vector2.up * (distance + rect_absolute.height / 2f)
                            + Vector2.left * (distance + rect_absolute.width / 2f)
                            + Vector2.right * perp_distance;
                        break;
                    default:
                        position = rect_absolute.center;
                        break;
                }
                UIManager.Instance.UITooltip.Set(position, pivot, size, this.Text, AutoSize: this.AutoSize);
                UIManager.Instance.UITooltip.SetAdditional(this.Title, TitleSize);
            }
        }

        private void OnDisable()
        {
            this.OnDestroy();
        }

        public void OnDestroy()
        {
            if (UIManager.Instance != null && UIManager.Instance.UITooltip != null)
                UIManager.Instance.UITooltip.Hide();
        }
    }




}
