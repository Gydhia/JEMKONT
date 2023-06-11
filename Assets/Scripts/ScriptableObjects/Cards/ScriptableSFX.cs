using DG.Tweening;
using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Spells;
using EODE.Wonderland;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Mechanics
{
    public enum ESFXTravelType
    {
        Instantaneous,
        ProjectileToEnemy,
        //We can imagine others:
        ProjectileFromEnemy,
        ProjectileBackAndForth,
        InPlaceProjectile
    }
    [CreateAssetMenu(menuName = "DownBelow/Cards/SpellSFX")]
    public class ScriptableSFX : ScriptableObject
    {
        public GameObject Prefab;
        [Tooltip("This also defines when the spells are going to be applied.")]
        public ESFXTravelType TravelType;
        //Sounds too, one day,maybe...
        [EnableIf("@TravelType != ESFXTravelType.Instantaneous")]
        public float TravelDuration = 0.35f;
        [ShowIf("@TravelType == ESFXTravelType.InPlaceProjectile")]
        public float TravelUnit = 1.2f;

    }
    #region runtimeData
    public class RuntimeSFXData
    {
        public GameObject Prefab;
        public ESFXTravelType TravelType;
        public float TravelDuration = 0.35f;
        public float TravelUnit = 1.2f;
        public CharacterEntity caster;
        public Cell target;
        public Spell spell;

        public SFXEventData.Event OnSFXStarted;
        public SFXEventData.Event OnSFXEnded;

        public RuntimeSFXData(ScriptableSFX sfx, CharacterEntity caster, Cell target, Spell spell)
        {
            if (sfx == null) return;
            Prefab = sfx.Prefab;
            TravelType = sfx.TravelType;
            TravelDuration = sfx.TravelDuration;
            TravelUnit = sfx.TravelUnit;
            this.caster = caster;
            this.target = target;
            this.spell = spell;
        }
    }

    public class SFXEventData : EventData<SFXEventData>
    {
        public RuntimeSFXData SfxData;
        public CharacterEntity caster;
        public Cell target;
        public Spell spell;

        public SFXEventData(RuntimeSFXData SfxData)
        {
            this.SfxData = SfxData;
            this.caster = SfxData.caster;
            this.target = SfxData.target;
            this.spell = SfxData.spell;
        }
    }
    #endregion
}