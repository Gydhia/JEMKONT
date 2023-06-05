using DG.Tweening;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.UI.Menu
{
    public class BaseMenuPopup : MonoBehaviour
    {
        public MenuPopup PopupType;
        public bool IsHidden = true;

        public virtual void Init()
        {
            _selfCanvasGroup = this.GetComponent<CanvasGroup>();
            _selfRectTransform = this.GetComponent<RectTransform>();

            _selfPanelAnchors = new Vector4(_selfRectTransform.anchorMin.x, _selfRectTransform.anchorMin.y, _selfRectTransform.anchorMax.x, _selfRectTransform.anchorMax.y);
        }

        public virtual void ShowPopup()
        {
            this.ShowSelfPanel();
        }

        public virtual void HidePopup()
        {
            this.HideSelfPanel();
        }

        #region Fields

        private CanvasGroup _selfCanvasGroup;
        private RectTransform _selfRectTransform;

        private float _offset = 1.5f;
        private Vector4 _selfPanelAnchors;

        #endregion

        private void HideSelfPanel(bool animated = true)
        {
            if (animated)
            {
               // _selfRectTransform.DOShakeAnchorPos(1.1f, 20f, 20);
                
                _selfRectTransform.DOAnchorMax(new Vector2(_selfPanelAnchors.z, _selfPanelAnchors.w + 0.08f), 0.2f).SetEase(Ease.InQuad);
                _selfRectTransform.DOAnchorMin(new Vector2(_selfPanelAnchors.x, _selfPanelAnchors.y + 0.08f), 0.2f).SetEase(Ease.InQuad).OnComplete(() =>
                {
                    _selfRectTransform.DOAnchorMax(new Vector2(_selfPanelAnchors.z, _selfPanelAnchors.w - _offset), 0.8f).SetEase(Ease.OutQuint);
                    _selfRectTransform.DOAnchorMin(new Vector2(_selfPanelAnchors.x, _selfPanelAnchors.y - _offset), 0.8f).SetEase(Ease.OutQuint).OnComplete(() =>
                    {
                        IsHidden = true;
                        this.gameObject.SetActive(false);
                        _selfCanvasGroup.alpha = 0;
                        if (this.PopupType != MenuManager.Instance.LastPopup)
                        {
                            MenuManager.Instance.ShowNextPopup();
                        }
                    });
                });
            }
            else
            {
                _selfRectTransform.DOAnchorMax(new Vector2(_selfPanelAnchors.z, _selfPanelAnchors.w - _offset), 0f).SetEase(Ease.OutQuint);
                _selfRectTransform.DOAnchorMin(new Vector2(_selfPanelAnchors.x, _selfPanelAnchors.y - _offset), 0f).SetEase(Ease.OutQuint).OnComplete(() =>
                {
                    IsHidden = true;
                    this.gameObject.SetActive(false);
                    _selfCanvasGroup.alpha = 0;
                });
            }
        }

        private void ShowSelfPanel(bool animated = true)
        {
            this.gameObject.SetActive(true);

            _selfCanvasGroup.alpha = 1;
            if (animated)
            {
                _selfRectTransform.DOAnchorMax(new Vector2(_selfPanelAnchors.z, _selfPanelAnchors.w), 0.4f).SetEase(Ease.OutQuad);
                _selfRectTransform.DOAnchorMin(new Vector2(_selfPanelAnchors.x, _selfPanelAnchors.y), 0.4f).SetEase(Ease.OutQuad).OnComplete(() => IsHidden = false) ;
            }
            else
            {
                _selfRectTransform.DOAnchorMax(new Vector2(_selfPanelAnchors.z, _selfPanelAnchors.w), 0f).SetEase(Ease.OutQuad);
                _selfRectTransform.DOAnchorMin(new Vector2(_selfPanelAnchors.x, _selfPanelAnchors.y), 0f).SetEase(Ease.OutQuad).OnComplete(() => IsHidden = false);
            }
        }
    }

}