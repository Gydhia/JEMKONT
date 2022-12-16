using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DownBelow.Entity;

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


        // /!\
        // TODO : When initing, sub this to entity events

        public void Init(CharacterEntity Entity)
        {
            _attackStat.text = Entity.Strength.ToString();
            _defenseStat.text = Entity.Shield.ToString();
            _healthStat.text = Entity.Health.ToString();
            _rangeStat.text = Entity.Range.ToString();
        }
    }

}
