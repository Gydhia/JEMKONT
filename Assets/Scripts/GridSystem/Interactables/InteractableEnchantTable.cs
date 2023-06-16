using DownBelow.Entity;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class InteractableEnchantTable : Interactable
    {
        public override void Interact(PlayerBehavior p)
        {
            UIManager.Instance.EnchantSection.OpenPanel();
        }
    }

}
