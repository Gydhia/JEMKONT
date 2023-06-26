using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Mechanics;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Spells
{
    public class SpellData_Card : SpellData
    {
        [Tooltip("Keep to null of should draw random card")]
        [FoldoutGroup("CARD Spell Datas")]
        public ScriptableCard SpecificCard;
        [FoldoutGroup("CARD Spell Datas")]
        public int DrawNumber = 1;
        [FoldoutGroup("CARD Spell Datas")]
        public bool EveryoneDraw;
    }

    public class Spell_DrawCard : Spell<SpellData_Card>
    {
        public Spell_DrawCard(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, TargettingCondition targCond, CastingCondition castCond) : base(CopyData, RefEntity, TargetCell, ParentSpell, targCond,castCond)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();

            List<PlayerBehavior> targets = new List<PlayerBehavior>();

            if (this.LocalData.EveryoneDraw)
            {
                targets = CombatManager.Instance.PlayersInGrid;
            }
            else
            {
                targets.Add(this.RefEntity as PlayerBehavior);
            }

            foreach (var player in targets)
            {
                for (int i = 0; i < LocalData.DrawNumber; i++)
                {
                    if (LocalData.SpecificCard != null)
                    {
                        player.Deck.CreateAndDrawSpecificCard(LocalData.SpecificCard);
                    }
                    else
                    {
                        player.Deck.DrawCard();
                    }
                }   
            }
        }
    }
}
