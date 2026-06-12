using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeadLetterOffice.Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadLetterOffice.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _characterImage;
        [SerializeField] private GameObject _illustrationPanel;
        [SerializeField] private Image _illustrationImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _dialogueText;
        [SerializeField] private Transform _choiceContainer;
        [SerializeField] private Button _choiceButtonPrefab;
        [SerializeField] private GameObject _advanceIndicator;
        [SerializeField] private bool _useDefaultTypewriter = true;
        [SerializeField] private float _charactersPerSecond = 26f;
        [SerializeField] private float _fadeDuration = 0.32f;
        [SerializeField] private float _lineStartDelay = 0.35f;
        [SerializeField] private float _choiceFadeDuration = 0.22f;
        [SerializeField] private float _choiceStaggerDelay = 0.055f;

        private readonly List<Button> _spawnedChoiceButtons = new();
        private Coroutine _currentEffectCoroutine;
        private Coroutine _visibilityCoroutine;
        private Coroutine _choiceRevealCoroutine;
        private CanvasGroup _rootCanvasGroup;
        private bool _isPlaying;
        private bool _skipRequested;
        private string _currentFullText;

        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            EnsureCanvasGroup();
            HideImmediate();
        }

        public IEnumerator ShowLine(DialogueLineSO line)
        {
            if (line == null)
            {
                yield break;
            }

            Show();
            ClearChoices();
            SetAdvanceIndicatorVisible(false);

            if (_nameText != null)
            {
                _nameText.gameObject.SetActive(line.ShowsSpeakerName);
                _nameText.text = line.ShowsSpeakerName ? line.Speaker : string.Empty;
            }

            if (_characterImage != null)
            {
                _characterImage.sprite = line.CharacterSprite;
                _characterImage.enabled = line.CharacterSprite != null;
            }

            if (_illustrationPanel != null)
            {
                _illustrationPanel.SetActive(line.ShowIllustrationAsPanel && line.IllustrationSprite != null);
            }

            if (_illustrationImage != null)
            {
                _illustrationImage.sprite = line.IllustrationSprite;
                _illustrationImage.enabled = line.IllustrationSprite != null;
            }

            _currentFullText = line.Text;
            _isPlaying = true;
            _skipRequested = false;

            if (_dialogueText != null)
            {
                _dialogueText.text = string.Empty;
            }

            if (_lineStartDelay > 0f)
            {
                float delayElapsed = 0f;
                while (delayElapsed < _lineStartDelay && !_skipRequested)
                {
                    delayElapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            List<TextEffect> effects = line.GetActiveEffects().ToList();
            if (_dialogueText != null && !_skipRequested)
            {
                if (effects.Count == 0)
                {
                    if (_useDefaultTypewriter)
                    {
                        yield return PlayDefaultTypewriter(line.Text);
                    }
                    else
                    {
                        _dialogueText.text = line.Text;
                    }
                }
                else
                {
                    yield return PlaySkippableEffect(effects[0], line.Text);
                }
            }

            if (_dialogueText != null)
            {
                _dialogueText.text = _currentFullText;
            }

            _isPlaying = false;
            _skipRequested = false;
            _currentEffectCoroutine = null;
            SetAdvanceIndicatorVisible(true);
        }

        public void ShowChoices(IEnumerable<DialogueChoiceSO> choices, System.Action<DialogueChoiceSO> onSelected, DeadLetterOffice.State.GameStateSO gameState = null)
        {
            Show();
            ClearChoices();
            SetAdvanceIndicatorVisible(false);

            if (_choiceContainer == null || _choiceButtonPrefab == null || choices == null)
            {
                return;
            }

            foreach (DialogueChoiceSO choice in choices)
            {
                if (choice == null || !choice.CanSelect(gameState))
                {
                    continue;
                }

                Button button = Instantiate(_choiceButtonPrefab, _choiceContainer);
                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = choice.GetDisplayText();
                }

                DialogueChoiceSO capturedChoice = choice;
                button.onClick.AddListener(() => onSelected?.Invoke(capturedChoice));

                CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
                }

                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                button.gameObject.SetActive(true);
                _spawnedChoiceButtons.Add(button);
            }

            if (_choiceRevealCoroutine != null)
            {
                StopCoroutine(_choiceRevealCoroutine);
            }

            _choiceRevealCoroutine = StartCoroutine(RevealChoices());
        }

        public void SkipCurrentLine()
        {
            if (!_isPlaying)
            {
                return;
            }

            _skipRequested = true;

            if (_dialogueText != null)
            {
                _dialogueText.text = _currentFullText;
            }

            SetAdvanceIndicatorVisible(true);
        }

        public void Show()
        {
            if (_root == null)
            {
                return;
            }

            EnsureCanvasGroup();
            bool wasInactive = !_root.activeSelf;
            _root.SetActive(true);

            if (wasInactive)
            {
                PlayVisibility(0f, 1f, true);
            }
            else if (_rootCanvasGroup != null)
            {
                _rootCanvasGroup.alpha = 1f;
                _rootCanvasGroup.blocksRaycasts = true;
                _rootCanvasGroup.interactable = true;
            }
        }

        public void Hide()
        {
            ClearChoices();

            if (_illustrationPanel != null)
            {
                _illustrationPanel.SetActive(false);
            }

            SetAdvanceIndicatorVisible(false);

            if (_root == null)
            {
                return;
            }

            EnsureCanvasGroup();
            PlayVisibility(_rootCanvasGroup != null ? _rootCanvasGroup.alpha : 1f, 0f, false);
        }

        private IEnumerator PlayDefaultTypewriter(string text)
        {
            _dialogueText.text = string.Empty;

            if (string.IsNullOrEmpty(text))
            {
                yield break;
            }

            float delay = 1f / Mathf.Max(1f, _charactersPerSecond);
            float elapsed = 0f;
            int visibleCharacters = 0;

            while (visibleCharacters < text.Length)
            {
                if (_skipRequested)
                {
                    break;
                }

                elapsed += Time.unscaledDeltaTime;
                int targetCharacters = Mathf.Min(text.Length, Mathf.FloorToInt(elapsed / delay) + 1);

                if (targetCharacters != visibleCharacters)
                {
                    visibleCharacters = targetCharacters;
                    _dialogueText.text = text.Substring(0, visibleCharacters);
                }

                yield return null;
            }
        }

        private IEnumerator PlaySkippableEffect(TextEffect effect, string text)
        {
            if (effect == null)
            {
                yield break;
            }

            _currentEffectCoroutine = StartCoroutine(EffectWrapper(effect, text));
            while (_currentEffectCoroutine != null && !_skipRequested)
            {
                yield return null;
            }

            if (_skipRequested && _currentEffectCoroutine != null)
            {
                StopCoroutine(_currentEffectCoroutine);
                _currentEffectCoroutine = null;
            }
        }

        private IEnumerator EffectWrapper(TextEffect effect, string text)
        {
            yield return effect.Play(_dialogueText, text);
            _currentEffectCoroutine = null;
        }

        private void PlayVisibility(float fromAlpha, float toAlpha, bool staysActive)
        {
            if (_visibilityCoroutine != null)
            {
                StopCoroutine(_visibilityCoroutine);
            }

            _visibilityCoroutine = StartCoroutine(VisibilityRoutine(fromAlpha, toAlpha, staysActive));
        }

        private IEnumerator VisibilityRoutine(float fromAlpha, float toAlpha, bool staysActive)
        {
            if (_rootCanvasGroup == null)
            {
                yield break;
            }

            _rootCanvasGroup.blocksRaycasts = true;
            _rootCanvasGroup.interactable = true;

            float duration = Mathf.Max(0.01f, _fadeDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                _rootCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);
                yield return null;
            }

            _rootCanvasGroup.alpha = toAlpha;
            _rootCanvasGroup.blocksRaycasts = staysActive;
            _rootCanvasGroup.interactable = staysActive;

            if (!staysActive && _root != null)
            {
                _root.SetActive(false);
            }
        }

        private void EnsureCanvasGroup()
        {
            if (_root == null || _rootCanvasGroup != null)
            {
                return;
            }

            _rootCanvasGroup = _root.GetComponent<CanvasGroup>();
            if (_rootCanvasGroup == null)
            {
                _rootCanvasGroup = _root.AddComponent<CanvasGroup>();
            }
        }

        private void HideImmediate()
        {
            ClearChoices();

            if (_rootCanvasGroup != null)
            {
                _rootCanvasGroup.alpha = 0f;
                _rootCanvasGroup.blocksRaycasts = false;
                _rootCanvasGroup.interactable = false;
            }

            if (_root != null)
            {
                _root.SetActive(false);
            }

            if (_illustrationPanel != null)
            {
                _illustrationPanel.SetActive(false);
            }

            SetAdvanceIndicatorVisible(false);
        }

        private void ClearChoices()
        {
            if (_choiceRevealCoroutine != null)
            {
                StopCoroutine(_choiceRevealCoroutine);
                _choiceRevealCoroutine = null;
            }

            foreach (Button button in _spawnedChoiceButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            _spawnedChoiceButtons.Clear();
        }

        private IEnumerator RevealChoices()
        {
            for (int i = 0; i < _spawnedChoiceButtons.Count; i++)
            {
                Button button = _spawnedChoiceButtons[i];
                if (button == null)
                {
                    continue;
                }

                if (i > 0)
                {
                    yield return new WaitForSecondsRealtime(_choiceStaggerDelay);
                }

                yield return AnimateChoice(button);
            }

            _choiceRevealCoroutine = null;
        }

        private IEnumerator AnimateChoice(Button button)
        {
            CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            if (canvasGroup == null || rectTransform == null)
            {
                yield break;
            }

            Vector3 restScale = rectTransform.localScale;
            Vector3 startScale = new(restScale.x * 0.985f, restScale.y * 0.985f, restScale.z);

            rectTransform.localScale = startScale;
            float duration = Mathf.Max(0.01f, _choiceFadeDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, eased);
                rectTransform.localScale = Vector3.LerpUnclamped(startScale, restScale, eased);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            rectTransform.localScale = restScale;
        }

        private void SetAdvanceIndicatorVisible(bool visible)
        {
            if (_advanceIndicator != null && _advanceIndicator.activeSelf != visible)
            {
                _advanceIndicator.SetActive(visible);
            }
        }
    }
}
