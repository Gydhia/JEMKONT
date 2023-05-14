using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SD_DrawNode : SpellDataNode
{
    [Input(ShowBackingValue.Never, connectionType = ConnectionType.Override)]
    public Value_Link Value;
}
