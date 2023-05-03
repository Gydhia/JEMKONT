using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Network
{
    public class NetworkParamDesc
    {
        public string name;
        public bool optional;
        public object defaultValue;
        public Type type;
    }
}