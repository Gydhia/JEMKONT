using DownBelow.Spells;
using System;
using System.Collections;
using System.Collections.Generic;
namespace DownBelow.Entity
{
    [Serializable]
    public class SpellAction : EntityAction
    {
        public List<Spell> Spells;

        public override void ExecuteAction(CharacterEntity RefEntity, Action EndCallback)
        {
            base.ExecuteAction(RefEntity, EndCallback);

            Spells[0].ExecuteSpell(null, null);
        }
    }
}