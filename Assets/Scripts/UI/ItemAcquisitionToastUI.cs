using System.Collections;
using DeadLetterOffice.Core;
using DeadLetterOffice.Letter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadLetterOffice.UI
{
    public class ItemAcquisitionToastUI : MonoBehaviour
    {
        [SerializeField] private Transform _toastRoot;
        [SerializeField] private GameObject _toastTemplate;
        [SerializeField] private float _visibleDuration = 1.15f;
        [SerializeField] private float _fadeDuration = 0.42f;
        [SerializeField] private Vector2 _exitOffset = new Vector2(0f, 34f);

        private void OnEnable()
        {
            GameEventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
            if (_toastTemplate != null)
            {
                _toastTemplate.SetActive(false);
            }
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<ItemCollectedEvent>(OnItemCollected);
        }

        private void OnItemCollected(ItemCollectedEvent evt)
        {
            if (evt.Item == null || _toastRoot == null || _toastTemplate == null)
            {
                return;
            }

            GameObject toast = Instantiate(_toastTemplate, _toastRoot);
            toast.SetActive(true);
            BindToast(toast, evt.Item, evt.Amount);
            StartCoroutine(PlayToast(toast));
        }

        private static void BindToast(GameObject toast, CollectibleSO item, int amount)
        {
            TMP_Text nameText = toast.transform.Find("NameText")?.GetComponent<TMP_Text>();
            TMP_Text amountText = toast.transform.Find("AmountText")?.GetComponent<TMP_Text>();
            Image iconImage = toast.transform.Find("Icon")?.GetComponent<Image>();

            if (nameText != null)
            {
                nameText.text = string.IsNullOrWhiteSpace(item.DisplayName) ? item.name : item.DisplayName;
            }

            if (amountText != null)
            {
                amountText.text = $"x{Mathf.Max(1, amount)}";
            }

            if (iconImage != null)
            {
                iconImage.sprite = item.Icon;
                iconImage.enabled = item.Icon != null;
            }
        }

        private IEnumerator PlayToast(GameObject toast)
        {
            CanvasGroup canvasGroup = toast.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = toast.AddComponent<CanvasGroup>();
            }

            RectTransform rect = toast.GetComponent<RectTransform>();
            Vector2 startPosition = rect.anchoredPosition;
            canvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(_visibleDuration);

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _fadeDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, eased);
                rect.anchoredPosition = Vector2.LerpUnclamped(startPosition, startPosition + _exitOffset, eased);
                yield return null;
            }

            Destroy(toast);
        }
    }
}
