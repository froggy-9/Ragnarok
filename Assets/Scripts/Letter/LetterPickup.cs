using DeadLetterOffice.Core;
using DeadLetterOffice.Interfaces;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Letter
{
    public class LetterPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameStateSO _gameState;
        [SerializeField] private LetterSO _letter;
        [SerializeField] private FlagSO _setFlagOnFound;
        [SerializeField] private AudioCueSO _pickupSfx;
        [SerializeField] private bool _disableAfterPickup = true;

        public bool CanInteract()
        {
            return _gameState != null && _letter != null;
        }

        public void OnInteract()
        {
            if (!CanInteract())
            {
                Debug.LogWarning("[LetterPickup] Missing GameStateSO or LetterSO.");
                return;
            }

            _gameState.AddLetter(_letter);

            if (_setFlagOnFound != null)
            {
                _setFlagOnFound.Value = true;
            }

            GameEventBus.Publish(new LetterFoundEvent(_letter));

            if (_pickupSfx != null)
            {
                GameEventBus.Publish(new AudioPlayEvent(_pickupSfx));
            }

            if (_disableAfterPickup)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
