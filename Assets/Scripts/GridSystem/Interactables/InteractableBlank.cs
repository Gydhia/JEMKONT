using DownBelow.Managers;
using DownBelow.UI.Inventory;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class InteractableBlank : Interactable
    {
        public override void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            base.Init(InteractableRef, RefCell);


        }


        public override void Interact(Entity.PlayerBehavior player)
        {
            // Only show the ui if we're the one who asked
            if(player == GameManager.RealSelfPlayer)
            {
            }
        }
    }
}