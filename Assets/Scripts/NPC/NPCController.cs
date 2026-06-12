using DeadLetterOffice.Core;
using DeadLetterOffice.Dialogue;
using DeadLetterOffice.Interfaces;
using UnityEngine;

namespace DeadLetterOffice.NPC
{
    public class NPCController : MonoBehaviour, IInteractable, IInteractionPromptProvider
    {
        [SerializeField] private NPCDataSO _npcData;
        [SerializeField] private string _promptText = "대화";

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

            GameEventBus.Publish(new DialogueRequestedEvent(_cachedDialogue, transform));
        }

        public string GetPromptText()
        {
            return string.IsNullOrWhiteSpace(_promptText) ? "대화" : _promptText;
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
