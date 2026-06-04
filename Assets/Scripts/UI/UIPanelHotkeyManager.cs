using UnityEngine;
using UnityEngine.InputSystem;

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
        [SerializeField] private Key _questKey = Key.J;
        [SerializeField] private Key _helpKey = Key.H;
        [SerializeField] private Key _archiveKey = Key.I;
        [SerializeField] private Key _boardKey = Key.B;
        [SerializeField] private Key _settingsKey = Key.Escape;
        [SerializeField] private bool _boardUnlocked;

        private GameObject[] _panels;

        private void Awake()
        {
            _panels = new[] { _mapPanel, _questPanel, _helpPanel, _archivePanel, _boardPanel, _settingsPanel };
        }

        private void Update()
        {
            if (WasPressedThisFrame(_settingsKey))
            {
                if (!CloseTopPanel())
                {
                    Show(_settingsPanel);
                }

                return;
            }

            ToggleOnKey(_mapKey, _mapPanel);
            ToggleOnKey(_questKey, _questPanel);
            ToggleOnKey(_helpKey, _helpPanel);
            ToggleOnKey(_archiveKey, _archivePanel);
            if (_boardUnlocked)
            {
                ToggleOnKey(_boardKey, _boardPanel);
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
            if (panel == null)
            {
                return;
            }

            panel.transform.SetAsLastSibling();
            panel.SetActive(true);
        }

        public void Hide(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public void Toggle(GameObject panel)
        {
            if (panel == null)
            {
                return;
            }

            if (panel.activeSelf)
            {
                panel.SetActive(false);
            }
            else
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

        private void ToggleOnKey(Key key, GameObject panel)
        {
            if (WasPressedThisFrame(key))
            {
                Toggle(panel);
            }
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
                    panel.SetActive(false);
                    return true;
                }
            }

            return false;
        }
    }
}
