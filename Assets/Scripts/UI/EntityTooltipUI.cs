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

        public void Init(CharacterEntity Entity)
        {
            _attackStat.text = Entity.Strength.ToString();
            _defenseStat.text = Entity.ShieldFill.ToString();
            _healthStat.text = Entity.HealthFill.ToString();
            _rangeStat.text = Entity.Range.ToString();

        }
    }

}
