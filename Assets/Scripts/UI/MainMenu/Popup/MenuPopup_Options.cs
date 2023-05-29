using System;
using System.Collections;
using System.Collections.Generic;
using DownBelow.UI.Menu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DisplayModes
{
FullScreen = 0,
Borderless = 1,
Windowed = 2,
Count
}

public class MenuPopup_Options : BaseMenuPopup
{
    [SerializeField] private Button _visualButton, _audioButton, _gameplayButton, _validateDisplayModeButton;
    [SerializeField] private GameObject _visualPanel, _audioPanel, _gameplayPanel;
    [SerializeField] private Slider _brightnessSlider, _gameVolumeSlider, _musicVolumeSlider, _effectsVolumeSlider;
    [SerializeField] private TMP_Dropdown _displayModeDropDown;

    private void OnEnable()
    {
        _visualButton.onClick.AddListener(OpenVisualPanel);
        _audioButton.onClick.AddListener(OpenAudioPanel);
        _gameplayButton.onClick.AddListener(OpenGameplaylPanel);
        _validateDisplayModeButton.onClick.AddListener(OnDisplayModeValidated);
        _brightnessSlider.onValueChanged.AddListener(OnBrightnessUpdated);
        _gameVolumeSlider.onValueChanged.AddListener(OnGameVolumeUpdated);
        _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeUpdated);
        _effectsVolumeSlider.onValueChanged.AddListener(OnEffectsVolumeUpdated);
    }

    private void OnDisable()
    {
        _visualButton.onClick.RemoveListener(OpenVisualPanel);
        _audioButton.onClick.RemoveListener(OpenAudioPanel);
        _gameplayButton.onClick.RemoveListener(OpenGameplaylPanel);
        _validateDisplayModeButton.onClick.RemoveListener(OnDisplayModeValidated);
        _brightnessSlider.onValueChanged.RemoveListener(OnBrightnessUpdated);
        _gameVolumeSlider.onValueChanged.RemoveListener(OnGameVolumeUpdated);
        _musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeUpdated);
        _effectsVolumeSlider.onValueChanged.RemoveListener(OnEffectsVolumeUpdated);
    }

    #region private Methods

    private void CloseEverything()
    {
        _visualPanel.SetActive(false);
        _audioPanel.SetActive(false);
        _gameplayPanel.SetActive(false);
    }

    private void OpenVisualPanel()
    {
        CloseEverything();
        _visualPanel.SetActive(true);
    }
    private void OpenAudioPanel()
    {
        CloseEverything();
        _audioPanel.SetActive(true);
    }
    private void OpenGameplaylPanel()
    {
        CloseEverything();
        _gameplayPanel.SetActive(true);
    }

    private void OnDisplayModeValidated()
    {
        OptionsManager.Instance.UpdateDisplayMode((DisplayModes)_displayModeDropDown.value);
    }

    private void OnBrightnessUpdated(float value)
    {
        OptionsManager.Instance.UpdateBrightness(_brightnessSlider.value);
    }

    private void OnGameVolumeUpdated(float value)
    {
        AkSoundEngine.SetRTPCValue("RTPC_Volume_Master", value, AkSoundEngine.AK_INVALID_GAME_OBJECT);
    }

    private void OnMusicVolumeUpdated(float value)
    {
        AkSoundEngine.SetRTPCValue("RTPC_Volume_Music", value, AkSoundEngine.AK_INVALID_GAME_OBJECT);
    }

    private void OnEffectsVolumeUpdated(float value)
    {
        AkSoundEngine.SetRTPCValue("RTPC_Volume_SFX", value, AkSoundEngine.AK_INVALID_GAME_OBJECT);
    }

    #endregion
   


}
