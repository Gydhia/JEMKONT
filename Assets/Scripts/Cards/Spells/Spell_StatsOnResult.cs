using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Linq;

namespace DownBelow.Spells
{
    public class Spell_StatsOnResult : Spell<SpellData_Stats>
    {
        public Spell_StatsOnResult(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            GetTargets(TargetCell);
            var targets = Result.TargetedCells.FindAll(x => x.EntityIn != null).Select(x => x.EntityIn);
            if(targets != null)
            {
                foreach (CharacterEntity target in targets)
                {
                    target.ApplyStat(LocalData.Statistic,
                        LocalData.StatAmount * (LocalData.IsNegativeEffect ? -1 : 1));
                }
            }

            EndAction();
        }
    }
}
