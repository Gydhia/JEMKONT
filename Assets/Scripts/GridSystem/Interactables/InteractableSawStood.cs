using DownBelow.Entity;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class InteractableSawStood : Interactable
    {
        public override void Interact(PlayerBehavior p)
        {
            UIManager.Instance.WorkshopSection.WorkshopName.text = "Mechanical Saw";
            UIManager.Instance.WorkshopSection.OpenPanel();
        }
    }
}