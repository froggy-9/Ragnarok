using DeadLetterOffice.Core;
using DeadLetterOffice.Dialogue;
using DeadLetterOffice.Interfaces;
using UnityEngine;

namespace DeadLetterOffice.NPC
{
    public class NPCController : MonoBehaviour, IInteractable
    {
        [SerializeField] private NPCDataSO _npcData;

        private DialogueSO _cachedDialogue;

        private void OnEnable()
        {
            GameEventBus.Subscribe<FlagChangedEvent>(OnFlagChanged);
            RefreshDialogue();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<FlagChangedEvent>(OnFlagChanged);
        }

        public bool CanInteract()
        {
            return _npcData != null && _cachedDialogue != null;
        }

        public void OnInteract()
        {
            if (!CanInteract())
            {
                Debug.LogWarning("[NPCController] No available dialogue.");
                return;
            }

            GameEventBus.Publish(new DialogueRequestedEvent(_cachedDialogue));
        }

        private void RefreshDialogue()
        {
            _cachedDialogue = _npcData != null ? _npcData.GetBestDialogue() : null;
        }

        private void OnFlagChanged(FlagChangedEvent evt)
        {
            RefreshDialogue();
        }
    }
}
