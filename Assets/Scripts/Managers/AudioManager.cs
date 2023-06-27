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

                CombatManager.Instance.OnCombatStarted += SubscribeLayerChangeToPlayers;


            }
        }

        public void ChangeLayerOnHealthDown(SpellEventData s)
        {
            int totalMaxHealth = 0;
            int totalHealth = 0;
            foreach (var player in CombatManager.Instance.PlayersInGrid)
            {
                totalMaxHealth += player.MaxHealth;
                totalHealth += player.Health;
            }
            foreach (var player in CombatManager.Instance.FakePlayers)
            {
                totalMaxHealth += player.MaxHealth;
                totalHealth += player.Health;
            }

            if (totalHealth < totalMaxHealth * 0.75)
            {
                AkSoundEngine.PostEvent("Set_Layer_1", AudioHolder.Instance.gameObject);
                if (totalHealth < totalMaxHealth * 0.5)
                {
                    AkSoundEngine.PostEvent("Set_Layer_2", AudioHolder.Instance.gameObject);
                    if (totalHealth < totalMaxHealth * 0.25)
                    {
                        AkSoundEngine.PostEvent("Set_Layer_3", AudioHolder.Instance.gameObject);
                    }
                }
            }
        }
        public void SubscribeLayerChangeToPlayers(GridEventData g)
        {
            foreach(var player in CombatManager.Instance.PlayersInGrid)
            {
                player.OnHealthRemoved += ChangeLayerOnHealthDown;
            }
            foreach (var player in CombatManager.Instance.FakePlayers)
            {
                player.OnHealthRemoved += ChangeLayerOnHealthDown;
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
            AkSoundEngine.PostEvent("Set_Layer_0", AudioHolder.Instance.gameObject);

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

        


    }
}