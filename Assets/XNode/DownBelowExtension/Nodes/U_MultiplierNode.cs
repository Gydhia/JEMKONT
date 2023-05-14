using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[ShowOdinSerializedPropertiesInInspector, NodeWidth(250)]
public class U_MultiplierNode : Node
{
    [Input(ShowBackingValue.Never, connectionType = ConnectionType.Override)]
    public Value_Link BaseValue;

    public float Multiplier;

    [Output(ShowBackingValue.Never, dynamicPortList = false, connectionType = ConnectionType.Multiple)]
    public Value_Link ReturnValue;
}
