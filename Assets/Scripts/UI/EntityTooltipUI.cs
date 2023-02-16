using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.Managers;

namespace DownBelow.UI
{
    public class EntityTooltipUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _entityName;
        [SerializeField] private Image _entityIllustrations;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _attackStat;
        [SerializeField] private TextMeshProUGUI _defenseStat;
        [SerializeField] private TextMeshProUGUI _healthStat;
        [SerializeField] private TextMeshProUGUI _rangeStat;

        private CharacterEntity _currentEntity;

        private EntityPreset _spawnablePreset;
        // /!\
        // TODO : When initing, sub this to entity events

        public void Init(CharacterEntity Entity)
        {
            if (!_currentEntity)
            {
                _currentEntity = Entity;
            }
            else
            {
                ResetEvents();
            }

            _currentEntity = Entity;
            _spawnablePreset = Entity.Preset as EntityPreset;

            SetEvents();
            _entityName.text = _spawnablePreset.UName;
            _entityIllustrations.sprite = _spawnablePreset.Icon;
            _attackStat.text = _currentEntity.Strength.ToString();
            _defenseStat.text = _currentEntity.Shield.ToString();
            _healthStat.text = _currentEntity.Health.ToString();
            _rangeStat.text = _currentEntity.Range.ToString();
            
            
            
        }
        private void SetEvents()
        {
            _currentEntity.OnAlterationReceived += SetAlterations;
            _currentEntity.OnDefenseAdded += SetDefense;
            _currentEntity.OnDefenseRemoved += SetDefense;
            _currentEntity.OnStrengthAdded += SetAttack;
            _currentEntity.OnStrengthRemoved += SetAttack;
            _currentEntity.OnHealthAdded += SetHealth;
            _currentEntity.OnHealthRemoved += SetHealth;
            _currentEntity.OnRangeAdded += SetRange;
            _currentEntity.OnRangeRemoved += SetRange;
        }
        private void ResetEvents()
        {
            _currentEntity.OnAlterationReceived -= SetAlterations;
            _currentEntity.OnDefenseAdded -= SetDefense;
            _currentEntity.OnDefenseRemoved -= SetDefense;
            _currentEntity.OnStrengthAdded -= SetAttack;
            _currentEntity.OnStrengthRemoved -= SetAttack;
            _currentEntity.OnHealthAdded -= SetHealth;
            _currentEntity.OnHealthRemoved -= SetHealth;
            _currentEntity.OnRangeAdded -= SetRange;
            _currentEntity.OnRangeRemoved -= SetRange;
        }
        private void SetAlterations(SpellEventData Data)
        {

        }
        private void SetDefense(SpellEventData Data)
        {
            _defenseStat.text = _currentEntity.Shield.ToString();
        }
        private void SetAttack(SpellEventData Data)
        {
            _attackStat.text = _currentEntity.Strength.ToString();
        }
        private void SetHealth(SpellEventData Data)
        {
            _healthStat.text = _currentEntity.Health.ToString();
        }
        private void SetRange(SpellEventData Data)
        {
            _rangeStat.text = _currentEntity.Range.ToString();
        }
    }

}
