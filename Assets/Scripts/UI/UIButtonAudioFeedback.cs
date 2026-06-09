using UnityEngine;
using UnityEngine.UI;

namespace DeadLetterOffice.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonAudioFeedback : MonoBehaviour
    {
        [SerializeField] private UIAudioKind _clickSound = UIAudioKind.ButtonClick;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(PlayClick);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(PlayClick);
            }
        }

        private void PlayClick()
        {
            UIAudioPlayer.Play(_clickSound);
        }
    }
}
