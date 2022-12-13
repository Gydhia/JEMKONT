using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [HideInInspector]
    public InteractablePreset InteractableRef;

    public MeshRenderer Renderer;

    public Outline Outline;

    public void Init(InteractablePreset InteractableRef)
    {
        this.Outline.enabled = false;

        this.InteractableRef = InteractableRef;
        this.Outline.OutlineColor = InteractableRef.OutlineColor;
    }

    public void Interact()
    {

    }
    public void OnFocused()
    {
        this.Outline.enabled = true;
    }

    public void OnUnfocused()
    {
        this.Outline.enabled = false;
    }
}
