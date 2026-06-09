using UnityEngine;
using DeadLetterOffice.Core;

namespace DeadLetterOffice.UI
{
    public class UIPanelToggle : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private bool _hideOnAwake = true;

        private void Awake()
        {
            if (_hideOnAwake)
            {
                Hide();
            }
        }

        public void Show()
        {
            if (!CanUsePanels())
            {
                return;
            }

            if (_panel != null)
            {
                _panel.transform.SetAsLastSibling();
                if (_panel.TryGetComponent(out UIPanelAnimator animator))
                {
                    animator.Show();
                }
                else
                {
                    _panel.SetActive(true);
                }

                SetUiMode();
            }
        }

        public void Hide()
        {
            if (_panel != null)
            {
                if (_panel.activeSelf && _panel.TryGetComponent(out UIPanelAnimator animator))
                {
                    animator.Hide();
                }
                else
                {
                    _panel.SetActive(false);
                }

                SetExplorationModeIfUiMode();
            }
        }

        public void Toggle()
        {
            if (!CanUsePanels())
            {
                return;
            }

            if (_panel != null)
            {
                if (_panel.activeSelf)
                {
                    Hide();
                }
                else
                {
                    Show();
                }
            }
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
