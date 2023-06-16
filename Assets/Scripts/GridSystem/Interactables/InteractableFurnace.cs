using DownBelow.Entity;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class InteractableFurnace : Interactable
    {
        public override void Interact(PlayerBehavior p)
        {
            UIManager.Instance.WorkshopSection.WorkshopName.text = "Furnace";
            UIManager.Instance.WorkshopSection.OpenPanel();
        }
    }
}