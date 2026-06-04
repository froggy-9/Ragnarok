using System.Collections;
using UnityEngine;

namespace DeadLetterOffice.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanelAnimator : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.18f;
        [SerializeField] private Vector2 _showOffset = new Vector2(0f, -18f);

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Vector2 _restPosition;
        private Coroutine _routine;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            _restPosition = _rectTransform.anchoredPosition;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Play(0f, 1f, _restPosition + _showOffset, _restPosition, true);
        }

        public void Hide()
        {
            Play(_canvasGroup.alpha, 0f, _rectTransform.anchoredPosition, _restPosition + _showOffset, false);
        }

        private void Play(float fromAlpha, float toAlpha, Vector2 fromPosition, Vector2 toPosition, bool staysActive)
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(Animate(fromAlpha, toAlpha, fromPosition, toPosition, staysActive));
        }

        private IEnumerator Animate(float fromAlpha, float toAlpha, Vector2 fromPosition, Vector2 toPosition, bool staysActive)
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
                yield return null;
            }

            _canvasGroup.alpha = toAlpha;
            _rectTransform.anchoredPosition = toPosition;
            _canvasGroup.blocksRaycasts = staysActive;
            _canvasGroup.interactable = staysActive;

            if (!staysActive)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
