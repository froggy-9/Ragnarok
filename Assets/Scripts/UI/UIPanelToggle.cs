using UnityEngine;

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
            }
        }

        public void Toggle()
        {
            if (_panel != null)
            {
                _panel.SetActive(!_panel.activeSelf);
            }
        }
    }
}
