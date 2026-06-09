using UnityEngine;
using UnityEngine.InputSystem;
using DeadLetterOffice.Core;

namespace DeadLetterOffice.UI
{
    public class UIPanelHotkeyManager : MonoBehaviour
    {
        [SerializeField] private GameObject _mapPanel;
        [SerializeField] private GameObject _questPanel;
        [SerializeField] private GameObject _helpPanel;
        [SerializeField] private GameObject _archivePanel;
        [SerializeField] private GameObject _boardPanel;
        [SerializeField] private GameObject _settingsPanel;

        [SerializeField] private Key _mapKey = Key.M;
        [SerializeField] private Key _questKey = Key.Q;
        [SerializeField] private Key _helpKey = Key.H;
        [SerializeField] private Key _archiveKey = Key.E;
        [SerializeField] private Key _boardKey = Key.R;
        [SerializeField] private Key _settingsKey = Key.Escape;
        [SerializeField] private bool _boardUnlocked = true;

        private GameObject[] _panels;

        private void Awake()
        {
            _panels = new[] { _mapPanel, _questPanel, _helpPanel, _archivePanel, _boardPanel, _settingsPanel };
        }

        private void Update()
        {
            if (!CanUsePanels())
            {
                HideAll();
                return;
            }

            if (AnyPanelOpen())
            {
                if (WasPressedThisFrame(_settingsKey))
                {
                    CloseTopPanel();
                    return;
                }

                if (TryCloseActivePanelOnKey(_mapKey, _mapPanel)
                    || TryCloseActivePanelOnKey(_questKey, _questPanel)
                    || TryCloseActivePanelOnKey(_helpKey, _helpPanel)
                    || TryCloseActivePanelOnKey(_archiveKey, _archivePanel)
                    || TryCloseActivePanelOnKey(_boardKey, _boardPanel))
                {
                    return;
                }

                return;
            }

            SetExplorationModeIfUiMode();

            OpenOnKey(_settingsKey, _settingsPanel);
            OpenOnKey(_mapKey, _mapPanel);
            OpenOnKey(_questKey, _questPanel);
            OpenOnKey(_helpKey, _helpPanel);
            OpenOnKey(_archiveKey, _archivePanel);
            if (_boardUnlocked)
            {
                OpenOnKey(_boardKey, _boardPanel);
            }
        }

        public void UnlockBoard()
        {
            _boardUnlocked = true;
        }

        public void LockBoard()
        {
            _boardUnlocked = false;
            Hide(_boardPanel);
        }

        public void Show(GameObject panel)
        {
            if (panel == null || !CanUsePanels())
            {
                return;
            }

            panel.transform.SetAsLastSibling();
            if (panel.TryGetComponent(out UIPanelAnimator animator))
            {
                animator.Show();
            }
            else
            {
                panel.SetActive(true);
            }

            SetUiMode();
        }

        public void Hide(GameObject panel)
        {
            if (panel != null)
            {
                if (panel.activeSelf && panel.TryGetComponent(out UIPanelAnimator animator))
                {
                    animator.Hide();
                }
                else
                {
                    panel.SetActive(false);
                }
            }

            if (!AnyPanelOpenExcept(panel))
            {
                SetExplorationModeIfUiMode();
            }
        }

        public void Toggle(GameObject panel)
        {
            if (panel == null || !CanUsePanels())
            {
                return;
            }

            if (!AnyPanelOpen())
            {
                Show(panel);
            }
        }

        public void HideAll()
        {
            if (_panels == null)
            {
                return;
            }

            foreach (GameObject panel in _panels)
            {
                Hide(panel);
            }
        }

        private void OpenOnKey(Key key, GameObject panel)
        {
            if (WasPressedThisFrame(key))
            {
                Show(panel);
            }
        }

        private bool TryCloseActivePanelOnKey(Key key, GameObject panel)
        {
            if (panel == null || !panel.activeSelf || !WasPressedThisFrame(key))
            {
                return false;
            }

            Hide(panel);
            return true;
        }

        private static bool WasPressedThisFrame(Key key)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            return keyboard[key].wasPressedThisFrame;
        }

        private bool AnyPanelOpen()
        {
            if (_panels == null)
            {
                return false;
            }

            foreach (GameObject panel in _panels)
            {
                if (panel != null && panel.activeSelf)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CloseTopPanel()
        {
            if (_panels == null)
            {
                return false;
            }

            for (int i = _panels.Length - 1; i >= 0; i--)
            {
                GameObject panel = _panels[i];
                if (panel != null && panel.activeSelf)
                {
                    Hide(panel);
                    return true;
                }
            }

            return false;
        }

        private bool AnyPanelOpenExcept(GameObject ignoredPanel)
        {
            if (_panels == null)
            {
                return false;
            }

            foreach (GameObject panel in _panels)
            {
                if (panel != null && panel != ignoredPanel && panel.activeSelf)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetUiMode()
        {
            if (ServiceLocator.TryGet(out GameModeManager modeManager))
            {
                modeManager.SetUiMode();
            }
        }

        private static void SetExplorationModeIfUiMode()
        {
            if (ServiceLocator.TryGet(out GameModeManager modeManager) && modeManager.CurrentMode == GameMode.UI)
            {
                modeManager.SetExplorationMode();
            }
        }

        private static bool CanUsePanels()
        {
            if (!ServiceLocator.TryGet(out GameModeManager modeManager))
            {
                return true;
            }

            return modeManager.CurrentMode == GameMode.Exploration || modeManager.CurrentMode == GameMode.UI;
        }
    }
}
