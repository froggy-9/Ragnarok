using DeadLetterOffice.Core;
using DeadLetterOffice.Dialogue;
using DeadLetterOffice.Interfaces;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Interaction
{
    public class DialogueInteractionPoint : MonoBehaviour, IInteractable, IInteractionPromptProvider
    {
        [Header("Condition")]
        [SerializeField] private FlagSO[] _requiredFlags;

        [Header("Prompt")]
        [SerializeField] private string _promptText = "대화";

        [Header("Dialogue")]
        [SerializeField] private DialogueSO _dialogue;

        [Header("Result")]
        [SerializeField] private FlagSO[] _setFlagsOnInteract;
        [SerializeField] private AudioCueSO _interactSfx;
        [SerializeField] private bool _disableAfterInteract;

        public bool CanInteract()
        {
            return _dialogue != null && AreFlagsMet();
        }

        public string GetPromptText()
        {
            return string.IsNullOrWhiteSpace(_promptText) ? "대화" : _promptText;
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

            GameEventBus.Publish(new DialogueRequestedEvent(_dialogue, transform));

            if (_disableAfterInteract)
            {
                gameObject.SetActive(false);
            }
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
