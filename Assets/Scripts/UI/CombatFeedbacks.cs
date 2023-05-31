using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DownBelow.Entity;
using DownBelow.Events;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using DownBelow.Managers;
using DownBelow.GridSystem;

public class CombatFeedbacks : MonoBehaviour
{
    #region FIELDS
    [SerializeField] private CharacterEntity _entity;
    [SerializeField] private TextMeshProUGUI _healthFeedback;
    [SerializeField] private Color _healthRemovedColor;
    [SerializeField] private Color _healthAddedColor;
    [SerializeField] private Image _targetImage;

    [SerializeField] private TextMeshProUGUI healthText;
    
    #endregion
    private Camera _mainCam;


    private void Awake()
    {
        this._mainCam = Camera.main;
        this._entity.OnInited += this.Init;   
    }

    public void Init(GameEventData Data)
    {
        this._entity.OnHealthRemoved += OnHealthRemoved;
        this._entity.OnHealthAdded += OnHealthAdded;
        this._entity.OnEntityTargetted += OnEntityTargetted;
        this._entity.OnStatisticsChanged += _refresh;
        CombatManager.Instance.OnCombatStarted += ShowHealthText;
        CombatManager.Instance.OnCombatEnded += HidehealthText;

        this._healthFeedback.gameObject.SetActive(false);
        this.healthText.gameObject.SetActive(this._entity.CurrentGrid is CombatGrid cGrid && cGrid.HasStarted);
        //Maybe for later add feedbacks for the other effects
    }

    private void OnDestroy()
    {
        this._entity.OnHealthRemoved -= OnHealthRemoved;
        this._entity.OnHealthAdded -= OnHealthAdded;
        this._entity.OnEntityTargetted -= OnEntityTargetted;
        this._entity.OnStatisticsChanged -= _refresh;
        if(CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStarted -= ShowHealthText;
            CombatManager.Instance.OnCombatEnded -= HidehealthText;
        }
    }

    private void LateUpdate()
    {
      //  this.HealthFill.transform.LookAt(Camera.main.transform.position);
        //this.ShieldFill.transform.LookAt(Camera.main.transform.position);
    }

    private void _refresh(GameEventData data)
    {
        this.healthText.text = this._entity.Health.ToString();
    }


    private void OnHealthRemoved(SpellEventData Data)
    {
        if(Data.Value > 0)
        {
            _healthFeedback.text = "-" + Data.Value.ToString();
            _healthFeedback.color = _healthRemovedColor;
            _healthFeedback.transform.LookAt(_mainCam.transform.position);
            _healthFeedback.transform.Rotate(new Vector3(0, 180f, 0));
            _healthFeedback.gameObject.SetActive(true);
            _healthFeedback.rectTransform.DOShakePosition(1.5f, 0.5f,5).SetEase(Ease.InExpo).OnComplete(() => _healthFeedback.gameObject.SetActive(false));
        }
        
        UpdateUILife(Data);
    }

    private void ShowHealthText(GridEventData data)
    {
        this.healthText.text = this._entity.Health.ToString();
        healthText.gameObject.SetActive(true);
    }
    private void HidehealthText(GridEventData data)
    {
        healthText.gameObject.SetActive(false);
    }


    private void OnHealthAdded(SpellEventData Data)
    {
        if(Data.Value < 0)
        {
            _healthFeedback.text = "+" + Mathf.Abs(Data.Value).ToString();
            _healthFeedback.color = _healthAddedColor;
            _healthFeedback.transform.LookAt(_mainCam.transform.position);
            _healthFeedback.transform.Rotate(new Vector3(0f, 180f, 0));
            _healthFeedback.gameObject.SetActive(true);
            _healthFeedback.rectTransform.DOShakePosition(1.5f, 0.5f, 5).SetEase(Ease.InExpo).OnComplete(() => _healthFeedback.gameObject.SetActive(false));
        }
        
        UpdateUILife(Data);
    }

    private void OnEntityTargetted(EntityEventData Data)
    {
        this._targetImage
            .DOFade(1f, 0.35f)
            .SetEase(Ease.InExpo)
            .OnComplete(() => this._targetImage.DOFade(0f, 0.35f));
    }
    
    public void UpdateUILife(SpellEventData data)
    {
        int oldLife = int.Parse(this.healthText.text);
        if (oldLife > this._entity.Health)
        {
            this.healthText.DOColor(Color.red, 0.4f).SetEase(Ease.OutQuad);
            this.healthText.transform.DOShakeRotation(0.8f).SetEase(Ease.OutQuad).OnComplete(() =>
                this.healthText.DOColor(Color.white, 0.2f).SetEase(Ease.OutQuad));
            this.healthText.text = this._entity.Health.ToString();
        } else
        {
            this.healthText.DOColor(Color.green, 0.4f).SetEase(Ease.OutQuad);
            this.healthText.transform.DOPunchScale(Vector3.one * 1.3f, 0.8f).SetEase(Ease.OutQuad).OnComplete(() =>
                this.healthText.DOColor(Color.white, 0.2f).SetEase(Ease.OutQuad));
            this.healthText.text = this._entity.Health.ToString();
        }
    }
}
