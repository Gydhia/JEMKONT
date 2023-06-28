using DownBelow.GridSystem;
using DownBelow.Managers;
using EODE.Wonderland;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity {
	public class TargettingAction : EntityAction {
		public AttackType Type;

		public TargettingAction(CharacterEntity RefEntity, Cell TargetCell)
			: base(RefEntity, TargetCell) 
		{
		}

		public void Init(AttackType type) 
		{
			Debug.Log($"INIT: {type}");
			Type = type;
		}

		public override void ExecuteAction() 
		{
			CharacterEntity target;

			switch (Type) {
				case AttackType.ClosestRandom: 
					target = this.TargetClosestRandom();
					break;
				case AttackType.LowestRandom:
					target = this.TargetRandomByHP(false);
					break;
				case AttackType.HighestRandom:
					target = this.TargetRandomByHP(true);
					break;
				case AttackType.Random:
					var players = this.GetAllPlayers();
					target = players.Random();
					break;
				case AttackType.HighestBaseHP:
					target = this.TargetByMaxHP();
					break;
				case AttackType.FarthestRandom:
				default: 
					target = this.TargetFarthestRandom(); 
					break;
			}

			Debug.Log($"TARGETTED: {target} => {Type}");
			if (target != null) 
			{
				this.TargetCell = target.EntityCell;
				target.FireEntityTargetted(this.RefEntity);
			}

			base.EndAction();
		}

		private CharacterEntity TargetByMaxHP()
        {
			var players = this.GetAllPlayers();

			if(players.Count <= 0) { return null; }

			var targetPlayer = players[0];
            for (int i = 1; i < players.Count; i++)
            {
				if(players[i].MaxHealth > targetPlayer.MaxHealth)
                {
					targetPlayer = players[i];
                }
            }

			return targetPlayer;
		}

		private CharacterEntity TargetRandomByHP(bool highestInTheRoom) 
		{
			int HP;
			var targets = new List<PlayerBehavior>();

			var players = this.GetAllPlayers();

			if (highestInTheRoom) 
			{
				HP = 0;
				foreach (PlayerBehavior player in players) 
				{
					if (HP == Mathf.Max(player.Health, HP)) 
					{
						targets.Add(player);
					}
					else if (HP < Mathf.Max(player.Health, HP)) 
					{
						HP = Mathf.Max(player.Health, HP);
						targets.Clear();
						targets.Add(player);
					}
				}
			}
			else 
			{
				HP = int.MaxValue;
				foreach (PlayerBehavior player in players) 
				{
					if (HP == Mathf.Min(player.Health, HP)) 
					{
						targets.Add(player);
					}
					else if (HP > Mathf.Min(player.Health, HP)) 
					{
						HP = Mathf.Min(player.Health, HP);
						targets.Clear();
						targets.Add(player);
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

			return PlayersByDistance.Length > 0 ? PlayersByDistance[0] : null;
		}

		/// <summary>
		/// Will target the closest player (random btwn two at the same range)
		/// </summary>
		private CharacterEntity TargetFarthestRandom() {
			CharacterEntity[] PlayersByDistance = ((EnemyEntity)this.RefEntity).PlayersOrderedByDistance("Max", out int sameDist);

			return PlayersByDistance.Length > 0 ? PlayersByDistance[0] : null;
		}


		public List<PlayerBehavior> GetAllPlayers()
        {
			List<PlayerBehavior> players = new List<PlayerBehavior>();
			for (int i = 0; i < CombatManager.Instance.PlayingEntities.Count; i++)
			{
				if (CombatManager.Instance.PlayingEntities[i] is PlayerBehavior player)
				{
					players.Add(player);
				}
			}

			return players;
		}

		public override object[] GetDatas() {
			return new object[1] { (int)Type };
		}

		public override void SetDatas(object[] Datas) {
			Type = (AttackType)(int.Parse(Datas[0].ToString()));
		}
	}
}