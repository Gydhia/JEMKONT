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
        [SerializeField]
        private CinemachineVirtualCamera VirtualCamera;
        [SerializeField]
        private Camera MainCamera;

        [SerializeField] private float _zoomCoefficient = 0.3f;
        [SerializeField] private float _maxZoom = 20f;
        [SerializeField] private float _minZoom = 15f;

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

                    this.VirtualCamera.transform.position = new Vector3(52f, 25f, -45f);
                    this.VirtualCamera.transform.eulerAngles = new Vector3(70f, 0f, 0f);
                }
                else
                {
                    this.VirtualCamera.Follow = Data.Entity.transform;
                    this.VirtualCamera.LookAt = Data.Entity.transform;
                    
                    Vector3 pos = this.VirtualCamera.transform.position;
                    
                }

                
            }
   
        }

        private void Update()
        {
            Vector3 pos = this.VirtualCamera.transform.position;
            float newY = Input.mouseScrollDelta.y * _zoomCoefficient;
            Debug.Log(pos.y + newY);
            if (_transposer.m_FollowOffset.y + newY >= _minZoom && _transposer.m_FollowOffset.y + newY <= _maxZoom)
            {
                Debug.Log("test");
                _transposer.m_FollowOffset = new Vector3(_transposer.m_FollowOffset.x,
                    _transposer.m_FollowOffset.y + newY, _transposer.m_FollowOffset.z);
            }


        }
    }

}
