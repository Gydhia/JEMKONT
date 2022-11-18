using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;



public enum MenuButtonsTypes
{
    SOLO,
    HOST,
    JOIN,
    OPTIONS,
    QUIT,
    COUNT
}

public class MainMenuUI : MonoBehaviour
{
    //Fields 
    [Header("Buttons")]
    [SerializeField] private Button _soloPlayButton;
    [SerializeField] private Button _hostGameButton;
    [SerializeField] private Button _joinGameButton;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _quitButton;

    [SerializeField] private List<MainMenuButton> mainMenuButtons;

    [SerializeField] private float _buttonsOffset = 5f;

    private RectTransform _soloPlayRect, _hostGameRect, _joinGameRect, _optionsRect, _quitRect;

    private bool _buttonAnimated = false;

    private Vector2 _currentlySelectedButtonMaxAnchor;
    private Vector2 _currentlySelectedButtonMinAnchor;
    private RectTransform _currentlySelectedButtonTransform;

    private void Start()
    {
        _soloPlayRect = _soloPlayButton.GetComponent<RectTransform>();
        _hostGameRect = _hostGameButton.GetComponent<RectTransform>();
        _joinGameRect = _joinGameButton.GetComponent<RectTransform>();
        _optionsRect = _optionsButton.GetComponent<RectTransform>();
        _quitRect = _quitButton.GetComponent<RectTransform>();

        _currentlySelectedButtonMaxAnchor =_soloPlayRect.anchorMax;
        _currentlySelectedButtonMinAnchor =_soloPlayRect.anchorMin;
    }

    private void OnEnable()
    {
        _soloPlayButton.onClick.AddListener(OnSoloPlayButtonCliqued);
        _hostGameButton.onClick.AddListener(OnHostGameButtonCliqued); 
        _joinGameButton.onClick.AddListener(OnJoinGameButtonClicked); 
        _optionsButton.onClick.AddListener(OnOptionsBittonCliqued);  
        _quitButton.onClick.AddListener(OnQuitButtonCliqued);

        for (int i = 0; i < mainMenuButtons.Count; i++)
        {
            mainMenuButtons[i].OnMouseOverButton += OnMouseOverButton;
            mainMenuButtons[i].OnMouseExitButton += OnMouseExitButton;
        }
    }

    private void OnDisable()
    {
        _soloPlayButton.onClick.RemoveListener(OnSoloPlayButtonCliqued);
        _hostGameButton.onClick.RemoveListener(OnHostGameButtonCliqued);
        _joinGameButton.onClick.RemoveListener(OnJoinGameButtonClicked); 
        _optionsButton.onClick.RemoveListener(OnOptionsBittonCliqued);
        _quitButton.onClick.RemoveListener(OnQuitButtonCliqued);

        for (int i = 0; i < mainMenuButtons.Count; i++)
        {
            mainMenuButtons[i].OnMouseOverButton -= OnMouseOverButton;
            mainMenuButtons[i].OnMouseExitButton -= OnMouseExitButton;
        }
    }

    private void Update()
    {

    }

    #region private methods
    private void OnSoloPlayButtonCliqued()
    {

    }

    private void OnHostGameButtonCliqued()
    {

    }

    private void OnJoinGameButtonClicked()
    {

    }

    private void OnOptionsBittonCliqued()
    {

    }

    private void OnQuitButtonCliqued()
    {

    }

    private void OnMouseOverButton(MenuButtonsTypes type)
    {

        if (!_buttonAnimated)
        {
            switch (type)
            {
                case MenuButtonsTypes.SOLO:
                    MouseOverButtonAnimation(_soloPlayRect);
                    Debug.Log("OVER : SOLO");
                    break;
                case MenuButtonsTypes.HOST:
                    MouseOverButtonAnimation(_hostGameRect);
                    Debug.Log("OVER : HOST");
                    break;
                case MenuButtonsTypes.JOIN:
                    MouseOverButtonAnimation(_joinGameRect);
                    Debug.Log("OVER : JOIN");
                    break;
                case MenuButtonsTypes.OPTIONS:
                    MouseOverButtonAnimation(_optionsRect);
                    Debug.Log("OVER : OPTIONS");
                    break;
                case MenuButtonsTypes.QUIT:
                    MouseOverButtonAnimation(_quitRect);
                    Debug.Log("OVER : QUIT");
                    break;
                default:
                    break;
            }

            _buttonAnimated = true;
        }
           
    }

    private void MouseOverButtonAnimation(RectTransform transform)
    {
        _currentlySelectedButtonTransform = transform;

        _currentlySelectedButtonTransform.DOAnchorMax(new Vector2(_currentlySelectedButtonMaxAnchor.x - _buttonsOffset, _currentlySelectedButtonTransform.anchorMax.y), 0.3f).SetEase(Ease.OutQuint);
        _currentlySelectedButtonTransform.DOAnchorMin(new Vector2(_currentlySelectedButtonMinAnchor.x - _buttonsOffset, _currentlySelectedButtonTransform.anchorMin.y), 0.3f).SetEase(Ease.OutQuint);
    }
    private void ResetButtonposition()
    {

        _buttonAnimated = false;
        _currentlySelectedButtonTransform.DOAnchorMax(new Vector2(_currentlySelectedButtonMaxAnchor.x, _currentlySelectedButtonTransform.anchorMax.y), 0.3f).SetEase(Ease.OutQuint);
        _currentlySelectedButtonTransform.DOAnchorMin(new Vector2(_currentlySelectedButtonMinAnchor.x, _currentlySelectedButtonTransform.anchorMin.y), 0.3f).SetEase(Ease.OutQuint);
    }

    private void OnMouseExitButton()
    {
        ResetButtonposition();

        for (int i = 0; i < mainMenuButtons.Count; i++)
        {
            mainMenuButtons[i].ResetEventSent();
        }
    }
    #endregion
}
