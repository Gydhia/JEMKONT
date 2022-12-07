using Jemkont.Events;
using Jemkont.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Managers
{
    public class UIManager : _baseManager<UIManager>
    {
        public UIStaticTurnSection TurnSection;
        public UIPlayerInfos PlayerInfos;
        public UICardSection CardSection;

        public void Init()
        {
            this.TurnSection.gameObject.SetActive(false);
            this.PlayerInfos.gameObject.SetActive(false);
            this.CardSection.gameObject.SetActive(false);

            GameManager.Instance.OnEnteredGrid += this.SetupCombatInterface;
        }

        public void SetupCombatInterface(EventData Data)
        {
            this.TurnSection.gameObject.SetActive(true);
            this.PlayerInfos.gameObject.SetActive(true);
            this.CardSection.gameObject.SetActive(true);
        }
    }
}

