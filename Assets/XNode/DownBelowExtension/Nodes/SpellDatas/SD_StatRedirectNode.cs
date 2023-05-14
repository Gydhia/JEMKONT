using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SD_StatRedirectNode : SpellDataNode
{
    [Input(ShowBackingValue.Never, connectionType = ConnectionType.Override)]
    public Entities_Link RefEntity;

    [Input(connectionType = ConnectionType.Override)]
    public EntityStatistics Statistic;

    public float Multiplier;
}
