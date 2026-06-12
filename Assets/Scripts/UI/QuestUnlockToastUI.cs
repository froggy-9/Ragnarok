using System.Collections;
using DeadLetterOffice.Core;
using TMPro;
using UnityEngine;

namespace DeadLetterOffice.UI
{
    public class QuestUnlockToastUI : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private RectTransform _banner;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private float _enterSeconds = 0.42f;
        [SerializeField] private float _holdSeconds = 2.1f;
        [SerializeField] private float _exitSeconds = 0.36f;

        private Coroutine _routine;

        private void Awake()
        {
            if (_root == null)
            {
                _root = gameObject;
            }

            if (_banner == null)
            {
                _banner = transform as RectTransform;
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            HideImmediate();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<QuestUnlockedEvent>(OnQuestUnlocked);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<QuestUnlockedEvent>(OnQuestUnlocked);
        }

        public void ShowQuestUnlocked(StoryQuestSO quest)
        {
            if (quest == null)
            {
                return;
            }

            string prefix = quest.Type == StoryQuestType.Main ? "임무 개방" : "서브 임무 개방";
            Show($"{quest.Title} - {prefix}");
        }

        private void OnQuestUnlocked(QuestUnlockedEvent evt)
        {
            ShowQuestUnlocked(evt.Quest);
        }

        private void Show(string message)
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(ShowRoutine(message));
        }

        private IEnumerator ShowRoutine(string message)
        {
            if (_messageText != null)
            {
                _messageText.text = message;
            }

            if (_root != null)
            {
                _root.SetActive(true);
            }

            Vector2 hiddenPosition = new(0f, 118f);
            Vector2 visiblePosition = new(0f, -34f);
            SetVisual(hiddenPosition, 0f, 0.82f);

            yield return Animate(hiddenPosition, visiblePosition, 0f, 1f, 0.82f, 1f, _enterSeconds);
            yield return new WaitForSecondsRealtime(_holdSeconds);
            yield return Animate(visiblePosition, hiddenPosition, 1f, 0f, 1f, 0.94f, _exitSeconds);

            HideImmediate();
            _routine = null;
        }

        private IEnumerator Animate(Vector2 fromPosition, Vector2 toPosition, float fromAlpha, float toAlpha, float fromScale, float toScale, float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, seconds));
                float eased = EaseOutCubic(t);
                SetVisual(Vector2.LerpUnclamped(fromPosition, toPosition, eased), Mathf.Lerp(fromAlpha, toAlpha, eased), Mathf.Lerp(fromScale, toScale, eased));
                yield return null;
            }

            SetVisual(toPosition, toAlpha, toScale);
        }

        private void SetVisual(Vector2 anchoredPosition, float alpha, float scale)
        {
            if (_banner != null)
            {
                _banner.anchoredPosition = anchoredPosition;
                _banner.localScale = new Vector3(scale, scale, 1f);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = alpha;
            }
        }

        private void HideImmediate()
        {
            SetVisual(new Vector2(0f, 118f), 0f, 0.94f);
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}
