using DeadLetterOffice.Core;
using DeadLetterOffice.Letter;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Dialogue
{
    public enum DialogueChoiceKind
    {
        Normal,
        EvidenceSubmit
    }

    [CreateAssetMenu(menuName = "DLO/Dialogue/Choice")]
    public class DialogueChoiceSO : ScriptableObject
    {
        [SerializeField] private DialogueChoiceKind _choiceKind = DialogueChoiceKind.Normal;
        [SerializeField] private string _choiceText;
        [SerializeField] private FlagSO[] _requiredFlags;
        [SerializeField] private CollectibleSO[] _requiredEvidenceItems;
        [SerializeField] private bool _consumeRequiredEvidenceOnSelect;
        [SerializeField] private FlagSO _setFlagOnSelect;
        [SerializeField] private DialogueSO _nextDialogue;

        public DialogueChoiceKind ChoiceKind => _choiceKind;
        public string ChoiceText => _choiceText;
        public CollectibleSO[] RequiredEvidenceItems => _requiredEvidenceItems;
        public FlagSO SetFlagOnSelect => _setFlagOnSelect;
        public DialogueSO NextDialogue => _nextDialogue;
        public bool IsEvidenceSubmit => _choiceKind == DialogueChoiceKind.EvidenceSubmit;

        public string GetDisplayText()
        {
            return IsEvidenceSubmit ? $"[증거] {_choiceText}" : _choiceText;
        }

        public bool CanSelect(GameStateSO gameState = null)
        {
            return ConditionUtility.AreFlagsMet(_requiredFlags) && AreEvidenceRequirementsMet(gameState);
        }

        public void ApplyResult(GameStateSO gameState = null)
        {
            if (_setFlagOnSelect != null)
            {
                _setFlagOnSelect.Value = true;
            }

            if (_consumeRequiredEvidenceOnSelect && gameState != null && _requiredEvidenceItems != null)
            {
                foreach (CollectibleSO item in _requiredEvidenceItems)
                {
                    if (item != null)
                    {
                        gameState.RemoveItem(item);
                    }
                }
            }
        }

        private bool AreEvidenceRequirementsMet(GameStateSO gameState)
        {
            if (_requiredEvidenceItems == null || _requiredEvidenceItems.Length == 0)
            {
                return true;
            }

            if (gameState == null)
            {
                return false;
            }

            foreach (CollectibleSO item in _requiredEvidenceItems)
            {
                if (item != null && gameState.CountItem(item) <= 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
