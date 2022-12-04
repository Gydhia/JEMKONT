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
        [SerializeField] private RectTransform _playerNamePanel;
        [SerializeField] private RectTransform _roomsPanel;
        [SerializeField] private RectTransform _lobbyPanel;

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
        }
        private void OnDisable()
        {
            _uIMenuLobby.OnPlayerNameValidated -= OnPlayerNameValidated;
        }

        private void Start()
        {
            _playerNamePanelAnchors = new Vector4(_playerNamePanel.anchorMin.x, _playerNamePanel.anchorMin.y, _playerNamePanel.anchorMax.x, _playerNamePanel.anchorMax.y);
            _roomsPanelAnchors = new Vector4(_roomsPanel.anchorMin.x, _roomsPanel.anchorMin.y, _roomsPanel.anchorMax.x, _roomsPanel.anchorMax.y);
            _lobbyPanelAnchors = new Vector4(_lobbyPanel.anchorMin.x, _lobbyPanel.anchorMin.y, _lobbyPanel.anchorMax.x, _lobbyPanel.anchorMax.y);


            Init();
        }

        private void Init()
        {

            HideRoomPanel();
            HideLobbyPanel();
            _playerNamePanel.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w - _offset), 0f).SetEase(Ease.OutQuint);
            _playerNamePanel.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y - _offset), 0f).SetEase(Ease.OutQuint).OnComplete(() =>
            {
                _isPlayerNamePanelHidden = true;
                _playerNamePanel.gameObject.SetActive(false);
                ShowPlayerNamePanel();
            });
        }


        #region PlayerNamePanel

        private void HidePlayerNamePanel(bool animated = true)
        {

            if (animated)
            {
                _playerNamePanel.DOShakeAnchorPos(0.8f, 20f, 20).OnComplete(() =>
                {
                    _playerNamePanel.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w + 0.08f), 0.4f).SetEase(Ease.InQuad);
                    _playerNamePanel.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y + 0.08f), 0.4f).SetEase(Ease.InQuad).OnComplete(() =>
                    {
                        _playerNamePanel.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w - _offset), 0.6f).SetEase(Ease.OutQuint);
                        _playerNamePanel.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y - _offset), 0.6f).SetEase(Ease.OutQuint).OnComplete(() =>
                        {
                            _isPlayerNamePanelHidden = true;
                            _playerNamePanel.gameObject.SetActive(false);

                        });
                    });


                });
            }
            else
            {
                _playerNamePanel.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w - _offset), 0f).SetEase(Ease.OutQuint);
                _playerNamePanel.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y - _offset), 0f).SetEase(Ease.OutQuint).OnComplete(() =>
                {
                    _isPlayerNamePanelHidden = true;
                    _playerNamePanel.gameObject.SetActive(false);
                });
            }
        }



        private void ShowPlayerNamePanel(bool animated = true)
        {

            if (animated)
            {
                _playerNamePanel.gameObject.SetActive(true);

                _playerNamePanel.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w), 0.4f).SetEase(Ease.OutQuad);
                _playerNamePanel.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y), 0.4f).SetEase(Ease.OutQuad).OnComplete(() => _isPlayerNamePanelHidden = false);
            }
            else
            {
                _playerNamePanel.gameObject.SetActive(true);

                _playerNamePanel.DOAnchorMax(new Vector2(_playerNamePanelAnchors.z, _playerNamePanelAnchors.w), 0f).SetEase(Ease.OutQuad);
                _playerNamePanel.DOAnchorMin(new Vector2(_playerNamePanelAnchors.x, _playerNamePanelAnchors.y), 0f).SetEase(Ease.OutQuad).OnComplete(() => _isPlayerNamePanelHidden = false);
            }

           
        }
        #endregion

        #region Rooms Panel
        private void HideRoomPanel(bool animated = true)
        {

            if (animated)
            {
                _roomsPanel.DOShakeAnchorPos(0.8f, 20f, 20).OnComplete(() =>
                {
                    _roomsPanel.DOAnchorMax(new Vector2(_roomsPanelAnchors.z, _roomsPanelAnchors.w + 0.08f), 0.4f).SetEase(Ease.InQuad);
                    _roomsPanel.DOAnchorMin(new Vector2(_roomsPanelAnchors.x, _roomsPanelAnchors.y + 0.08f), 0.4f).SetEase(Ease.InQuad).OnComplete(() =>
                    {
                        _roomsPanel.DOAnchorMax(new Vector2(_roomsPanelAnchors.z, _roomsPanelAnchors.w - _offset), 0.6f).SetEase(Ease.OutQuint);
                        _roomsPanel.DOAnchorMin(new Vector2(_roomsPanelAnchors.x, _roomsPanelAnchors.y - _offset), 0.6f).SetEase(Ease.OutQuint).OnComplete(() =>
                        {
                            _isRoomPanelHidden = true;
                            _roomsPanel.gameObject.SetActive(false);

                        });
                    });


                });
            }
            else
            {
                _roomsPanel.DOAnchorMax(new Vector2(_roomsPanelAnchors.z, _roomsPanelAnchors.w - _offset), 0f).SetEase(Ease.OutQuint);
                _roomsPanel.DOAnchorMin(new Vector2(_roomsPanelAnchors.x, _roomsPanelAnchors.y - _offset), 0f).SetEase(Ease.OutQuint).OnComplete(() =>
                {
                    _isRoomPanelHidden = true;
                    _roomsPanel.gameObject.SetActive(false);
                });
            }
        }



        private void ShowRoomPanel(bool animated = true)
        {

            if (animated)
            {
                _roomsPanel.gameObject.SetActive(true);

                _roomsPanel.DOAnchorMax(new Vector2(_roomsPanelAnchors.z, _roomsPanelAnchors.w), 0.4f).SetEase(Ease.OutQuad);
                _roomsPanel.DOAnchorMin(new Vector2(_roomsPanelAnchors.x, _roomsPanelAnchors.y), 0.4f).SetEase(Ease.OutQuad).OnComplete(() => _isRoomPanelHidden = false);
            }
            else
            {
                _roomsPanel.gameObject.SetActive(true);

                _roomsPanel.DOAnchorMax(new Vector2(_roomsPanelAnchors.z, _roomsPanelAnchors.w), 0f).SetEase(Ease.OutQuad);
                _roomsPanel.DOAnchorMin(new Vector2(_roomsPanelAnchors.x, _roomsPanelAnchors.y), 0f).SetEase(Ease.OutQuad).OnComplete(() => _isRoomPanelHidden = false);
            }


        }

        #endregion

        #region Lobby
        private void HideLobbyPanel(bool animated = true)
        {

            if (animated)
            {
                _lobbyPanel.DOShakeAnchorPos(0.8f, 20f, 20).OnComplete(() =>
                {
                    _lobbyPanel.DOAnchorMax(new Vector2(_lobbyPanelAnchors.z, _lobbyPanelAnchors.w + 0.08f), 0.4f).SetEase(Ease.InQuad);
                    _lobbyPanel.DOAnchorMin(new Vector2(_lobbyPanelAnchors.x, _lobbyPanelAnchors.y + 0.08f), 0.4f).SetEase(Ease.InQuad).OnComplete(() =>
                    {
                        _lobbyPanel.DOAnchorMax(new Vector2(_lobbyPanelAnchors.z, _lobbyPanelAnchors.w - _offset), 0.6f).SetEase(Ease.OutQuint);
                        _lobbyPanel.DOAnchorMin(new Vector2(_lobbyPanelAnchors.x, _lobbyPanelAnchors.y - _offset), 0.6f).SetEase(Ease.OutQuint).OnComplete(() =>
                        {
                            _isLobbyPanelHidden = true;
                            _lobbyPanel.gameObject.SetActive(false);

                        });
                    });


                });
            }
            else
            {
                _lobbyPanel.DOAnchorMax(new Vector2(_lobbyPanelAnchors.z, _lobbyPanelAnchors.w - _offset), 0f).SetEase(Ease.OutQuint);
                _lobbyPanel.DOAnchorMin(new Vector2(_lobbyPanelAnchors.x, _lobbyPanelAnchors.y - _offset), 0f).SetEase(Ease.OutQuint).OnComplete(() =>
                {
                    _isLobbyPanelHidden = true;
                    _lobbyPanel.gameObject.SetActive(false);
                });
            }
        }



        private void ShowLobbyPanel(bool animated = true)
        {

            if (animated)
            {
                _lobbyPanel.gameObject.SetActive(true);

                _lobbyPanel.DOAnchorMax(new Vector2(_lobbyPanelAnchors.z, _lobbyPanelAnchors.w), 0.4f).SetEase(Ease.OutQuad);
                _lobbyPanel.DOAnchorMin(new Vector2(_lobbyPanelAnchors.x, _lobbyPanelAnchors.y), 0.4f).SetEase(Ease.OutQuad).OnComplete(() => _isLobbyPanelHidden = false);
            }
            else
            {
                _lobbyPanel.gameObject.SetActive(true);

                _lobbyPanel.DOAnchorMax(new Vector2(_lobbyPanelAnchors.z, _lobbyPanelAnchors.w), 0f).SetEase(Ease.OutQuad);
                _lobbyPanel.DOAnchorMin(new Vector2(_lobbyPanelAnchors.x, _lobbyPanelAnchors.y), 0f).SetEase(Ease.OutQuad).OnComplete(() => _isLobbyPanelHidden = false);
            }


        }
        #endregion

        private void OnPlayerNameValidated()
        {
            HidePlayerNamePanel();
            ShowRoomPanel();
        }



        private void ResetAllPanels()
        {

        }

    }

}
