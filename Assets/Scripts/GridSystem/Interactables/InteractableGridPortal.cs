using DG.Tweening;
using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using EasyTransition;
using System.Collections;
using UnityEngine;

namespace DownBelow
{
    public class InteractableGridPortal : Interactable<PortalInteractablePreset>
    {
        public Transform Portal;
        public GameObject InnerCrackParticles;
        public GameObject MainCrack;
        public GameObject CrackBorders;

        public ParticleSystem Orb;
        public ParticleSystem BubbleTrail;

        public float Delay;


        private bool _playedOnce = false;
        private bool _isPlaying = false;


        private void Start()
        {
            this.Portal.DOMoveY(this.transform.localPosition.y - 0.2f, 3f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
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
                // We chose the abyss
                if (this.LocalPreset.ToCombat)
                {
                    UIManager.Instance.AbyssesSection.OpenPanel();
                }
                else
                {
                    player.TeleportToGrid("FarmLand");
                }                
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

            this.BubbleTrail.gameObject.SetActive(true);
        }

        // Open the portal
        private void _openPortal()
        {
            this.Portal.gameObject.SetActive(true);
            CrackBorders.transform.DOScale(1.65f, 0.5f);
            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(ActivationRoutine());
            }
            else
            {
                _activeParticles();
            }
        }

        private IEnumerator ActivationRoutine()
        {
            yield return new WaitForSeconds(Delay);

            this._activeParticles();
        }

        private void _activeParticles()
        {
            InnerCrackParticles.SetActive(true);
            MainCrack.SetActive(true);

            this._isPlaying = false;
            this._playedOnce = true;
        }

        private void OnDisable()
        {
            this.InnerCrackParticles.SetActive(false);
            this.MainCrack.SetActive(false);
            this.CrackBorders.transform.localScale = Vector3.zero;
        }

        private void OnEnable()
        {
            if(!this._isPlaying && this._playedOnce)
            {
                this._hideInteractParticle();
                this._openPortal();
            }
        }
    }

}