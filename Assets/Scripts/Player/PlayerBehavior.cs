using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Player
{
    public class PlayerBehavior : CharacterEntity
    {
        public MeshRenderer PlayerBody;

        public static int MovementPoints = 5;

        public void Init(GridPosition startingPosition, Vector3 worldPosition)
        {
            this.PlayerPosition = startingPosition;
            this.gameObject.transform.position = new Vector3(worldPosition.z, 0f, worldPosition.x);
        }

        public void MovePlayer()
        {

        }
    }
}
