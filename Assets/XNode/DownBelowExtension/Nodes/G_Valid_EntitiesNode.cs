using DownBelow.Spells;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public enum EValidationType
{
    None = 0,

    Greatest = 1,
    Equal = 2,
    Lowest = 3
}

[ShowOdinSerializedPropertiesInInspector, NodeWidth(300)]
public class G_Valid_EntitiesNode : Node
{
    public EntityStatistics Statistic;
    public ETargetType TargetType;
    public EValidationType ValidationType;

    [Output(ShowBackingValue.Never, dynamicPortList = false, connectionType = ConnectionType.Multiple)]
    public Entities_Link Entity;
}
