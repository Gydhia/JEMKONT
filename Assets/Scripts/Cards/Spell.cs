using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Mechanics;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DownBelow.Spells
{
    public class Spell : EntityAction
    {
        public Spell ParentSpell;
        public SpellCondition ConditionData;
        public SpellAction ActionData;

        public static Color AnchorColor = new(.584313725f, .741176471f, 1f, 1f);
        public static Color SelectedColor = new(.874509804f, 1f, .847058824f, 1f);
        public static Color NotSelectedColor = new(0f, 0f, 0f, 0.5f);


        public bool CanTargetAnyEntityOnGrid;
        [DetailedInfoBox("Infos", "Cette grille représente les cases où le sort peut-être lancé. Légende:\nBleu: Position relative du joueur sur la grille.\nVert: case sur laquelle le joueur peut caster son spell.\nNoir: case sur laquelle le joueur ne peut pas caster son spell.")]
        [TableMatrix(DrawElementMethod = "_processDrawSpellCasting", SquareCells = true, ResizableColumns = false, HorizontalTitle = nameof(CastingMatrix)), OdinSerialize, ShowIf("@CanTargetAnyEntityOnGrid == false"), HorizontalGroup("Grids")]
        public bool[,] CastingMatrix;

        [DetailedInfoBox("Infos", "Cette grille représente l'aire d'effet du sort. Légende:\nBleu: Position relative de la case où le spell sera cast sur la grille.\nVert: cases que le spell affecte.\nNoir: cases que le spell n'affecte pas.")]
        [TableMatrix(DrawElementMethod = "_processDrawSpellShape", SquareCells = true, ResizableColumns = false, HorizontalTitle = nameof(SpellShapeMatrix)), OdinSerialize, HorizontalGroup("Grids")]
        public bool[,] SpellShapeMatrix = new bool[5, 5];
#if UNITY_EDITOR
        private bool _processDrawSpellShape(Rect rect, bool value, int x, int y)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                Event.current.Use();

                //OnValueChanged!!!
                Debug.Log($"OldLengthes:{SpellShapeMatrix.GetLength(0)} {SpellShapeMatrix.GetLength(1)}");
                if (x == SpellShapeMatrix.GetLength(0) - 1)
                {
                    //Add column
                    SpellShapeMatrix = ArrayHelper.ResizeArrayTwo(SpellShapeMatrix, SpellShapeMatrix.GetLength(0) + 1, SpellShapeMatrix.GetLength(1), false);
                }
                if (y == SpellShapeMatrix.GetLength(1) - 1)
                {
                    //Add line
                    SpellShapeMatrix = ArrayHelper.ResizeArrayTwo(SpellShapeMatrix, SpellShapeMatrix.GetLength(0), SpellShapeMatrix.GetLength(1) + 1, false);
                }
                Debug.Log($"New Lengthes:{SpellShapeMatrix.GetLength(0)} {SpellShapeMatrix.GetLength(1)}");

            }
            return _drawCell(rect, value, x, y);
        }
        private bool _processDrawSpellCasting(Rect rect, bool value, int x, int y)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                Event.current.Use();
                //OnValueChanged!!!
                Debug.Log($"OldLengthes:{CastingMatrix.GetLength(0)} {CastingMatrix.GetLength(1)}");
                if (x == CastingMatrix.GetLength(0) - 1)
                {
                    //Add column
                    CastingMatrix = ArrayHelper.ResizeArrayTwo(CastingMatrix, CastingMatrix.GetLength(0) + 1, CastingMatrix.GetLength(1), false);
                }
                if (y == CastingMatrix.GetLength(1) - 1)
                {
                    //Add line
                    CastingMatrix = ArrayHelper.ResizeArrayTwo(CastingMatrix, CastingMatrix.GetLength(0), CastingMatrix.GetLength(1) + 1, false);
                }
                Debug.Log($"New Lengthes:{CastingMatrix.GetLength(0)} {CastingMatrix.GetLength(1)}");
            }
            return _drawCell(rect, value, x, y);
        }
        private static bool _drawCell(Rect rect, bool value, int x, int y)
        {
            Color color = value ? color = SelectedColor : NotSelectedColor;
            if (x == 2 && y == 2)
            {
                EditorGUI.DrawRect(
              rect.Padding(1),
              AnchorColor);
                EditorGUI.DrawRect(
                    new Rect(rect)
                    {
                        size = rect.size / 2,
                        x = rect.x + (rect.size.x / 4),
                        y = rect.y + (rect.size.y / 4)
                    }, color);
                //Achor cell
            } 
            else
            {
                EditorGUI.DrawRect(
                              rect.Padding(1),
                              color);
            }
            return value;
        }
        [OnInspectorInit]
        private void InitData()
        {
            CastingMatrix ??= new bool[5, 5];
            SpellShapeMatrix ??= new bool[5, 5];
        }
