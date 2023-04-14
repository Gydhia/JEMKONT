using DownBelow.Entity;
using DownBelow.GridSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    public enum PushType
    {
        Left,
        Right,
        Top,
        Bottom,

        Inside,
        Outside
    }
    public class Spell_Push : Spell
    {
        [FoldoutGroup("PUSH Spell Datas")]
        public PushType PushType;
        [FoldoutGroup("PUSH Spell Datas")]
        public int PushAmount;

        public Spell_Push(CharacterEntity RefEntity, Cell TargetCell, SpellAction ActionData, SpellCondition ConditionData = null) 
            : base(RefEntity, TargetCell, ActionData, ConditionData)
        {
        }
    }
}