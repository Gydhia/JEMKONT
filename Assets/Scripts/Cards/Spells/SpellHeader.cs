using DownBelow.Entity;
using DownBelow.GridSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellHeader
{
    public Dictionary<Guid, List<CharacterEntity>> RuntimeEntities;
    public Dictionary<Guid, Cell> RuntimeCells;
    public Dictionary<Guid, bool[,]> RuntimeShapes;


}
