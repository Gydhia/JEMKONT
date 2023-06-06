using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Spells
{
    public class SpellData_BIAB : SpellData
    {
        public int HealingPerCard;
    }

    public class Spell_BileInABottle : Spell<SpellData_BIAB>
    {
        public Spell_BileInABottle(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();

            foreach (PlayerBehavior item in CombatManager.Instance.PlayingEntities.FindAll(x => x.IsAlly).Cast<PlayerBehavior>())
            {
                item.Deck.DrawCard();

                item.ApplyStat(EntityStatistics.Health, LocalData.HealingPerCard * item.Deck.RefCardsHolder.PileSize(PileType.Hand));
            }

            EndAction();
        }

    }

}
