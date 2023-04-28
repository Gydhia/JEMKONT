using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Network
{
    public class NetworkPreset : ScriptableObject
    {
        [Range(1f, 50f)]
        public float TimeBeforeTimeOut = 10f;
    }

}