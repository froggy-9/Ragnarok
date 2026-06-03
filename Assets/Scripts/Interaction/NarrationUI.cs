using DeadLetterOffice.Core;
using TMPro;
using UnityEngine;

namespace DeadLetterOffice.Interaction
{
    public class NarrationUI : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _text;

        private void Awake()
        {
            Hide();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<NarrationRequestedEvent>(OnNarrationRequested);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<NarrationRequestedEvent>(OnNarrationRequested);
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private void OnNarrationRequested(NarrationRequestedEvent evt)
        {
            if (_text != null)
            {
                _text.text = evt.Text;
            }

            if (_root != null)
            {
                _root.SetActive(true);
            }
        }
    }
}
