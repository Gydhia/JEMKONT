using DownBelow.GridSystem;
using DownBelow.Managers;
using EODE.Wonderland;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity {
	public class TargettingAction : EntityAction {
		public AttackType Type;

		public TargettingAction(CharacterEntity RefEntity, Cell TargetCell)
			: base(RefEntity, TargetCell) {
		}
		public void Init(AttackType type) {
			Debug.Log($"INIT: {type}");
			Type = type;
		}
		public override void ExecuteAction() {
			CharacterEntity target;

			switch (Type) {
				case AttackType.ClosestRandom: target = this.TargetClosestRandom(); break;
				case AttackType.LowestRandom:
					target = TargetRandomByHP(false);
					break;
				case AttackType.HighestRandom:
					target = TargetRandomByHP(true);
					break;
				case AttackType.Random:
					target = CombatManager.Instance.PlayersInGrid.Random();
					break;
				case AttackType.FarthestRandom:
				default: target = this.TargetFarthestRandom(); break;
			}
			Debug.Log($"TARGETTED: {target} => {Type}");
			if (target != null) {
				this.TargetCell = target.EntityCell;
				target.FireEntityTargetted(this.RefEntity);
			}

			base.EndAction();
		}

		private CharacterEntity TargetRandomByHP(bool highestInTheRoom) {
			int HP;
			var targets = new List<PlayerBehavior>();
			targets.AddRange(CombatManager.Instance.PlayersInGrid);
			if (highestInTheRoom) {
				HP = 0;
				foreach (PlayerBehavior play in CombatManager.Instance.PlayersInGrid) {
					if (HP == Mathf.Max(play.Health, HP)) {
						targets.Add(play);
					}
					else if (HP < Mathf.Max(play.Health, HP)) {
						HP = Mathf.Max(play.Health, HP);
						targets.Clear();
						targets.Add(play);
					}
				}
			}
			else {
				HP = int.MaxValue;
				foreach (PlayerBehavior play in CombatManager.Instance.PlayersInGrid) {
					if (HP == Mathf.Min(play.Health, HP)) {
						targets.Add(play);
					}
					else if (HP > Mathf.Min(play.Health, HP)) {
						HP = Mathf.Min(play.Health, HP);
						targets.Clear();
						targets.Add(play);
					}
				}
			}
			return targets.Random();
		}

		/// <summary>
		/// Will target the closest player (random btwn two at the same range)
		/// </summary>
		private CharacterEntity TargetClosestRandom() {
			CharacterEntity[] PlayersByDistance = ((EnemyEntity)this.RefEntity).PlayersOrderedByDistance("Min", out int sameDist);

			return PlayersByDistance.Length > 0 ? PlayersByDistance[UnityEngine.Random.Range(0, sameDist)] : null;
		}

		/// <summary>
		/// Will target the closest player (random btwn two at the same range)
		/// </summary>
		private CharacterEntity TargetFarthestRandom() {
			CharacterEntity[] PlayersByDistance = ((EnemyEntity)this.RefEntity).PlayersOrderedByDistance("Max", out int sameDist);

			return PlayersByDistance.Length > 0 ? PlayersByDistance[UnityEngine.Random.Range(0, sameDist)] : null;
		}


		public override object[] GetDatas() {
			return new object[1] { (int)Type };
		}

		public override void SetDatas(object[] Datas) {
			Type = (AttackType)(int.Parse(Datas[0].ToString()));
		}
	}
}