using Cinemachine;
using DownBelow.Entity;
using DownBelow.Events;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DownBelow.Managers
{
    public class CameraManager : _baseManager<CameraManager>
    {
        //player input
        public PlayerInput PlayerInput;

        //Events

        [SerializeField]
        private CinemachineVirtualCamera VirtualCamera;
        [SerializeField]
        private Camera MainCamera;
        [SerializeField]
        private bool IsInCombatGrid;
        private bool IsGatheringZoom;
        private Vector3 ArenaCenterPos = new Vector3(0, 0, 0);
        private GameObject ArenaCenterObj;
        private Vector2 CurrentArenaSize = new Vector2(0, 0);

        //Parameters
        public Vector3 InBattleAngle = new Vector3(45,-30,0);
        private float InBattleFOV = 40f;
        public float Zoom = 0f;
        public float ZoomSpeed = 1f;
        public float ZoomSmooth = 1f;
        public float NormalOrthoSize= 8f;
        public float InBattleOrthoSizeByArenaSize = 0.3f;

        public float GatheringZoomingTimeInSec = 1f;
        public float GatheringTargetZoom = 1f;



        private void Start()
        {
            this.SubscribeToInputEvents();
        }


        public void SubscribeToInputEvents()
        {
            PlayerInputs.player_scroll.performed += this._manageScroll;
        }

        public void AttachPlayerToCamera(PlayerBehavior player)
        {
            this.VirtualCamera.Follow = player.gameObject.transform;

            GameManager.Instance.OnEnteredGrid += this._setupCamera;
            player.OnGatheringStarted += this.GatheringZoomIn;
            player.OnGatheringEnded += this.GatheringZoomOut;
        }

        private void _setupCamera(EntityEventData Data)
        {
            if (GameManager.SelfPlayer == Data.Entity)
            {
                if (GameManager.SelfPlayer.CurrentGrid.IsCombatGrid) 
                {
                    float maxX = GameManager.SelfPlayer.CurrentGrid.GridHeight;
                    float maxY = GameManager.SelfPlayer.CurrentGrid.GridWidth;

                    CurrentArenaSize = new Vector2(maxX, maxY);

                    float midX = maxX / 2;
                    float midY = maxY / 2;

                    ArenaCenterPos = GameManager.SelfPlayer.CurrentGrid.Cells[Mathf.RoundToInt(midX), Mathf.RoundToInt(midY)].WorldPosition;
                    ArenaCenterObj = new GameObject("ArenaCenterObj");
                    ArenaCenterObj.transform.position = ArenaCenterPos;

                    IsInCombatGrid = true;

                    this.VirtualCamera.transform.position = new Vector3(61.28f, 25f, -52.17f);
                    this.VirtualCamera.transform.eulerAngles = InBattleAngle;
                    this.VirtualCamera.Follow = ArenaCenterObj.transform;
                    this.VirtualCamera.LookAt = ArenaCenterObj.transform;
                }
                else
                {
                    IsInCombatGrid = false;
                    this.VirtualCamera.Follow = Data.Entity.transform;
                }

                
            }
   
        }

        private void Update()
        {
            if (this.VirtualCamera == null)
            {
                return;
            }
            if (GameManager.SelfPlayer && GameManager.SelfPlayer.CurrentGrid)
            {
                if (GameManager.SelfPlayer.CurrentGrid.IsCombatGrid)
                {
                    InBattleAngle.x = Mathf.Clamp(InBattleAngle.x, 0, 90);
                    IsInCombatGrid = true;

                    Vector3 orientationVector = new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * InBattleAngle.y + 45), Mathf.Sin(Mathf.Deg2Rad * InBattleAngle.y + 45));
                    this.VirtualCamera.transform.position = ArenaCenterPos;
                    this.VirtualCamera.transform.eulerAngles = InBattleAngle;
                    this.VirtualCamera.m_Lens.FieldOfView = InBattleFOV;

                    float rotXRatio = InBattleAngle.x / 90;
                    float rotYRatio = Mathf.Abs(InBattleAngle.y / 45);
                    rotYRatio = Mathf.Abs(Mathf.Sin(rotYRatio * 1.57f));
                    float orthoWithoutZoom = (CurrentArenaSize.x + CurrentArenaSize.x * rotXRatio + CurrentArenaSize.y + CurrentArenaSize.y * rotYRatio) * InBattleOrthoSizeByArenaSize;
                    float orthoZoom = Zoom * (CurrentArenaSize.x + CurrentArenaSize.y) * InBattleOrthoSizeByArenaSize;
                    this.VirtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(this.VirtualCamera.m_Lens.OrthographicSize, orthoWithoutZoom - orthoZoom, 1 / (ZoomSmooth * 10));
                }
                else if (IsGatheringZoom)
                {
                    this.VirtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(this.VirtualCamera.m_Lens.OrthographicSize, NormalOrthoSize - Zoom * NormalOrthoSize, 1 / (ZoomSmooth * 10));
                }
            }
            
        }

        private void _manageScroll(InputAction.CallbackContext ctx) => this.manageScroll(ctx.ReadValue<float>());

        private void manageScroll(float delta)
        {
            if (IsInCombatGrid)
            {
                Zoom += delta * ZoomSpeed* Time.deltaTime;
                Zoom = Mathf.Clamp(Zoom, 0f, 0.99f);
            }
        }

        IEnumerator Zooming(bool Positive)
        {
            IsGatheringZoom = true;
            float timeInSec = GatheringZoomingTimeInSec;
            float ZoomTarget = GatheringTargetZoom*2;
            float Weight = timeInSec;
            for (float i = 0; i <= timeInSec; i+= Time.deltaTime)
            {
                int sign = Positive ? 1 : -1;
                float ratio = Weight / timeInSec;
                Zoom += sign * ((ZoomTarget * ratio) / timeInSec) * Time.deltaTime;
                Zoom = Mathf.Clamp(Zoom, 0f, 0.99f);
                Weight -= Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }
            Zoom = Positive ? GatheringTargetZoom : 0;
            IsGatheringZoom = false;
        }

        private void GatheringZoomIn(GatheringEventData data)
        {
            Zoom = 0;
            StartCoroutine(Zooming(true));
        }
        private void GatheringZoomOut(GatheringEventData data)
        {
            Zoom = GatheringTargetZoom;
            StartCoroutine(Zooming(false));
        }
    }

}
