using DeadLetterOffice.Core;
using DeadLetterOffice.Interfaces;
using DeadLetterOffice.Letter;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Interaction
{
    public class CollectibleObject : MonoBehaviour, IInteractable, ICollectible
    {
        [SerializeField] private GameStateSO _gameState;
        [SerializeField] private CollectibleSO _collectible;
        [SerializeField] private FlagSO _setFlagOnCollect;
        [SerializeField] private AudioCueSO _collectSfx;
        [SerializeField] private bool _disableAfterCollect = true;

        public string ItemId => _collectible != null ? _collectible.ItemId : string.Empty;

        public bool CanInteract()
        {
            return _collectible != null && _gameState != null;
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

            _gameState.AddItem(_collectible);

            if (_setFlagOnCollect != null)
            {
                _setFlagOnCollect.Value = true;
            }

            GameEventBus.Publish(new ItemCollectedEvent(_collectible));

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
