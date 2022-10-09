using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Managers
{
    public class InputManager : _baseManager<InputManager>
    {
   
        public void ChangeCursorAppearance(CursorAppearance newAppearance)
        {
            switch (newAppearance)
            {
                case CursorAppearance.Idle:
                    Cursor.SetCursor(null, new Vector2(0f, 20f), CursorMode.Auto);
                    break;
                case CursorAppearance.Card:
                    Cursor.SetCursor(SettingsManager.Instance.InputPreset.CardCursor, new Vector2(0f, 20f), CursorMode.Auto);
                    break;
            }
        }
    }

    public enum CursorAppearance
    {
        Idle = 0,
        Card = 1
    }
}