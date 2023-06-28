using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Spells.Alterations;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Spells {
	public class SpellData_Alteration : SpellData {
		public Alteration Alteration;
	}

	public class Spell_Alteration : Spell<SpellData_Alteration> {
		public Spell_Alteration(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, TargettingCondition targCond, CastingCondition castCond) : base(CopyData, RefEntity, TargetCell, ParentSpell, targCond, castCond) {
		}

		public override async Task DoSpellBehavior() {
			await base.DoSpellBehavior();
			switch (LocalData.Alteration) {
				case FishyBusiness fishy:
					fishy.Player = (PlayerBehavior)RefEntity;
					break;
			}

			foreach (var entity in TargetEntities) {
				entity.AddAlteration(LocalData.Alteration);
			}
		}
	}
}
