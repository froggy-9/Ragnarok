using DeadLetterOffice.Core;
using DeadLetterOffice.Dialogue;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.NPC
{
    [CreateAssetMenu(menuName = "DLO/NPC/Data")]
    public class NPCDataSO : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _portrait;
        [SerializeField] private DialogueEntry[] _dialogues;

        public string DisplayName => _displayName;
        public Sprite Portrait => _portrait;

        public DialogueSO GetBestDialogue()
        {
            if (_dialogues == null)
            {
                return null;
            }

            for (int i = _dialogues.Length - 1; i >= 0; i--)
            {
                DialogueEntry entry = _dialogues[i];
                if (entry != null && entry.IsAvailable())
                {
                    return entry.Dialogue;
                }
            }

            return null;
        }
    }

    [System.Serializable]
    public class DialogueEntry
    {
        [SerializeField] private FlagSO[] _requiredFlags;
        [SerializeField] private DialogueSO _dialogue;

        public DialogueSO Dialogue => _dialogue;

        public bool IsAvailable()
        {
            return _dialogue != null && ConditionUtility.AreFlagsMet(_requiredFlags);
        }
    }
}
