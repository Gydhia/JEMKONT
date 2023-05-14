using DownBelow;
using DownBelow.Spells;
using DownBelow.Spells.Alterations;
using ExternalPropertyAttributes;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[ShowOdinSerializedPropertiesInInspector, NodeWidth(300)]
public class Spell_Condition_Node : Node
{
    [Input(ShowBackingValue.Never ,dynamicPortList = false, connectionType = ConnectionType.Override)]
    public Entities_Link Targets;

    [Input(dynamicPortList = false, connectionType = ConnectionType.Override)]
    public ETargetType TargetType;
    [Input(dynamicPortList = false, connectionType = ConnectionType.Override)]
    public EntityStatistics Statistic;
    [Input(dynamicPortList = false, connectionType = ConnectionType.Override)]
    public EAlterationType Alteration;
    [Input(dynamicPortList = false, connectionType = ConnectionType.Override)]
    public ConditionOperator Operator;
    [Input(dynamicPortList = false, connectionType = ConnectionType.Override)]
    public int Value;

    [Output(ShowBackingValue.Never, dynamicPortList = false, connectionType = ConnectionType.Multiple)]
    public ConditionLink DoValidate;
}
