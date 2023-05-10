using DownBelow.Entity;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XNode;

[ShowOdinSerializedPropertiesInInspector, NodeWidth(500)]
public class SpellNode : Node
{
    public Spell spell;

    [Output(ShowBackingValue.Never, dynamicPortList = false, connectionType = ConnectionType.Multiple)]
    public bool GetTargetsHit;
    private List<CharacterEntity> _getTargetHit => spell.Result.DamagesDealt.Keys.ToList();
    public override object GetValue(NodePort port)
    {
        if(port.fieldName == nameof(GetTargetsHit))
        {
            return _getTargetHit;
        }
        return base.GetValue(port);
    }
}

public class SpellPort : NodePort
{
    public SpellPort(NodePort nodePort, Node node) : base(nodePort, node)
    {
    }
}
