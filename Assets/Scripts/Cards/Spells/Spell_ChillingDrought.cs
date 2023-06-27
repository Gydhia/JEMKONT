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
        public Spell_ChillingDrought(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, TargettingCondition targCond, CastingCondition castCond) : base(CopyData, RefEntity, TargetCell, ParentSpell, targCond,castCond)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();

            SetTargets(TargetCell)[0].ApplyStat(EntityStatistics.Health,
                - LocalData.BaseDamage +
                (((PlayerBehavior)RefEntity).Deck.RefCardsHolder.PileSize(Managers.PileType.Hand) * LocalData.DamagePenaltyPerCard)
                );
        }
    }
}
