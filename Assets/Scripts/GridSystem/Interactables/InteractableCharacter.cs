using DownBelow.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class InteractableCharacter : Interactable
    {


        public override void Init(InteractablePreset InteractableRef, Cell RefCell)
        {
            base.Init(InteractableRef, RefCell);
        }

        public override void Interact(PlayerBehavior p)
        {
            throw new System.NotImplementedException();
        }
    }
}
