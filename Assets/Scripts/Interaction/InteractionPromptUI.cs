using DeadLetterOffice.Core;
using TMPro;
using UnityEngine;

namespace DeadLetterOffice.Interaction
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _promptText;
        [SerializeField] private string _prefix = "F";
        [SerializeField] private int _displayPriority;

        private CanvasGroup _canvasGroup;
        private bool _isInExploration = true;

        private void Awake()
        {
            if (_root == null)
            {
                _root = gameObject;
            }

            _canvasGroup = _root.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = _root.AddComponent<CanvasGroup>();
            }

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            SetPrompt(false, string.Empty);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<InteractionPromptChangedEvent>(OnPromptChanged);
            GameEventBus.Subscribe<GameModeChangedEvent>(OnGameModeChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<InteractionPromptChangedEvent>(OnPromptChanged);
            GameEventBus.Unsubscribe<GameModeChangedEvent>(OnGameModeChanged);
        }

        private void OnPromptChanged(InteractionPromptChangedEvent evt)
        {
            SetPrompt(evt.Visible, evt.Text);
        }

        private void OnGameModeChanged(GameModeChangedEvent evt)
        {
            _isInExploration = evt.Mode == GameMode.Exploration;
            if (!_isInExploration)
            {
                SetPrompt(false, string.Empty);
            }
        }

        private void SetPrompt(bool visible, string text)
        {
            bool shouldShow = visible && _isInExploration && !string.IsNullOrWhiteSpace(text) && IsPrimaryPrompt();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = shouldShow ? 1f : 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            else if (_root != null && _root != gameObject)
            {
                _root.SetActive(shouldShow);
            }

            if (_promptText != null)
            {
                _promptText.text = string.IsNullOrWhiteSpace(_prefix) ? text : $"{_prefix}  {text}";
            }
        }

        private bool IsPrimaryPrompt()
        {
            InteractionPromptUI[] prompts = FindObjectsByType<InteractionPromptUI>(FindObjectsSortMode.None);
            InteractionPromptUI best = null;
            int bestScore = int.MinValue;

            foreach (InteractionPromptUI prompt in prompts)
            {
                if (prompt == null || !prompt.isActiveAndEnabled)
                {
                    continue;
                }

                int score = prompt.GetPriorityScore();
                if (best == null || score > bestScore || (score == bestScore && prompt.GetInstanceID() < best.GetInstanceID()))
                {
                    best = prompt;
                    bestScore = score;
                }
            }

            return ReferenceEquals(best, this);
        }

        private int GetPriorityScore()
        {
            int score = _displayPriority;
            Transform current = transform;
            while (current != null)
            {
                if (current.name == "DLO_MainHUD")
                {
                    score += 100;
                }
                else if (current.name == "DLO_InteractionCanvas")
                {
                    score -= 10;
                }

                current = current.parent;
            }

            return score;
        }
    }
}
