﻿using DG.Tweening;
using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using EasyTransition;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Android.LowLevel;

namespace DownBelow
{
    public class InteractableGridPortal : Interactable
    {
        public string TargetedGrid;

        public TransitionSettings TransitionSettings;

        public Transform Portal;
        public GameObject InnerCrackParticles;
        public GameObject MainCrack;
        public GameObject CrackBorders;
        public ParticleSystem Orb;

        public float Delay;


        private bool _playedOnce = false;
        private bool _isPlaying = false;


        private void Start()
        {
            this.Portal.DOMoveY(this.transform.position.y - 0.2f, 1.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            this.Portal.gameObject.SetActive(false);
        }

        public override void Interact(PlayerBehavior player)
        {
            if(this._isPlaying) { return; }

            if (!this._playedOnce)
            {
                this._isPlaying = true;

                this._hideInteractParticle();
                this._openPortal();
            }
            // Only the local player with process animations, ...
            else if (GameManager.RealSelfPlayer == player)
            {
                player.CanMove = false;
                StartCoroutine(PlayTeleport());
            }
            
        }

        // Stop the little light that we clicked on to enable portal
        private void _hideInteractParticle()
        {
            ParticleSystem.MainModule main;

            foreach (var ps in this.Orb.GetComponentsInChildren<ParticleSystem>())
            {
                main = ps.main;
                main.startLifetimeMultiplier = 0f;
                main.startLifetime = 0;
                main.maxParticles = 0;
            }
        }

        // Open the portal
        private void _openPortal()
        {
            this.Portal.gameObject.SetActive(true);
            CrackBorders.transform.DOScale(1.65f, 0.5f);
            StartCoroutine(ActivationRoutine());
        }

        private IEnumerator ActivationRoutine()
        {
            yield return new WaitForSeconds(Delay);

            InnerCrackParticles.SetActive(true);
            MainCrack.SetActive(true);

            this._isPlaying = false;
            this._playedOnce = true;
        }

        private IEnumerator PlayTeleport()
        {
            TransitionManager.Instance().Transition(this.TransitionSettings, 0f);
            yield return new WaitForSeconds(this.TransitionSettings.transitionTime / 2f);
            this._teleportToGrid();
        }

        private void _teleportToGrid()
        {
            if(GridManager.Instance.WorldGrids.TryGetValue(this.TargetedGrid, out WorldGrid grid))
            {
                var gridAction = new EnterGridAction(GameManager.RealSelfPlayer, grid.Cells[0, 0]);
                NetworkManager.Instance.EntityAskToBuffAction(gridAction);
            }

            GameManager.RealSelfPlayer.CanMove = true;
        }

        private void OnDisable()
        {
            this.InnerCrackParticles.SetActive(false);
            this.MainCrack.SetActive(false);
            this.CrackBorders.transform.localScale = Vector3.zero;
        }
    }

}