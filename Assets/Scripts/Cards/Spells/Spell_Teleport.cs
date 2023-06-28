using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Mechanics;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Spells {
	public class TeleportationEventData : EventData<TeleportationEventData> {
		public CharacterEntity EntityThatTeleported;
		public CharacterEntity EntityTheyTriedTeleportingTo;
		public Cell StartCell;
		public Cell EndCell;

		public TeleportationEventData(CharacterEntity EntityThatTeleported, CharacterEntity EntityTheyTriedTeleportingTo, Cell StartCell, Cell EndCell) {
			this.EntityThatTeleported = EntityThatTeleported;
			this.EntityTheyTriedTeleportingTo = EntityTheyTriedTeleportingTo;
			this.StartCell = StartCell;
			this.EndCell = EndCell;
		}
	}
	public enum ETeleportType { GoTo, PullTo, TP0to1 };

	public class SpellData_Teleport : SpellData {
		[InfoBox("Pull : teleport the target to YOU.\nGoTo: You get teleported TO the target.")]
		public ETeleportType TeleportType;
	}

	public class Spell_Teleport : Spell<SpellData_Teleport> {
		public Spell_Teleport(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, TargettingCondition targCond, CastingCondition castCond) : base(CopyData, RefEntity, TargetCell, ParentSpell, targCond, castCond) {
		}

		public override async Task DoSpellBehavior() {
			await base.DoSpellBehavior();
			if (TargetEntities.Count >= 1 ) {
				Cell cellToTP = null;
				switch (LocalData.TeleportType) {
					case ETeleportType.PullTo: {
						Cell oldCell = TargetEntities[0].EntityCell;
						cellToTP = TargetEntities[0].SmartTeleport(RefEntity.EntityCell, Result);
						TargetedCells.Add(cellToTP);
						RefEntity.FireOnTeleportation(new TeleportationEventData(TargetEntities[0], RefEntity, oldCell, cellToTP));
						break;
					}

					case ETeleportType.GoTo: {
						Cell oldCell = RefEntity.EntityCell;
						cellToTP = RefEntity.SmartTeleport(TargetCell, Result);
						TargetedCells.Add(oldCell);
						RefEntity.FireOnTeleportation(new TeleportationEventData(RefEntity, TargetCell.EntityIn, oldCell, cellToTP));
						break;
					}
					case ETeleportType.TP0to1: {
						Cell oldCell = TargetEntities[0].EntityCell;
						cellToTP = TargetedCells[1].EntityIn.SmartTeleport(TargetedCells[0], Result);
						SFXManager.Instance.DOSFX(new RuntimeSFXData(Data.CellSFX, RefEntity, cellToTP, this));
						TargetedCells.Add(oldCell);
						TargetedCells.Add(cellToTP);
						RefEntity.FireOnTeleportation(new TeleportationEventData(TargetedCells[1].EntityIn, TargetedCells[0].EntityIn, oldCell, cellToTP));
						break;
					}
				}
			}
			else if (LocalData.TeleportType == ETeleportType.PullTo) {
				string debug = "NO ENTITIES";
				if (TargetEntities.Count > 0) {
					debug = "";
					foreach (var item in TargetEntities) {
						debug += item.ToString();
					}
				}
				Debug.LogWarning($"TargetEntities.Count != 1 in the teleport spell. This is not an issue, but it's not normal. Entities: {debug}");
			}

		}
	}
}
