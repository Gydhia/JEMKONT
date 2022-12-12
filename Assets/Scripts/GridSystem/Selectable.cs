using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.GridSystem
{
    public class Selectable : MonoBehaviour
    {
        public Outline Outline;
        public MeshRenderer Renderer;

        public void Init(Color outlineColor)
        {
            this.Outline.OutlineColor = outlineColor;
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
}

