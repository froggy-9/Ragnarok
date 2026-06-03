using System.Collections;
using DeadLetterOffice.Core;
using UnityEngine;

namespace DeadLetterOffice.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueUI _dialogueUI;

        private DialogueSO _currentDialogue;
        private int _currentLineIndex;
        private bool _isWaitingForAdvance;
        private bool _isRunning;

        public bool IsRunning => _isRunning;

        private void Awake()
        {
            if (_dialogueUI == null)
            {
                Debug.LogError("[DialogueManager] DialogueUI is required.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DialogueRequestedEvent>(OnDialogueRequested);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DialogueRequestedEvent>(OnDialogueRequested);
        }

        public void StartDialogue(DialogueSO dialogue)
        {
            if (dialogue == null || !dialogue.CanPlay())
            {
                Debug.LogWarning("[DialogueManager] Dialogue is missing or conditions are not met.");
                return;
            }

            if (_isRunning)
            {
                StopAllCoroutines();
            }

            _currentDialogue = dialogue;
            _currentLineIndex = 0;
            StartCoroutine(RunDialogue());
        }

        public void SkipOrAdvance()
        {
            if (!_isRunning)
            {
                return;
            }

            if (_dialogueUI.IsPlaying)
            {
                _dialogueUI.SkipCurrentLine();
                return;
            }

            _isWaitingForAdvance = false;
        }

        private IEnumerator RunDialogue()
        {
            _isRunning = true;

            while (_currentDialogue != null && _currentLineIndex < _currentDialogue.Lines.Length)
            {
                DialogueLineSO line = _currentDialogue.Lines[_currentLineIndex];
                yield return _dialogueUI.ShowLine(line);

                _isWaitingForAdvance = true;
                while (_isWaitingForAdvance)
                {
                    yield return null;
                }

                _currentLineIndex++;
            }

            CompleteDialogue();
        }

        private void CompleteDialogue()
        {
            if (_currentDialogue != null && _currentDialogue.SetFlagOnComplete != null)
            {
                _currentDialogue.SetFlagOnComplete.Value = true;
            }

            if (_currentDialogue != null && _currentDialogue.Choices != null && _currentDialogue.Choices.Length > 0)
            {
                _dialogueUI.ShowChoices(_currentDialogue.Choices, SelectChoice);
            }
            else
            {
                _dialogueUI.Hide();
                _isRunning = false;
            }
        }

        private void SelectChoice(DialogueChoiceSO choice)
        {
            if (choice == null)
            {
                return;
            }

            choice.ApplyResult();

            if (choice.NextDialogue != null)
            {
                StartDialogue(choice.NextDialogue);
                return;
            }

            _dialogueUI.Hide();
            _isRunning = false;
        }

        private void OnDialogueRequested(DialogueRequestedEvent evt)
        {
            StartDialogue(evt.Dialogue);
        }
    }
}
