using DeadLetterOffice.Core;
using DeadLetterOffice.Interfaces;
using DeadLetterOffice.Letter;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Interaction
{
    public class CollectibleObject : MonoBehaviour, IInteractable, ICollectible, IInteractionPromptProvider
    {
        [SerializeField] private GameStateSO _gameState;
        [SerializeField] private CollectibleSO _collectible;
        [SerializeField, Range(1, 999)] private int _amount = 1;
        [SerializeField] private string _promptText = "줍기";
        [SerializeField] private FlagSO _setFlagOnCollect;
        [SerializeField] private AudioCueSO _collectSfx;
        [SerializeField] private bool _disableAfterCollect = true;

        public string ItemId => _collectible != null ? _collectible.ItemId : string.Empty;

        public bool CanInteract()
        {
            return _collectible != null && _gameState != null;
        }

        public string GetPromptText()
        {
            if (!string.IsNullOrWhiteSpace(_promptText))
            {
                return _promptText;
            }

            return _collectible != null ? $"줍기: {_collectible.DisplayName}" : "줍기";
        }

        public void OnInteract()
        {
            OnCollect();
        }

        public void OnCollect()
        {
            if (!CanInteract())
            {
                Debug.LogWarning("[CollectibleObject] Missing GameStateSO or CollectibleSO.");
                return;
            }

            if (!_gameState.AddItem(_collectible, _amount))
            {
                Debug.LogWarning("[CollectibleObject] Inventory is full or the item cannot be added.");
                return;
            }

            if (_setFlagOnCollect != null)
            {
                _setFlagOnCollect.Value = true;
            }

            GameEventBus.Publish(new ItemCollectedEvent(_collectible, _amount));

            if (_collectSfx != null)
            {
                GameEventBus.Publish(new AudioPlayEvent(_collectSfx));
            }

            if (_disableAfterCollect)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
