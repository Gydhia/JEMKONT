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

        public List<Cell> EntranceCells;

        public override void Init(GridData data, WorldGrid ParentGrid = null)
        {
            base.Init(data);
            this.ParentGrid = ParentGrid;

            this.Longitude = data.Longitude;
            this.Latitude = data.Latitude;

            this.PlacementCells = new List<Cell>();

            foreach (var sp in data.SpawnablePresets)
            {
                if(SettingsManager.Instance.SpawnablesPresets[sp.Value] is SpawnPreset)
                {
                    var cell = this.Cells[sp.Key.latitude, sp.Key.longitude];
                    cell.IsPlacementCell = true;

                    this.PlacementCells.Add(cell);
                }
            }

            if(data.Entrances != null)
            {
                foreach (var entrance in data.Entrances)
                {
                    var cell = this.ParentGrid.Cells[entrance.latitude, entrance.longitude];
                    cell.RedirectedGrid = this;
                    var particle = Instantiate(SettingsManager.Instance.GridsPreset.CombatEntracePrefab, cell.transform.position, Quaternion.identity, cell.transform);
                    particle.transform.Rotate(new Vector3(90f, 0f, 0f));

                    this.EntranceCells.Add(cell);
                }
            }
            else
            {
                Debug.LogError("A Combat Grid has no Entrance !");
            }
        }
    }
}
