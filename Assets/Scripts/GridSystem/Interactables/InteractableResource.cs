using DownBelow.Managers;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class InteractableResource : Interactable<ResourcePreset>
    {
        public List<GameObject> MeshToHide = new List<GameObject>();
        public GameObject GatheredMesh;
        public AK.Wwise.Event Sound;
        [ReadOnly] public bool isMature = true;
        [Tooltip("In Seconds.")] public int TimeToGrowUp = 80;

        public override void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            isMature = true;
            base.Init(InteractableRef, RefCell);

            foreach (GameObject ToHide in MeshToHide)
            {
                ToHide.SetActive(true);
            }
            this.GatheredMesh.gameObject.SetActive(false);
        }

        public override void Interact(Entity.PlayerBehavior player)
        {
            foreach (GameObject ToHide in MeshToHide)
            {
                ToHide.SetActive(false);
            }
            this.GatheredMesh.gameObject.SetActive(true);
            
            StartCoroutine(GrowingRoutine());
        }
        IEnumerator GrowingRoutine()
        {
            isMature = false;
            yield return new WaitForSeconds(TimeToGrowUp);
            isMature = true;
            foreach (GameObject ToHide in MeshToHide)
            {
                ToHide.SetActive(true);
            }
            this.GatheredMesh.gameObject.SetActive(false);
        }
    }
}