using UnityEngine;
using UnityEngine.EventSystems;

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
            if (_panel == null)
            {
                return;
            }

            _panel.transform.SetAsLastSibling();
            _panel.SetActive(true);
        }
    }
}
