using Cinemachine;
using DownBelow.Entity;
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
        }
    }

}
