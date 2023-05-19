using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Entity
{
    public class MoleHole : NonCharacterEntity
    {
        [HideInInspector] public MoleHole otherHole;
        [HideInInspector] public bool CanTP;

        public override void Init(Cell AttachedCell, int TurnsLeft, CharacterEntity RefEntity, NCEPreset preset)
        {
            CanTP = true;
            base.Init(AttachedCell, TurnsLeft, RefEntity, preset);
            foreach (var item in CombatManager.Instance.PlayingEntities)
            {
                item.OnEnteredCell += CheckIfOnCell;
            }
            CombatManager.Instance.OnTurnEnded += ResetUse;
        }

        private void ResetUse(EntityEventData Data)
        {
            CanTP = true;
        }

        private void CheckIfOnCell(CellEventData Data)
        {
            if (Data.Cell == AttachedCell && CanTP)
            {
                otherHole.CanTP = false;
                Data.Cell.EntityIn.Teleport(otherHole.AttachedCell, null);
            }
        }
    }

}
