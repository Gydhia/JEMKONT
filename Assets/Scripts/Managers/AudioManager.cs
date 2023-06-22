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

        private bool _inited = false;


        private void SetVolumes()
        {
            AkSoundEngine.SetRTPCValue("RTPC_Volume_Master", 1f, AkSoundEngine.AK_INVALID_GAME_OBJECT);

            AkSoundEngine.SetRTPCValue("RTPC_Volume_Music", 1f, AkSoundEngine.AK_INVALID_GAME_OBJECT);

            AkSoundEngine.SetRTPCValue("RTPC_Volume_SFX", 1f, AkSoundEngine.AK_INVALID_GAME_OBJECT);

        }

        public void Init()
        {
            if (!AudioHolder.HasMusicStarted) 
            { 
                this.SetVolumes();
                AkSoundEngine.PostEvent("StartMusic", AudioHolder.Instance.gameObject);
                AudioHolder.HasMusicStarted = true;
                AkSoundEngine.PostEvent("SetMainMenu", AudioHolder.Instance.gameObject);
                AkSoundEngine.PostEvent("Set_Layer_0", AudioHolder.Instance.gameObject);
                AkSoundEngine.SetState("World", "Overworld");
            }
            

            if(CombatManager.Instance != null && !_inited)
            {
                this._inited = true;

                GameManager.Instance.OnEnteredGrid += SetExploreMusic;
                GameManager.Instance.OnGameStarted += SetExploreMusic;

                CombatManager.Instance.OnCombatStarted += SetCombatMusic;
                CombatManager.Instance.OnCombatEnded += SetExploreMusic;

                

                
            }
        }

        public void SetExploreMusic(GameEventData a)
        {
            AkSoundEngine.PostEvent("SetExplore", AudioHolder.Instance.gameObject);
        }
        public void SetExploreMusic(EntityEventData a)
        {
            if (a.Entity == GameManager.RealSelfPlayer && a.Entity.CurrentGrid.UName == GridManager.Instance.MainGrid)
            {
                // Farmland
                AkSoundEngine.SetState("World", "Overworld");
            }
            else if (a.Entity == GameManager.RealSelfPlayer && a.Entity.CurrentGrid.UName != GridManager.Instance.MainGrid)
            {
                // Abyss
                AkSoundEngine.SetState("World", "Abyss");
            }
            if (a.Entity == GameManager.RealSelfPlayer && !a.Entity.CurrentGrid.IsCombatGrid)
            {
                AkSoundEngine.PostEvent("SetExplore", AudioHolder.Instance.gameObject);
            }
            
        }
        public void SetExploreMusic(GridEventData a)
        {
            AkSoundEngine.PostEvent("SetExplore", AudioHolder.Instance.gameObject);
        }
        public void SetCombatMusic(GridEventData a)
        {
            AkSoundEngine.PostEvent("SetCombat", AudioHolder.Instance.gameObject); 
        }
        public void SetOverworld(GridEventData a)
        {
            AkSoundEngine.SetState("World", "Overworld");
        }
        public void SetAbyss(GridEventData a)
        {
            AkSoundEngine.SetState("World", "Abyss");
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

        //private void _newSettings(string ConfigName, string SettingName, object Value)
        //{
        //    if (ConfigName == "player" &&
        //        (SettingName == "audio_musicvolume" || SettingName == "audio_soundvolume" ||
        //         SettingName == "audio_mastervolume" || SettingName == "audio_ambiantvolume" || SettingName == "audio_uivolume"))
        //        this.SetVolumes();
        //}

        //private void _playSound(AudioFeedbackData Data)
        //{
        //    if (Data != null)
        //    {
        //        if (Data.AudioRef.WwiseObjectReference != null)
        //        {
        //            Data.AudioRef.Post(Data.SoundHolder);
        //        }
        //    }
        //}

        //private void _startPlayback(GameEventData Data)
        //{
        //    if (!AudioHolder.HasMusicStarted)
        //    {
        //        AkSoundEngine.PostEvent("StartMusic", AudioHolder.Instance.gameObject);
        //        AudioHolder.HasMusicStarted = true;
        //    }
        //
        //    // if Menu != null, we're in MainMenu
        //    if (MenuManager.Instance != null)
        //    {
        //        if (AudioHolder.Instance.AmbienceBedGenericID.HasValue)
        //        {
        //            AkSoundEngine.StopPlayingID(AudioHolder.Instance.AmbienceBedGenericID.Value);
        //            AudioHolder.Instance.AmbienceBedGenericID = null;
        //        }
        //
        //        //AkSoundEngine.SetState("GameState", "MainMenu");
        //        AkSoundEngine.PostEvent("SetMainMenu", AudioHolder.Instance.gameObject);
        //        AkSoundEngine.PostEvent("Set_Layer_0", AudioHolder.Instance.gameObject);
        //    }
        //    else
        //    {
        //        // Stop the menu ambience 
        //        if (AudioHolder.Instance.AmbienceMenuSoundID.HasValue)
        //        {
        //            AkSoundEngine.StopPlayingID(AudioHolder.Instance.AmbienceMenuSoundID.Value);
        //            AudioHolder.Instance.AmbienceMenuSoundID = null;
        //        }
        //
        //        AkSoundEngine.SetState("GameState", "Explore");
        //    }
        //}

        //private void _changeSceneMusic(GameEventData Data)
        //{
        //    // If we're comming from Game_dev
        //    if (MenuManager.Instance == null)
        //    {
        //        // Start the menu ambience sound
        //        if (!AudioHolder.Instance.AmbienceMenuSoundID.HasValue)
        //        {
        //            //AudioHolder.Instance.AmbienceMenuSoundID = SettingsManager.Instance.AudioPreset.audio_ambiance_mainmenu.Post(AudioHolder.Instance.gameObject);
        //        }
        //
        //        // Cut every remaining sounds of game_dev just to be sure
        //        AkSoundEngine.SetRTPCValue("RTPC_Surrounding_Citizens", 0f, AkSoundEngine.AK_INVALID_GAME_OBJECT);
        //
        //        if (AudioHolder.Instance.AmbienceBedGenericID.HasValue)
        //        {
        //            AkSoundEngine.StopPlayingID(AudioHolder.Instance.AmbienceBedGenericID.Value);
        //            AudioHolder.Instance.AmbienceBedGenericID = null;
        //        }
        //
        //        if (AudioHolder.Instance.AmbienceCrowdGenericID.HasValue)
        //        {
        //            AkSoundEngine.StopPlayingID(AudioHolder.Instance.AmbienceCrowdGenericID.Value);
        //            AudioHolder.Instance.AmbienceCrowdGenericID = null;
        //        }
        //    }
        //
        //    // Then filter the music
        //    AkSoundEngine.SetState("State_MusicFilter", "Filtered");
        //}


    }
}