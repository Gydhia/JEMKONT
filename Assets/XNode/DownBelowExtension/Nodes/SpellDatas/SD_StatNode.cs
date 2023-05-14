using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SD_StatNode : SpellDataNode
{
    [Input(connectionType = ConnectionType.Override)]
    public EntityStatistics Statistic;

    public bool Remove;
    [Input(ShowBackingValue.Never,connectionType = ConnectionType.Override)]
    public Value_Link Value;
}
