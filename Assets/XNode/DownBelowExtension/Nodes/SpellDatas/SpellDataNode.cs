using DownBelow.Spells;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[ShowOdinSerializedPropertiesInInspector, NodeWidth(500)]
public class SpellDataNode : Node
{
    [Input(ShowBackingValue.Never, connectionType = ConnectionType.Override)]
    public SpellDataNode PreviousSpell;
    [Output(ShowBackingValue.Never, connectionType = ConnectionType.Override)]
    public SpellDataNode NextSpell;


    [Input(ShowBackingValue.Never, connectionType = ConnectionType.Override)]
    public Entities_Link Targets;
    [Input(ShowBackingValue.Never, connectionType = ConnectionType.Override)]
    public ConditionLink ValidateCondition;

    [Output(ShowBackingValue.Never, dynamicPortList = false, connectionType = ConnectionType.Multiple)]
    public Entities_Link Entity;
}
