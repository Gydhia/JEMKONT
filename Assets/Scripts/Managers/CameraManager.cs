using System;
using Cinemachine;
using DownBelow.Entity;
using DownBelow.Events;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace DownBelow.Managers
{
    public class CameraManager : _baseManager<CameraManager>
    {
        [SerializeField] private CinemachineVirtualCamera VirtualCamera;
        [SerializeField] private Camera MainCamera;

        [SerializeField] private float _zoomCoefficient = 0.3f;
        [SerializeField] private float _maxZoomVirtualCam = 20f;
        [SerializeField] private float _minZoomVirtualCam = 15f;
        [SerializeField] private float _minZoomMainCam = 20f;
        [SerializeField] private float _maxZoomMainCam = 30f;

        private CinemachineTransposer _transposer;
        
        private void Start()
        {
            _transposer = VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        }

        public void AttachPlayerToCamera(PlayerBehavior player)
        {
            this.VirtualCamera.Follow = player.gameObject.transform;
            this.VirtualCamera.LookAt = player.gameObject.transform;
            Vector3 pos = this.VirtualCamera.transform.position;
            this.VirtualCamera.transform.DOMove(new Vector3(pos.x, 17f, pos.z), 0f);
            GameManager.Instance.OnEnteredGrid += this._setupCamera;
        }

        private void _setupCamera(EntityEventData Data)
        {
            if (GameManager.Instance.SelfPlayer == Data.Entity)
            {
                if (GameManager.Instance.SelfPlayer.CurrentGrid.IsCombatGrid)
                {
                    this.VirtualCamera.Follow = null;

                    this.VirtualCamera.transform.position = new Vector3(52f, 25f, -46f);
                    this.VirtualCamera.transform.eulerAngles = new Vector3(70f, 0f, 0f);

                    StartCoroutine(WaitBeforeDisabling());
                }
                else
                {
                    this.VirtualCamera.gameObject.SetActive(true);
                    
                    this.VirtualCamera.Follow = Data.Entity.transform;
                    this.VirtualCamera.LookAt = Data.Entity.transform;

                }


            }

        }

        private void Update()
        {

            if (GameManager.Instance.SelfPlayer.CurrentGrid.IsCombatGrid)
            {
                Vector3 pos = this.MainCamera.transform.position;
                float newY = Input.mouseScrollDelta.y * (-_zoomCoefficient);
                
                if(pos.y + newY >= _minZoomMainCam && pos.y + newY <= _maxZoomMainCam)
                    MainCamera.transform.position = new Vector3(pos.x, pos.y + newY, pos.z);
                



            }
            else
            {
                Vector3 pos = this.VirtualCamera.transform.position;
                float newY = Input.mouseScrollDelta.y * (-_zoomCoefficient);

                if (_transposer.m_FollowOffset.y + newY >= _minZoomVirtualCam && _transposer.m_FollowOffset.y + newY <= _maxZoomVirtualCam)
                {
                    _transposer.m_FollowOffset = new Vector3(_transposer.m_FollowOffset.x,
                        _transposer.m_FollowOffset.y + newY, _transposer.m_FollowOffset.z);
                }



            }
            

        }

        private IEnumerator WaitBeforeDisabling()
        {
            yield return new WaitForSeconds(0.1f);
            this.VirtualCamera.gameObject.SetActive(false);
        }

    }
}
