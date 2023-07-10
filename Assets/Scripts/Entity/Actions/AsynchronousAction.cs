using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace DownBelow.Entity {

	public abstract class AsynchronousAction : EntityAction {
		protected AsynchronousAction(CharacterEntity RefEntity, Cell TargetCell) : base(RefEntity, TargetCell) {
		}

		public override async void ExecuteAction() {

			await AsynchronousBehaviour();
			EndAction();
		}
		
		public abstract Task AsynchronousBehaviour();
	}
}