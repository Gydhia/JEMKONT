using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Jemkont.UI
{

    public class UIMenuLobbyAnimations : MonoBehaviour
    {
        #region Fields
        [Header("Panels")]
        [SerializeField] private RectTransform _playerNamePanel;

        [SerializeField] private float _offset = 1.5f;

        private Vector4 _playerNamePanelAnchors;
        #endregion
        private void Start()
        {
            _playerNamePanelAnchors = new Vector4(_playerNamePanel.anchorMin.x, _playerNamePanel.anchorMin.y, _playerNamePanel.anchorMax.x, _playerNamePanel.anchorMax.y);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                HidePlayerNamePanel();
            }
        }

        private void HidePlayerNamePanel()
        {
            _playerNamePanel.DOShakeAnchorPos(0.6f, 50f).OnComplete(() =>
            {
                _playerNamePanel.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z , _playerNamePanelAnchors.w - _offset), 0.4f).SetEase(Ease.OutQuad);
                _playerNamePanel.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x , _playerNamePanelAnchors.y - _offset), 0.4f).SetEase(Ease.OutQuad);
            });
        }

        private void ShowPlayerNamePanel()
        {
            _playerNamePanel.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w ), 0.4f).SetEase(Ease.OutQuad);
            _playerNamePanel.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y ), 0.4f).SetEase(Ease.OutQuad);
        }

        private void ResetAllPanels()
        {
            
        }

    }

}
