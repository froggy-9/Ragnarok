using System.Collections;
using DeadLetterOffice.Core;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueUI _dialogueUI;
        [SerializeField] private GameStateSO _gameState;

        private DialogueSO _currentDialogue;
        private int _currentLineIndex;
        private bool _isWaitingForAdvance;
        private bool _isRunning;
        private GameMode _previousMode = GameMode.Exploration;

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
            else if (ServiceLocator.TryGet(out GameModeManager modeManager))
            {
                _previousMode = modeManager.CurrentMode;
            }

            if (ServiceLocator.TryGet(out GameModeManager activeModeManager))
            {
                activeModeManager.SetDialogueMode();
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
                if (line.AudioCue != null)
                {
                    GameEventBus.Publish(new AudioPlayEvent(line.AudioCue));
                }

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
                _dialogueUI.ShowChoices(_currentDialogue.Choices, SelectChoice, _gameState);
            }
            else
            {
                _dialogueUI.Hide();
                EndDialogue();
            }
        }

        private void SelectChoice(DialogueChoiceSO choice)
        {
            if (choice == null)
            {
                return;
            }

            choice.ApplyResult(_gameState);

            if (choice.NextDialogue != null)
            {
                StartDialogue(choice.NextDialogue);
                return;
            }

            _dialogueUI.Hide();
            EndDialogue();
        }

        private void OnDialogueRequested(DialogueRequestedEvent evt)
        {
            StartDialogue(evt.Dialogue);
        }

        private void EndDialogue()
        {
            _isRunning = false;
            _isWaitingForAdvance = false;

            if (ServiceLocator.TryGet(out GameModeManager modeManager))
            {
                modeManager.SetMode(_previousMode);
            }
        }
    }
}