#endif
        public bool ApplyToCell = true;
        public bool ApplyToSelf = false;
        [EnableIf("@!this.ApplyToEnemies && !this.ApplyToAll")]
        public bool ApplyToAllies = false;
        [EnableIf("@!this.ApplyToAllies && !this.ApplyToAll")]
        public bool ApplyToEnemies = false;
        [EnableIf("@!this.ApplyToEnemies && !this.ApplyToAllies")]
        public bool ApplyToAll = false;


        // The instantiated spell
        [HideInInspector]
        public SpellAction CurrentAction;
        [HideInInspector]
        public CharacterEntity Caster;

        public Spell(CharacterEntity RefEntity, Cell TargetCell, SpellAction ActionData, SpellCondition ConditionData = null, bool ApplyToCell = true, bool ApplyToSelf = false, bool ApplyToAllies = false, bool ApplyToAll = false)
            : base(RefEntity, TargetCell)
        {
            this.ActionData = ActionData;
            this.ConditionData = ConditionData;
            this.ApplyToCell = ApplyToCell;
            this.ApplyToSelf = ApplyToSelf;
            this.ApplyToAllies = ApplyToAllies;
            this.ApplyToAll = ApplyToAll;
        }

        public void Init(Spell parentSpell)
        {
            this.ParentSpell = parentSpell;
        }

        public override void ExecuteAction()
        {
            this.CurrentAction = UnityEngine.Object.Instantiate(this.ActionData, Vector3.zero, Quaternion.identity, CombatManager.Instance.CurrentPlayingEntity.gameObject.transform);
            this.CurrentAction.Execute(this.GetTargets(RefEntity, TargetCell), this);
            EndAction();
        }

        public List<CharacterEntity> GetTargets(CharacterEntity caster, GridSystem.Cell cellTarget)
        {
            List<CharacterEntity> targets = new List<CharacterEntity>();
            //TODO: If getTarget doesn't get a target in a cell, cancel the spell?
            if (this.ApplyToCell && cellTarget.EntityIn != null)
                targets.Add(cellTarget.EntityIn);

            if (this.ApplyToSelf && !targets.Contains(caster))
                targets.Add(caster);

            if (this.ApplyToAllies)
            {
                foreach (CharacterEntity item in CombatManager.Instance.PlayingEntities)
                {
                    if (item.IsAlly && !targets.Contains(item))
                        targets.Add(item);
                }
            }

            if (this.ApplyToEnemies)
            {
                foreach (CharacterEntity item in CombatManager.Instance.PlayingEntities)
                {
                    if (!item.IsAlly && !targets.Contains(item))
                        targets.Add(item);
                }
            }

            if (this.ApplyToAll)
            {
                foreach (CharacterEntity item in CombatManager.Instance.PlayingEntities)
                {
                    if (!targets.Contains(item))
                        targets.Add(item);
                }
            }

            return targets;
        }

        public override object[] GetDatas()
        {
            // temporary
            return new object[0];
        }

        public override void SetDatas(object[] Datas)
        {
            throw new NotImplementedException();
        }
    }
}