using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using DownBelow.Mechanics;

namespace DownBelow.UI
{
    public class UIDialogSection : MonoBehaviour
    {
        public Image AvatarImage;
        public TextMeshProUGUI Dialog;
        public Image NextImage;
        public CanvasGroup SelfCanvas;

        public float letterDelay = 0.1f;
        [ReadOnly]
        public string displayText = "Hello, world!";

        private DialogPreset _currentDialogPreset;
        private Coroutine _dialogCoroutine = null;
        private int _currentDialogIndex = -1;

        public void Init()
        {
            this._toggleCanvas(false, false);
        }

        public void ShowDialog(DialogPreset preset)
        {
            if(this._dialogCoroutine != null)
            {
                StopCoroutine(this._dialogCoroutine);
                this._dialogCoroutine = null;
            }

            this._toggleCanvas(true, true);

            this._currentDialogPreset = preset;

            this.AvatarImage.sprite = preset.TalkerIcon;

            this._currentDialogIndex = -1;
            this.ShowNextText();

            PlayerInputs.player_interact.performed += this._tryNextDialog;
        }

        public void _tryNextDialog(InputAction.CallbackContext ctx) 
        { 
            // We should just show the full text, end the animation
            if(this._dialogCoroutine != null)
            {
                StopCoroutine(this._dialogCoroutine);
                this._dialogCoroutine = null;

                this.Dialog.text = this._currentDialogPreset.Dialogs[this._currentDialogIndex];
            }
            else
            {
                if(this._currentDialogIndex + 1 >= this._currentDialogPreset.Dialogs.Count)
                {
                    this.EndDialog();
                }
                else
                {
                    this.ShowNextText();
                }
            }
        }

        public void EndDialog()
        {
            this._toggleCanvas(false, true);
        }

        public void ShowNextText()
        {
            this._currentDialogIndex++;
            this.displayText = this._currentDialogPreset.Dialogs[this._currentDialogIndex];

            this.Dialog.text = string.Empty;
            this._dialogCoroutine = StartCoroutine(ShowTextWithDelay());
        }

        private void _toggleCanvas(bool enable, bool fade)
        {
            if (enable)
            {
                if (fade)
                    this.SelfCanvas.DOFade(1f, 0.4f);
                else
                    this.SelfCanvas.alpha = 1f;

                this.SelfCanvas.interactable = true;
                this.SelfCanvas.blocksRaycasts = true;
            }
            else
            {
                if (fade)
                    this.SelfCanvas.DOFade(0f, 0.4f);
                else
                    this.SelfCanvas.alpha = 0f;

                this.SelfCanvas.interactable = false;
                this.SelfCanvas.blocksRaycasts = false;
            }
        }

        private IEnumerator ShowTextWithDelay()
        {
            for (int i = 0; i < displayText.Length; i++)
            {
                Dialog.text += displayText[i];
                yield return new WaitForSeconds(letterDelay);
            }

            this._dialogCoroutine = null;
        }
    }
}