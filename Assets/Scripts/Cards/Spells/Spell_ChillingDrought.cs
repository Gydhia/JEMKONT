using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace DownBelow.Spells
{
    public class SpellData_ChillingDrought : SpellData
    {
        public int BaseDamage;
        public int DamagePenaltyPerCard;
    }

    public class Spell_ChillingDrought : Spell<SpellData_ChillingDrought>
    {
        public Spell_ChillingDrought(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();

            GetTargets(TargetCell)[0].ApplyStat(EntityStatistics.Health,
                LocalData.BaseDamage -
                (((PlayerBehavior)RefEntity).Deck.RefCardsHolder.PileSize(Managers.PileType.Hand) * LocalData.DamagePenaltyPerCard));

            EndAction();
        }
    }
}
