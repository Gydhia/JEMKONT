using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GridSystem
{
    public class CombatGrid : WorldGrid
    {
        public int Longitude;
        public int Latitude;

        public bool HasStarted = false;
        public WorldGrid ParentGrid = null;

        public List<Cell> PlacementCells;

        public override void Init(GridData data)
        {
            base.Init(data);

            this.PlacementCells = new List<Cell>();

            foreach (var sp in data.SpawnablePresets)
            {
                if(GridManager.Instance.SpawnablesPresets[sp.Value] is SpawnPreset)
                {
                    var cell = this.Cells[sp.Key.latitude, sp.Key.longitude];
                    cell.IsPlacementCell = true;

                    this.PlacementCells.Add(cell);
                }
            }
        }
    }
}
