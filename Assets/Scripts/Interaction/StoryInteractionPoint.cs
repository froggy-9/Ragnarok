using DeadLetterOffice.Core;
using DeadLetterOffice.Interfaces;
using DeadLetterOffice.Letter;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Interaction
{
    public class StoryInteractionPoint : MonoBehaviour, IInteractable, IInteractionPromptProvider
    {
        [Header("Condition")]
        [SerializeField] private FlagSO[] _requiredFlags;

        [Header("Prompt")]
        [SerializeField] private string _promptText = "조사";

        [Header("Result")]
        [SerializeField, TextArea(2, 5)] private string _narration;
        [SerializeField] private FlagSO[] _setFlagsOnInteract;
        [SerializeField] private CollectibleSO _giveItem;
        [SerializeField, Range(1, 999)] private int _giveItemAmount = 1;
        [SerializeField] private LetterSO _giveLetter;
        [SerializeField] private string _nextObjectiveText;
        [SerializeField] private Transform _nextObjectiveTarget;
        [SerializeField] private int _nextObjectiveStaticDistance = -1;
        [SerializeField] private AudioCueSO _interactSfx;
        [SerializeField] private bool _disableAfterInteract;

        [Header("State")]
        [SerializeField] private GameStateSO _gameState;

        public bool CanInteract()
        {
            return AreFlagsMet();
        }

        public string GetPromptText()
        {
            return string.IsNullOrWhiteSpace(_promptText) ? "조사" : _promptText;
        }

        public void OnInteract()
        {
            if (!CanInteract())
            {
                return;
            }

            if (_interactSfx != null)
            {
                GameEventBus.Publish(new AudioPlayEvent(_interactSfx));
            }

            if (_setFlagsOnInteract != null)
            {
                foreach (FlagSO flag in _setFlagsOnInteract)
                {
                    if (flag != null)
                    {
                        flag.Value = true;
                    }
                }
            }

            GiveItem();
            GiveLetter();

            if (!string.IsNullOrWhiteSpace(_narration))
            {
                GameEventBus.Publish(new NarrationRequestedEvent(_narration));
            }

            if (!string.IsNullOrWhiteSpace(_nextObjectiveText))
            {
                GameEventBus.Publish(new QuestObjectiveChangedEvent(_nextObjectiveText, _nextObjectiveTarget, _nextObjectiveStaticDistance));
            }

            if (_disableAfterInteract)
            {
                gameObject.SetActive(false);
            }
        }

        private void GiveItem()
        {
            if (_gameState == null || _giveItem == null)
            {
                return;
            }

            if (_gameState.AddItem(_giveItem, _giveItemAmount))
            {
                GameEventBus.Publish(new ItemCollectedEvent(_giveItem, _giveItemAmount));
            }
        }

        private void GiveLetter()
        {
            if (_gameState == null || _giveLetter == null)
            {
                return;
            }

            _gameState.AddLetter(_giveLetter);
            GameEventBus.Publish(new LetterFoundEvent(_giveLetter));
        }

        private bool AreFlagsMet()
        {
            if (_requiredFlags == null)
            {
                return true;
            }

            foreach (FlagSO flag in _requiredFlags)
            {
                if (flag != null && !flag.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
