using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Entity
{
    public class PlayerBehavior : CharacterEntity
    {
        public MeshRenderer PlayerBody;

        public void Init(GridPosition startingPosition, Vector3 worldPosition, GridSystem.CombatGrid grid)
        {
            this.PlayerPosition = startingPosition;
            this.gameObject.transform.position = new Vector3(worldPosition.x, 0f, worldPosition.z);
            this.CurrentGrid = grid;
        }

        public void MovePlayer()
        {

        }
    }
}
