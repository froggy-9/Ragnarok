using DeadLetterOffice.Core;
using DeadLetterOffice.Interfaces;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Interaction
{
    public class InteractableObject : MonoBehaviour, IInteractable, IInspectable, IInteractionPromptProvider
    {
        [SerializeField] private FlagSO _requiredFlag;
        [SerializeField] private string _promptText = "조사";
        [SerializeField, TextArea(2, 5)] private string _narration;
        [SerializeField] private AudioCueSO _interactSfx;

        public FlagSO RequiredFlag => _requiredFlag;

        public bool CanInteract()
        {
            return _requiredFlag == null || _requiredFlag.Value;
        }

        public string GetNarration()
        {
            return _narration;
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

            if (!string.IsNullOrWhiteSpace(_narration))
            {
                GameEventBus.Publish(new NarrationRequestedEvent(_narration));
            }
        }
    }
}
