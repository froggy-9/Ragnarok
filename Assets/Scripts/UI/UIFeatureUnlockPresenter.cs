using System.Collections;
using DeadLetterOffice.Core;
using TMPro;
using UnityEngine;

namespace DeadLetterOffice.UI
{
    public class UIFeatureUnlockPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _unlockOverlay;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private UnlockableHudButton _inferenceBoardButton;
        [SerializeField] private UIPanelHotkeyManager _hotkeyManager;
        [SerializeField] private float _displaySeconds = 2.2f;

        private Coroutine _unlockRoutine;

        private void Awake()
        {
            if (_unlockOverlay != null)
            {
                _unlockOverlay.SetActive(false);
            }
        }

        public void UnlockInferenceBoard()
        {
            ShowUnlock(
                "새 기능 해금",
                "추리보드를 사용할 수 있습니다.",
                () =>
                {
                    if (_inferenceBoardButton != null)
                    {
                        _inferenceBoardButton.Unlock();
                    }

                    if (_hotkeyManager != null)
                    {
                        _hotkeyManager.UnlockBoard();
                    }
                });
        }

        public void LockInferenceBoard()
        {
            if (_inferenceBoardButton != null)
            {
                _inferenceBoardButton.Lock();
            }

            if (_hotkeyManager != null)
            {
                _hotkeyManager.LockBoard();
            }
        }

        private void ShowUnlock(string title, string description, System.Action applyUnlock)
        {
            if (_unlockRoutine != null)
            {
                StopCoroutine(_unlockRoutine);
            }

            _unlockRoutine = StartCoroutine(ShowUnlockRoutine(title, description, applyUnlock));
        }

        private IEnumerator ShowUnlockRoutine(string title, string description, System.Action applyUnlock)
        {
            GameModeManager modeManager = ServiceLocator.TryGet(out GameModeManager foundModeManager)
                ? foundModeManager
                : null;
            GameMode previousMode = modeManager != null ? modeManager.CurrentMode : GameMode.Exploration;

            modeManager?.SetUiMode();

            if (_titleText != null)
            {
                _titleText.text = title;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = description;
            }

            if (_unlockOverlay != null)
            {
                _unlockOverlay.transform.SetAsLastSibling();
                _unlockOverlay.SetActive(true);
            }

            applyUnlock?.Invoke();
            yield return new WaitForSecondsRealtime(_displaySeconds);

            if (_unlockOverlay != null)
            {
                _unlockOverlay.SetActive(false);
            }

            if (modeManager != null)
            {
                modeManager.SetMode(previousMode);
            }

            _unlockRoutine = null;
        }
    }
}
