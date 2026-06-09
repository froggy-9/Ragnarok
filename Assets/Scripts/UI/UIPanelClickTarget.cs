using UnityEngine;
using UnityEngine.EventSystems;
using DeadLetterOffice.Core;

namespace DeadLetterOffice.UI
{
    public class UIPanelClickTarget : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameObject _panel;

        public void SetPanel(GameObject panel)
        {
            _panel = panel;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_panel == null || !CanUsePanels())
            {
                return;
            }

            _panel.transform.SetAsLastSibling();
            if (_panel.TryGetComponent(out UIPanelAnimator animator))
            {
                animator.Show();
            }
            else
            {
                _panel.SetActive(true);
            }

            if (ServiceLocator.TryGet(out GameModeManager modeManager))
            {
                modeManager.SetUiMode();
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
