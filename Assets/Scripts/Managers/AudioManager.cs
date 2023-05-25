using DownBelow.Events;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DownBelow.Managers
{
    public class AudioManager : _baseManager<AudioManager>
    {

        [ShowInInspector]
        [ReadOnly]
        private int _currentAudioUpdateFrame = 0;
        [SerializeField]
        private int _audioUpdateFrameRate = 15;
        public float SurroundingCitizenCount = 0;
        public float SurroundingCarCount = 0;
        [ShowInInspector]
        private bool _debugSurroundingCitizenRadius = false;

        private void SetVolumes()
        {
            AkSoundEngine.SetRTPCValue("RTPC_Volume_MASTER", 100f, AkSoundEngine.AK_INVALID_GAME_OBJECT);

            AkSoundEngine.SetRTPCValue("RTPC_Volume_MUSIC", 100f, AkSoundEngine.AK_INVALID_GAME_OBJECT);

            AkSoundEngine.SetRTPCValue("RTPC_Volume_SFX", 100f, AkSoundEngine.AK_INVALID_GAME_OBJECT);

        }

        public void Init()
        {

            this.SetVolumes();

            //GameController.Instance.OnBeforeSceneChange += _changeSceneMusic;
            //GameController.Instance.OnGameReady += _startPlayback;
            //GameController.Instance.OnAudioPlay += _startPlayback;
            //GameController.Instance.OnUserFeedback += _playSound;
            //GameController.Instance.OnSettingChange += _newSettings;
            //GameController.Instance.OnToolChanged += _toolChangedSound;
            //GameController.Instance.OnMapViewChanged += _filterMapView;

        }

        protected void Update()
        {
            if (!GameManager.GameStarted)
                return;

            // the necessary update of data required for audio are done every X frame for performance reasons
            this._currentAudioUpdateFrame++;

            if (this._currentAudioUpdateFrame < this._audioUpdateFrameRate)
                return;

            this._currentAudioUpdateFrame = 0;

            
        }

        private void _newSettings(string ConfigName, string SettingName, object Value)
        {
            if (ConfigName == "player" &&
                (SettingName == "audio_musicvolume" || SettingName == "audio_soundvolume" ||
                 SettingName == "audio_mastervolume" || SettingName == "audio_ambiantvolume" || SettingName == "audio_uivolume"))
                this.SetVolumes();
        }

        private void _playSound(AudioFeedbackData Data)
        {
            if (Data != null)
            {
                if (Data.AudioRef.WwiseObjectReference != null)
                {
                    Data.AudioRef.Post(Data.SoundHolder);
                }
            }
        }

        private void _startPlayback(GameEventData Data)
        {
            if (!AudioHolder.HasMusicStarted)
            {
                AkSoundEngine.PostEvent("Play_Music", AudioHolder.Instance.gameObject);
                AudioHolder.HasMusicStarted = true;
            }

            // if Menu != null, we're in MainMenu
            if (MenuManager.Instance != null)
            {
                if (AudioHolder.Instance.AmbienceBedGenericID.HasValue)
                {
                    AkSoundEngine.StopPlayingID(AudioHolder.Instance.AmbienceBedGenericID.Value);
                    AudioHolder.Instance.AmbienceBedGenericID = null;
                }

                AkSoundEngine.SetState("State_Music", "MainMenu");
            }
            else
            {
                //if (AudioHolder.Instance.AmbienceBedGenericID == null)
                //    AudioHolder.Instance.AmbienceBedGenericID = SettingsManager.Instance.AudioPreset.audio_ambiance_bed.Post(this.gameObject);
                //if (AudioHolder.Instance.AmbienceCrowdGenericID == null)
                //    AudioHolder.Instance.AmbienceCrowdGenericID = SettingsManager.Instance.AudioPreset.audio_ambiance_crowd.Post(this.gameObject);

                // Stop the menu ambience 
                if (AudioHolder.Instance.AmbienceMenuSoundID.HasValue)
                {
                    AkSoundEngine.StopPlayingID(AudioHolder.Instance.AmbienceMenuSoundID.Value);
                    AudioHolder.Instance.AmbienceMenuSoundID = null;
                }

                AkSoundEngine.SetState("State_Music", "InGame");
            }

            AkSoundEngine.SetState("State_MusicFilter", "Unfiltered");
        }

        private void _changeSceneMusic(GameEventData Data)
        {
            // If we're comming from Game_dev
            if (MenuManager.Instance == null)
            {
                // Start the menu ambience sound
                if (!AudioHolder.Instance.AmbienceMenuSoundID.HasValue)
                {
                    //AudioHolder.Instance.AmbienceMenuSoundID = SettingsManager.Instance.AudioPreset.audio_ambiance_mainmenu.Post(AudioHolder.Instance.gameObject);
                }

                // Cut every remaining sounds of game_dev just to be sure
                AkSoundEngine.SetRTPCValue("RTPC_Surrounding_Citizens", 0f, AkSoundEngine.AK_INVALID_GAME_OBJECT);

                if (AudioHolder.Instance.AmbienceBedGenericID.HasValue)
                {
                    AkSoundEngine.StopPlayingID(AudioHolder.Instance.AmbienceBedGenericID.Value);
                    AudioHolder.Instance.AmbienceBedGenericID = null;
                }

                if (AudioHolder.Instance.AmbienceCrowdGenericID.HasValue)
                {
                    AkSoundEngine.StopPlayingID(AudioHolder.Instance.AmbienceCrowdGenericID.Value);
                    AudioHolder.Instance.AmbienceCrowdGenericID = null;
                }
            }

            // Then filter the music
            AkSoundEngine.SetState("State_MusicFilter", "Filtered");
        }

        private void _filterMapView()
        {
            AkSoundEngine.SetState("State_MusicFilter", "Unfiltered");
        }

    }
}