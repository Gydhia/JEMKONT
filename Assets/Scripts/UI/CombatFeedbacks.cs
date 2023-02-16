using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DownBelow.Entity;
using DownBelow.Events;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using DownBelow.Spells.Alterations;
using DownBelow.UI;


public class CombatFeedbacks : MonoBehaviour
{
    #region FIELDS

    [SerializeField] private CharacterEntity _entity;
    [SerializeField] private TextMeshProUGUI _healthFeedback;
    [SerializeField] private Color _healthRemovedColor;
    [SerializeField] private Color _healthAddedColor;
    [SerializeField] private RectTransform _horizontalLayoutGroup;
    [SerializeField] private GameObject _alterationPrefab;
    [SerializeField] private AlterationDictionnary _dictionnary;

    #endregion

    private Camera _mainCam;
    private List<GameObject> _alterationsObjects = new List<GameObject>();


    private void OnEnable()
    {
        _entity.OnHealthRemoved += OnHealthRemoved;
        _entity.OnHealthAdded += OnHealthAdded;
        _healthFeedback.gameObject.SetActive(false);

        //Maybe for later add feedbacks for the other effects
    }

    private void Start()
    {
        _mainCam = Camera.main;
    }

    private void OnDisable()
    {
        _entity.OnHealthRemoved -= OnHealthRemoved;
        _entity.OnHealthAdded -= OnHealthAdded;
    }

    private void LateUpdate()
    {
        //  this.HealthFill.transform.LookAt(Camera.main.transform.position);
        //this.ShieldFill.transform.LookAt(Camera.main.transform.position);
    }

    private void OnHealthRemoved(SpellEventData Data)
    {
        if (Data.Value > 0)
        {
            _healthFeedback.text = "-" + Data.Value.ToString();
            _healthFeedback.color = _healthRemovedColor;
            _healthFeedback.transform.LookAt(_mainCam.transform.position);
            _healthFeedback.transform.Rotate(new Vector3(0, 180f, 0));
            _healthFeedback.gameObject.SetActive(true);
            _healthFeedback.rectTransform.DOShakePosition(1.5f, 0.5f, 5).SetEase(Ease.InExpo)
                .OnComplete(() => _healthFeedback.gameObject.SetActive(false));
        }
    }

    private void OnHealthAdded(SpellEventData Data)
    {
        if (Data.Value < 0)
        {
            _healthFeedback.text = "+" + Mathf.Abs(Data.Value).ToString();
            _healthFeedback.color = _healthAddedColor;
            _healthFeedback.transform.LookAt(_mainCam.transform.position);
            _healthFeedback.transform.Rotate(new Vector3(0f, 180f, 0));
            _healthFeedback.gameObject.SetActive(true);
            _healthFeedback.rectTransform.DOShakePosition(1.5f, 0.5f, 5).SetEase(Ease.InExpo)
                .OnComplete(() => _healthFeedback.gameObject.SetActive(false));
        }
    }

    private void OnAlterationAdded(EAlterationType alteration)
    {
        GameObject go = Instantiate(_alterationPrefab, _horizontalLayoutGroup);

        _alterationsObjects.Add(go);
        
        go.GetComponent<AlterationsFeedback>().SetAlteration(_dictionnary.DictionaryAlterations[alteration]);
    }
}