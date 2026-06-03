using DeadLetterOffice.Core;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Dialogue
{
    [CreateAssetMenu(menuName = "DLO/Dialogue/Choice")]
    public class DialogueChoiceSO : ScriptableObject
    {
        [SerializeField] private string _choiceText;
        [SerializeField] private FlagSO[] _requiredFlags;
        [SerializeField] private FlagSO _setFlagOnSelect;
        [SerializeField] private DialogueSO _nextDialogue;

        public string ChoiceText => _choiceText;
        public FlagSO SetFlagOnSelect => _setFlagOnSelect;
        public DialogueSO NextDialogue => _nextDialogue;

        public bool CanSelect()
        {
            return ConditionUtility.AreFlagsMet(_requiredFlags);
        }

        public void ApplyResult()
        {
            if (_setFlagOnSelect != null)
            {
                _setFlagOnSelect.Value = true;
            }
        }
    }
}
