using DownBelow.Entity;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[ShowOdinSerializedPropertiesInInspector, NodeWidth(300)]
public class G_Target_Entities_Node : Node
{
    public ETargetType TargetType;
    public bool ManualTargetting;

    [Input(ShowBackingValue.Never, dynamicPortList = false, connectionType = ConnectionType.Override)]
    public bool[,] CastingShape;

    [Input(ShowBackingValue.Never, dynamicPortList = false, connectionType = ConnectionType.Override)]
    public bool[,] SpellShape;

    [Input(ShowBackingValue.Never, dynamicPortList = false, connectionType = ConnectionType.Override)]
    public ConditionLink Condition;

    [Output(ShowBackingValue.Never, dynamicPortList = false, connectionType = ConnectionType.Multiple)]
    public Entities_Link Entities;

}
