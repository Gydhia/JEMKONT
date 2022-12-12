using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public InteractablePreset InteractableRef;

    public void Init(InteractablePreset InteractableRef)
    {
        this.InteractableRef = InteractableRef;
    }
}
