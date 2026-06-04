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
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _dialogueText;
        [SerializeField] private Transform _choiceContainer;
        [SerializeField] private Button _choiceButtonPrefab;

        private readonly List<Button> _spawnedChoiceButtons = new();
        private Coroutine _currentEffectCoroutine;
        private bool _isPlaying;
        private string _currentFullText;

        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            Hide();
        }

        public IEnumerator ShowLine(DialogueLineSO line)
        {
            if (line == null)
            {
                yield break;
            }

            Show();
            ClearChoices();

            if (_nameText != null)
            {
                _nameText.text = line.Speaker;
            }

            if (_characterImage != null)
            {
                _characterImage.sprite = line.CharacterSprite;
                _characterImage.enabled = line.CharacterSprite != null;
            }

            _currentFullText = line.Text;
            _isPlaying = true;

            List<TextEffect> effects = line.GetActiveEffects().ToList();
            if (_dialogueText != null)
            {
                if (effects.Count == 0)
                {
                    _dialogueText.text = line.Text;
                }
                else
                {
                    _currentEffectCoroutine = StartCoroutine(effects[0].Play(_dialogueText, line.Text));
                    yield return _currentEffectCoroutine;
                }
            }

            _isPlaying = false;
        }

        public void ShowChoices(IEnumerable<DialogueChoiceSO> choices, System.Action<DialogueChoiceSO> onSelected)
        {
            Show();
            ClearChoices();

            if (_choiceContainer == null || _choiceButtonPrefab == null || choices == null)
            {
                return;
            }

            foreach (DialogueChoiceSO choice in choices)
            {
                if (choice == null || !choice.CanSelect())
                {
                    continue;
                }

                Button button = Instantiate(_choiceButtonPrefab, _choiceContainer);
                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = choice.ChoiceText;
                }

                DialogueChoiceSO capturedChoice = choice;
                button.onClick.AddListener(() => onSelected?.Invoke(capturedChoice));
                button.gameObject.SetActive(true);
                _spawnedChoiceButtons.Add(button);
            }
        }

        public void SkipCurrentLine()
        {
            if (!_isPlaying)
            {
                return;
            }

            if (_currentEffectCoroutine != null)
            {
                StopCoroutine(_currentEffectCoroutine);
                _currentEffectCoroutine = null;
            }

            if (_dialogueText != null)
            {
                _dialogueText.text = _currentFullText;
            }

            _isPlaying = false;
        }

        public void Show()
        {
            if (_root != null)
            {
                _root.SetActive(true);
            }
        }

        public void Hide()
        {
            ClearChoices();

            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private void ClearChoices()
        {
            foreach (Button button in _spawnedChoiceButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            _spawnedChoiceButtons.Clear();
        }
    }
}
