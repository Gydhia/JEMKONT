using DG.Tweening;
using DownBelow.Entity;
using DownBelow.Managers;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIStaticGather : MonoBehaviour
    {
        public GameObject ResourceGauge;
        public TextMeshProUGUI Resources;

        public GameObject ContentQTE;

        public TextMeshProUGUI Result;
        public Slider CursorQTE;
        public RectTransform ZoneQTE;

        private Sequence _cursor;
        private Sequence _anchorMin;
        private Sequence _anchorMax;

        private Sequence _result;

        private GatheringAction _gatherAction;

        private int _maxInteract = 0;
        private int _failedInteract = 0;
        private int _currInteract = 0;

        private void Start()
        {
            this.ContentQTE.SetActive(false);
            this.ResourceGauge.SetActive(false);
        }

        public void StartInteract(GatheringAction action, int numberOfInteracts = 3)
        {
            this._gatherAction = action;

            this._maxInteract = numberOfInteracts;
            this._currInteract = 0;

            this.ContentQTE.gameObject.SetActive(true);

            this.Result.alpha = 0f;

            this.LoneInteract();
        }

        public void LoneInteract()
        {
            PlayerInputs.player_interact.performed += _playerInteract;
            var preset = SettingsManager.Instance.ResourcesPreset;

            this._cursor = DOTween.Sequence();
            this._anchorMin = DOTween.Sequence();
            this._anchorMax = DOTween.Sequence();
            this._result = DOTween.Sequence();

            this.ContentQTE.SetActive(true);

            float newCenter = Random.Range(preset.MinX, preset.MaxX);

            this.CursorQTE.value = 0f;
            this.ZoneQTE.anchorMin = new Vector2(newCenter - preset.InteractWidth / 2f, this.ZoneQTE.anchorMin.y);
            this.ZoneQTE.anchorMax = new Vector2(newCenter + preset.InteractWidth / 2f, this.ZoneQTE.anchorMax.y);
            
            float averageCenter = (this.ZoneQTE.anchorMin.x + this.ZoneQTE.anchorMax.x) / 2f;

            this._anchorMin.Append(this.ZoneQTE.DOAnchorMin(new Vector2(averageCenter, this.ZoneQTE.anchorMin.y), preset.DecreaseSpeed).SetEase(Ease.Linear));
            this._anchorMax.Append(this.ZoneQTE.DOAnchorMax(new Vector2(averageCenter, this.ZoneQTE.anchorMax.y), preset.DecreaseSpeed).SetEase(Ease.Linear));
            this._cursor.Append(this.CursorQTE.DOValue(1f, preset.CursorCrossTime).SetEase(Ease.Linear)).OnComplete(() =>
            {
                this.PlayerInteract(false);
            });
        }

        private void _playerInteract(InputAction.CallbackContext ctx) => this.PlayerInteract(true);
        public void PlayerInteract(bool fromPlayer)
        {
            this._currInteract++;

            var resTool = GameManager.RealSelfPlayer.ActiveTools.First(t => t.Class == this._gatherAction.CurrentRessource.LocalPreset.GatherableBy);
            GameManager.RealSelfPlayer.Animator.SetTrigger(resTool.GatherAnim);

            PlayerInputs.player_interact.performed -= _playerInteract;

            this._cursor.Kill();
            this._anchorMin.Kill();
            this._anchorMax.Kill(); 
            this._result.Kill();

            bool succeeded = this.CursorQTE.value > this.ZoneQTE.anchorMin.x && this.CursorQTE.value < this.ZoneQTE.anchorMax.x;

            this.Result.text = succeeded ? "GREAT !" :  "FAILED..";
            this.Result.color = succeeded ? Color.green : Color.red;

            var basePos = this.Result.transform.position;
            this.Result.alpha = 1f;
            this._result
                .Join(this.Result.transform.DOMoveY(this.Result.transform.position.y + 1f, 0.5f))
                .Join(this.Result.DOFade(0f, 0.5f))
                .OnComplete(() => { this.Result.transform.position = basePos; });

            if (this._currInteract == this._maxInteract)
            {
                this._onEndQTE();
            }
            else
            {
                this.LoneInteract();
            }
        }

        private void _onEndQTE()
        {
            StartCoroutine(_hideOnEnd());
            this._gatherAction.OnGatherEnded();
            
        }

        private IEnumerator _hideOnEnd()
        {
            yield return new WaitForSeconds(0.5f);

            this.ContentQTE.gameObject.SetActive(false);

        }
    }
}