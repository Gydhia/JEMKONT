using DownBelow.GridSystem;

namespace DownBelow.Entity
{
    public class TargettingAction : EntityAction
    {
        public AttackType Type;

        public TargettingAction(CharacterEntity RefEntity, Cell TargetCell) 
            : base(RefEntity, TargetCell)
        {
        }

        public override void ExecuteAction()
        {
            CharacterEntity target;

            switch (Type)
            {
                case AttackType.ClosestRandom: target = this.TargetClosestRandom(); break;
                case AttackType.FarthestRandom:
                default: target = this.TargetFarthestRandom(); break;
            }

            this.TargetCell = target.EntityCell;
            target.FireEntityTargetted(this.RefEntity);

            base.EndAction();
        }

        /// <summary>
        /// Will target the closest player (random btwn two at the same range)
        /// </summary>
        private CharacterEntity TargetClosestRandom()
        {
            CharacterEntity[] PlayersByDistance = ((EnemyEntity)this.RefEntity).PlayersOrderedByDistance("Min", out int sameDist);

            return PlayersByDistance[UnityEngine.Random.Range(0, sameDist)];
        }

        /// <summary>
        /// Will target the closest player (random btwn two at the same range)
        /// </summary>
        private CharacterEntity TargetFarthestRandom()
        {
            CharacterEntity[] PlayersByDistance = ((EnemyEntity)this.RefEntity).PlayersOrderedByDistance("Max", out int sameDist);

            return PlayersByDistance[UnityEngine.Random.Range(0, sameDist)];
        }


        public override object[] GetDatas()
        {
            return new object[0];
        }

        public override void SetDatas(object[] Datas) { }
    }
}