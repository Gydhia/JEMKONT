using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlaceable
{
    public void Place(CellEventData cell);
    /*{
        cell.Datas.placeableOnCell = this;
    }*/
    public void Previsualize(CellEventData cell);
}
