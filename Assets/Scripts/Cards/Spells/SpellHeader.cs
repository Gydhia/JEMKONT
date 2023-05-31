using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells
{
    public class SpellHeader
    {
        public Guid RefCard;
        public GridPosition[] TargetedCells;
        public string CasterID;

        public SpellHeader(Guid RefCard, int SpellsNumber, string CasterID)
        {
            this.RefCard = RefCard;
            this.TargetedCells = new GridPosition[SpellsNumber];
            this.CasterID = CasterID;
        }
    }
}