using DownBelow.Managers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI.Menu
{
    [RequireComponent(typeof(Button))]
    public class MenuButtonRedirect : MonoBehaviour
    {
        public MenuPopup PopupToGo = MenuPopup.None;

        [ShowIf("@PopupToGo", MenuPopup.SaveSelection)]
        public bool ToHost = false;

        private Button _selfButton;

        void Awake()
        {
            if(this.TryGetComponent<Button>(out this._selfButton))
            {
                switch (this.PopupToGo)
                {
                    case MenuPopup.Close:
                        this._selfButton.onClick.AddListener(() =>
                        {
                            if (MenuManager.Instance.LastPopup != MenuPopup.Room)
                            {
                                MenuManager.Instance.GoingToHost = false;
                            }
                            MenuManager.Instance.HideCurrentPopup();
                        });
                        break;
                    case MenuPopup.Quit:
                        this._selfButton.onClick.AddListener(() => MenuManager.Instance.OnClickQuit());
                        break;
                    case MenuPopup.Tutorial:
                        this._selfButton.onClick.AddListener(() => { GameManager.GoToTutorial(); });     
                        break;
                    default:
                        this._selfButton.onClick.AddListener(() => 
                        {
                            MenuManager.Instance.SelectPopup(this.PopupToGo);
                            MenuManager.Instance.GoingToHost = this.ToHost;
                        });
                        
                        break;
                }
            }
        }

       
    }
}
