using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Jemkont.UI
{

    public class UIMenuLobbyAnimations : MonoBehaviour
    {
        #region Fields
        [SerializeField] private UIMenuLobby _uIMenuLobby;

        [Header("Panels")]
        [SerializeField] private CanvasGroup _playerNamePanel;
        [SerializeField] private CanvasGroup _roomsPanel;
        [SerializeField] private CanvasGroup _lobbyPanel;



         private RectTransform _playerNamePanelTransform;
         private RectTransform _roomsPanelTransform;
         private RectTransform _lobbyPanelTransform;

        [SerializeField] private float _offset = 1.5f;

        private Vector4 _playerNamePanelAnchors;
        private Vector4 _roomsPanelAnchors;
        private Vector4 _lobbyPanelAnchors;

        private bool _isPlayerNamePanelHidden = true;
        private bool _isRoomPanelHidden = true;
        private bool _isLobbyPanelHidden = true;

        #endregion
        private void OnEnable()
        {
            _uIMenuLobby.OnPlayerNameValidated += OnPlayerNameValidated;
            _uIMenuLobby.OnRoomJoined += OnRoomJoined;
        }
        private void OnDisable()
        {
            _uIMenuLobby.OnPlayerNameValidated -= OnPlayerNameValidated;
            _uIMenuLobby.OnRoomJoined -= OnRoomJoined;
        }

        private void Start()
        {
            _playerNamePanelTransform = _playerNamePanel.GetComponent<RectTransform>();
            _roomsPanelTransform = _roomsPanel.GetComponent<RectTransform>();
            _lobbyPanelTransform = _lobbyPanel.GetComponent<RectTransform>();


            _playerNamePanelAnchors = new Vector4(_playerNamePanelTransform.anchorMin.x, _playerNamePanelTransform.anchorMin.y, _playerNamePanelTransform.anchorMax.x, _playerNamePanelTransform.anchorMax.y);
            _roomsPanelAnchors = new Vector4(_roomsPanelTransform.anchorMin.x, _roomsPanelTransform.anchorMin.y, _roomsPanelTransform.anchorMax.x, _roomsPanelTransform.anchorMax.y);
            _lobbyPanelAnchors = new Vector4(_lobbyPanelTransform.anchorMin.x, _lobbyPanelTransform.anchorMin.y, _lobbyPanelTransform.anchorMax.x, _lobbyPanelTransform.anchorMax.y);


            Init();
        }

        private void Init()
        {

            HideRoomPanel();
            HideLobbyPanel();
            _playerNamePanelTransform.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w - _offset), 0f).SetEase(Ease.OutQuint);
            _playerNamePanelTransform.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y - _offset), 0f).SetEase(Ease.OutQuint).OnComplete(() =>
            {
                _isPlayerNamePanelHidden = true;
                _playerNamePanel.alpha = 0;
                ShowPlayerNamePanel();
            });
        }


        #region PlayerNamePanel

        private void HidePlayerNamePanel(bool animated = true)
        {

            if (animated)
            {
                _playerNamePanelTransform.DOShakeAnchorPos(0.8f, 20f, 20).OnComplete(() =>
                {
                    _playerNamePanelTransform.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w + 0.08f), 0.4f).SetEase(Ease.InQuad);
                    _playerNamePanelTransform.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y + 0.08f), 0.4f).SetEase(Ease.InQuad).OnComplete(() =>
                    {
                        _playerNamePanelTransform.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w - _offset), 0.6f).SetEase(Ease.OutQuint);
                        _playerNamePanelTransform.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y - _offset), 0.6f).SetEase(Ease.OutQuint).OnComplete(() =>
                        {
                            _isPlayerNamePanelHidden = true;
                            _playerNamePanel.alpha = 0;
                        });
                    });


                });
            }
            else
            {
                _playerNamePanelTransform.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w - _offset), 0f).SetEase(Ease.OutQuint);
                _playerNamePanelTransform.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y - _offset), 0f).SetEase(Ease.OutQuint).OnComplete(() =>
                {
                    _isPlayerNamePanelHidden = true;
                    _playerNamePanel.alpha = 0;
                });
            }
        }



        private void ShowPlayerNamePanel(bool animated = true)
        {
            _playerNamePanel.alpha = 1;
            if (animated)
            {         
                _playerNamePanelTransform.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w), 0.4f).SetEase(Ease.OutQuad);
                _playerNamePanelTransform.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y), 0.4f).SetEase(Ease.OutQuad).OnComplete(() => _isPlayerNamePanelHidden = false);
            }
            else
            {
                _playerNamePanelTransform.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w), 0f).SetEase(Ease.OutQuad);
                _playerNamePanelTransform.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y), 0f).SetEase(Ease.OutQuad).OnComplete(() => _isPlayerNamePanelHidden = false);
            }

           
        }
        #endregion

        #region Rooms Panel
        private void HideRoomPanel(bool animated = true)
        {

            if (animated)
            {
                _roomsPanelTransform.DOShakeAnchorPos(0.8f, 20f, 20).OnComplete(() =>
                {
                    _roomsPanelTransform.DOAnchorMax(new Vector2(_roomsPanelAnchors.z, _roomsPanelAnchors.w + 0.08f), 0.4f).SetEase(Ease.InQuad);
                    _roomsPanelTransform.DOAnchorMin(new Vector2(_roomsPanelAnchors.x, _roomsPanelAnchors.y + 0.08f), 0.4f).SetEase(Ease.InQuad).OnComplete(() =>
                    {
                        _roomsPanelTransform.DOAnchorMax(new Vector2(_roomsPanelAnchors.z, _roomsPanelAnchors.w - _offset), 0.6f).SetEase(Ease.OutQuint);
                        _roomsPanelTransform.DOAnchorMin(new Vector2(_roomsPanelAnchors.x, _roomsPanelAnchors.y - _offset), 0.6f).SetEase(Ease.OutQuint).OnComplete(() =>
                        {
                            _isRoomPanelHidden = true;
                            _roomsPanel.alpha = 0;

                        });
                    });


                });
            }
            else
            {
                _roomsPanelTransform.DOAnchorMax(new Vector2(_roomsPanelAnchors.z, _roomsPanelAnchors.w - _offset), 0f).SetEase(Ease.OutQuint);
                _roomsPanelTransform.DOAnchorMin(new Vector2(_roomsPanelAnchors.x, _roomsPanelAnchors.y - _offset), 0f).SetEase(Ease.OutQuint).OnComplete(() =>
                {
                    _isRoomPanelHidden = true;
                    _roomsPanel.alpha = 0;
                    _roomsPanel.alpha = 0;
                });
            }
        }



        private void ShowRoomPanel(bool animated = true)
        {
            _roomsPanel.alpha = 1;
            if (animated)
            {
                _roomsPanelTransform.DOAnchorMax(new Vector2(_roomsPanelAnchors.z, _roomsPanelAnchors.w), 0.4f).SetEase(Ease.OutQuad);
                _roomsPanelTransform.DOAnchorMin(new Vector2(_roomsPanelAnchors.x, _roomsPanelAnchors.y), 0.4f).SetEase(Ease.OutQuad).OnComplete(() => _isRoomPanelHidden = false);
            }
            else
            {
                _roomsPanelTransform.DOAnchorMax(new Vector2(_roomsPanelAnchors.z, _roomsPanelAnchors.w), 0f).SetEase(Ease.OutQuad);
                _roomsPanelTransform.DOAnchorMin(new Vector2(_roomsPanelAnchors.x, _roomsPanelAnchors.y), 0f).SetEase(Ease.OutQuad).OnComplete(() => _isRoomPanelHidden = false);
            }


        }

        #endregion

        #region Lobby
        private void HideLobbyPanel(bool animated = true)
        {

            if (animated)
            {
                _lobbyPanelTransform.DOShakeAnchorPos(0.8f, 20f, 20).OnComplete(() =>
                {
                    _lobbyPanelTransform.DOAnchorMax(new Vector2(_lobbyPanelAnchors.z, _lobbyPanelAnchors.w + 0.08f), 0.4f).SetEase(Ease.InQuad);
                    _lobbyPanelTransform.DOAnchorMin(new Vector2(_lobbyPanelAnchors.x, _lobbyPanelAnchors.y + 0.08f), 0.4f).SetEase(Ease.InQuad).OnComplete(() =>
                    {
                        _lobbyPanelTransform.DOAnchorMax(new Vector2(_lobbyPanelAnchors.z, _lobbyPanelAnchors.w - _offset), 0.6f).SetEase(Ease.OutQuint);
                        _lobbyPanelTransform.DOAnchorMin(new Vector2(_lobbyPanelAnchors.x, _lobbyPanelAnchors.y - _offset), 0.6f).SetEase(Ease.OutQuint).OnComplete(() =>
                        {
                            _isLobbyPanelHidden = true;
                            _lobbyPanel.alpha = 0;

                        });
                    });


                });
            }
            else
            {
                _lobbyPanelTransform.DOAnchorMax(new Vector2(_lobbyPanelAnchors.z, _lobbyPanelAnchors.w - _offset), 0f).SetEase(Ease.OutQuint);
                _lobbyPanelTransform.DOAnchorMin(new Vector2(_lobbyPanelAnchors.x, _lobbyPanelAnchors.y - _offset), 0f).SetEase(Ease.OutQuint).OnComplete(() =>
                {
                    _isLobbyPanelHidden = true;
                    _lobbyPanel.alpha = 0;
                });
            }
        }



        private void ShowLobbyPanel(bool animated = true)
        {
            _lobbyPanel.alpha = 1;
            if (animated)
            {
                _lobbyPanelTransform.DOAnchorMax(new Vector2(_lobbyPanelAnchors.z, _lobbyPanelAnchors.w), 0.4f).SetEase(Ease.OutQuad);
                _lobbyPanelTransform.DOAnchorMin(new Vector2(_lobbyPanelAnchors.x, _lobbyPanelAnchors.y), 0.4f).SetEase(Ease.OutQuad).OnComplete(() => _isLobbyPanelHidden = false);
            }
            else
            {
                _lobbyPanelTransform.DOAnchorMax(new Vector2(_lobbyPanelAnchors.z, _lobbyPanelAnchors.w), 0f).SetEase(Ease.OutQuad);
                _lobbyPanelTransform.DOAnchorMin(new Vector2(_lobbyPanelAnchors.x, _lobbyPanelAnchors.y), 0f).SetEase(Ease.OutQuad).OnComplete(() => _isLobbyPanelHidden = false);
            }


        }
        #endregion

        private void OnPlayerNameValidated()
        {
            HidePlayerNamePanel();
            ShowRoomPanel();
        }

        private void OnRoomJoined()
        {
            HideRoomPanel();
            ShowLobbyPanel();
        }



        private void ResetAllPanels()
        {

        }

    }

}
