using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public class MainMenuButton : MonoBehaviour
{
    //Properties
    public MenuButtonsTypes Type => _type;
    //Fields
    [SerializeField] private MenuButtonsTypes _type;
    [SerializeField] private RectTransform _buttonTransform;

    public Action<MenuButtonsTypes> OnMouseOverButton;
    public Action OnMouseExitButton;

    private bool _isOnButton = false;

    private void Update()
    {
        if(!_isOnButton && RectTransformUtility.RectangleContainsScreenPoint(_buttonTransform, Input.mousePosition))
        {
            _isOnButton = true;
            OnMouseOverButton?.Invoke(Type);
        }
        else if (_isOnButton && !RectTransformUtility.RectangleContainsScreenPoint(_buttonTransform, Input.mousePosition))
        {
            _isOnButton = false;
            OnMouseExitButton?.Invoke();
        }
    }

    public void ResetEventSent()
    {
        _isOnButton = false;
    }
}
