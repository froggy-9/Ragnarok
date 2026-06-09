using UnityEngine;
using DeadLetterOffice.Core;
using UnityEngine.InputSystem;

namespace DeadLetterOffice.UI
{
    public class ExplorationCursorController : MonoBehaviour
    {
        [SerializeField] private GameObject _crosshairRoot;
        [SerializeField] private CursorLockMode _explorationLockMode = CursorLockMode.Locked;
        [SerializeField] private bool _hideCursorInExploration = true;
        [SerializeField] private Key _cursorUnlockKey = Key.LeftAlt;

        private GameMode _currentMode = GameMode.Exploration;

        private void OnEnable()
        {
            GameEventBus.Subscribe<GameModeChangedEvent>(OnGameModeChanged);

            if (ServiceLocator.TryGet(out GameModeManager modeManager))
            {
                _currentMode = modeManager.CurrentMode;
            }

            ApplyMode(_currentMode);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<GameModeChangedEvent>(OnGameModeChanged);
            ShowCursor();
        }

        private void OnGameModeChanged(GameModeChangedEvent evt)
        {
            _currentMode = evt.Mode;
            ApplyMode(evt.Mode);
        }

        private void LateUpdate()
        {
            if (ServiceLocator.TryGet(out GameModeManager modeManager))
            {
                _currentMode = modeManager.CurrentMode;
            }

            ApplyMode(_currentMode);
        }

        private void ApplyMode(GameMode mode)
        {
            bool isExploration = mode == GameMode.Exploration;
            bool isCursorUnlocked = isExploration && IsUnlockKeyPressed();
            bool showCrosshair = isExploration && !isCursorUnlocked;

            if (_crosshairRoot != null && _crosshairRoot.activeSelf != showCrosshair)
            {
                _crosshairRoot.SetActive(showCrosshair);
            }

            if (isExploration)
            {
                if (isCursorUnlocked)
                {
                    ShowCursor();
                    return;
                }

                Cursor.lockState = _explorationLockMode;
                Cursor.visible = !_hideCursorInExploration;
                return;
            }

            ShowCursor();
        }

        private bool IsUnlockKeyPressed()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard[_cursorUnlockKey].isPressed;
        }

        private static void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
