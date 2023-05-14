using DownBelow.Entity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;


[ShowOdinSerializedPropertiesInInspector, NodeWidth(300)]
public class G_Statistic_Node : Node
{
    [Input(dynamicPortList = false, connectionType = ConnectionType.Override)]
    public EntityStatistics I_Statistic = EntityStatistics.None;

    [Input(ShowBackingValue.Never, dynamicPortList = false, connectionType = ConnectionType.Override)]
    public Entities_Link I_Entity;

    [Output(ShowBackingValue.Never, dynamicPortList = false, connectionType = ConnectionType.Multiple)]
    public Value_Link Value;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(Value))
        {
            return I_Statistic;
        }
        return base.GetValue(port);
    }
}
