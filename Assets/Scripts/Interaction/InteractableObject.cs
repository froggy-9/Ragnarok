using DeadLetterOffice.Core;
using DeadLetterOffice.Interfaces;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Interaction
{
    public class InteractableObject : MonoBehaviour, IInteractable, IInspectable
    {
        [SerializeField] private FlagSO _requiredFlag;
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
