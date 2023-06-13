using Cinemachine;
using DownBelow.Entity;
using DownBelow.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Managers
{
    public class CameraManager : _baseManager<CameraManager>
    {
        [SerializeField]
        private CinemachineVirtualCamera VirtualCamera;
        [SerializeField]
        private Camera MainCamera;

        public void AttachPlayerToCamera(PlayerBehavior player)
        {
            this.VirtualCamera.Follow = player.gameObject.transform;

            GameManager.Instance.OnEnteredGrid += this._setupCamera;
        }

        private void _setupCamera(EntityEventData Data)
        {
            if (GameManager.RealSelfPlayer == Data.Entity)
            {
                if (GameManager.RealSelfPlayer.CurrentGrid.IsCombatGrid) 
                {
                    this.VirtualCamera.Follow = null;

                    this.VirtualCamera.transform.position = new Vector3(61.28f, 25f, -52.17f);
                    this.VirtualCamera.transform.eulerAngles = new Vector3(45f, -30f, 0f);
                }
                else
                {
                    this.VirtualCamera.Follow = GameManager.RealSelfPlayer.transform;
                    this.VirtualCamera.transform.eulerAngles = new Vector3(45f, 0, 0f);
                }
            }
        }
    }

}
