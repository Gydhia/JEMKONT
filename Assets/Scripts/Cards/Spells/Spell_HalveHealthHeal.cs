using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Mechanics;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells
{
    public class SpellData_BonusSFX : SpellData
    {
        [Tooltip("You can try to use this, if it works it works if not ask the devs.")]
        public ScriptableSFX BonusSFX;
        [Tooltip("(Has to be Instantaneous) You can try to use this, if it works it works if not ask the devs.")]
        public ScriptableSFX BonusCellSFX;
    }

    public class Spell_HalveHealthHeal : Spell<SpellData_BonusSFX>
    {
        public Spell_HalveHealthHeal(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override async void ExecuteAction()
        {
            base.ExecuteAction();

            CharacterEntity target = GetTargets(TargetCell)[0];
            int healAmount = target.Health / 2;

            target.ApplyStat(EntityStatistics.Health, -healAmount);

            var targets = CombatManager.Instance.PlayingEntities.FindAll(x => x.IsAlly && x != target);
            for (int i = 0;i < targets.Count;i++)
            {
                var targeted = targets[i];
                if (i != targets.Count)
                    //Not awaiting since we want to do it all. Suggestion could be to wait 0.05s to have some kind of wave effect.
                    SFXManager.Instance.DOSFX(new(LocalData.BonusSFX, RefEntity, targeted.EntityCell, this));
                else
                    await SFXManager.Instance.DOSFX(new(LocalData.BonusSFX, RefEntity, targeted.EntityCell, this));
            }

            foreach (var item in targets)
            {
                item.ApplyStat(EntityStatistics.Health, healAmount);
                await SFXManager.Instance.DOSFX(new(LocalData.BonusCellSFX, RefEntity, item.EntityCell, this));
            }

            EndAction();
        }

    }

}