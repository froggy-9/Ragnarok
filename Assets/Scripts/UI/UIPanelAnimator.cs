using System.Collections;
using UnityEngine;

namespace DeadLetterOffice.UI
{
    public enum UIPanelMotionPreset
    {
        GenericPanel,
        MapPanel
    }

    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanelAnimator : MonoBehaviour
    {
        [SerializeField] private UIPanelMotionPreset _motionPreset = UIPanelMotionPreset.GenericPanel;
        [SerializeField] private float _duration = 0.2f;
        [SerializeField] private Vector2 _showOffset = new Vector2(0f, -18f);
        [SerializeField] private Vector3 _hiddenScale = new Vector3(0.98f, 0.98f, 1f);
        [SerializeField] private UIAudioKind _openSound = UIAudioKind.GenericOpen;
        [SerializeField] private UIAudioKind _closeSound = UIAudioKind.GenericClose;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Vector2 _restPosition;
        private Vector3 _restScale;
        private Coroutine _routine;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            _restPosition = _rectTransform.anchoredPosition;
            _restScale = _rectTransform.localScale;
            ApplyPresetDefaults();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            UIAudioPlayer.Play(_openSound);
            Play(0f, 1f, _restPosition + _showOffset, _restPosition, _hiddenScale, _restScale, true);
        }

        public void Hide()
        {
            UIAudioPlayer.Play(_closeSound);
            Play(_canvasGroup.alpha, 0f, _rectTransform.anchoredPosition, _restPosition + _showOffset, _rectTransform.localScale, _hiddenScale, false);
        }

        private void Play(float fromAlpha, float toAlpha, Vector2 fromPosition, Vector2 toPosition, Vector3 fromScale, Vector3 toScale, bool staysActive)
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(Animate(fromAlpha, toAlpha, fromPosition, toPosition, fromScale, toScale, staysActive));
        }

        private IEnumerator Animate(float fromAlpha, float toAlpha, Vector2 fromPosition, Vector2 toPosition, Vector3 fromScale, Vector3 toScale, bool staysActive)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;

            float elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                _canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);
                _rectTransform.anchoredPosition = Vector2.LerpUnclamped(fromPosition, toPosition, eased);
                _rectTransform.localScale = Vector3.LerpUnclamped(fromScale, toScale, eased);
                yield return null;
            }

            _canvasGroup.alpha = toAlpha;
            _rectTransform.anchoredPosition = toPosition;
            _rectTransform.localScale = toScale;
            _canvasGroup.blocksRaycasts = staysActive;
            _canvasGroup.interactable = staysActive;

            if (!staysActive)
            {
                gameObject.SetActive(false);
            }
        }

        private void ApplyPresetDefaults()
        {
            if (_motionPreset == UIPanelMotionPreset.MapPanel)
            {
                _duration = Mathf.Max(_duration, 0.32f);
                _showOffset = Vector2.zero;
                _hiddenScale = new Vector3(0.86f, 0.86f, 1f);
                _openSound = UIAudioKind.MapOpen;
                _closeSound = UIAudioKind.MapClose;
                return;
            }

            _duration = Mathf.Max(_duration, 0.2f);
            _showOffset = new Vector2(0f, -24f);
            _hiddenScale = new Vector3(0.98f, 0.98f, 1f);
            _openSound = UIAudioKind.GenericOpen;
            _closeSound = UIAudioKind.GenericClose;
        }
    }
}
