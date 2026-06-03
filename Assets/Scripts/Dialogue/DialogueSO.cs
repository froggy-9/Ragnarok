using DeadLetterOffice.Core;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Dialogue
{
    [CreateAssetMenu(menuName = "DLO/Dialogue/Dialogue")]
    public class DialogueSO : ScriptableObject
    {
        [SerializeField] private FlagSO[] _requiredFlags;
        [SerializeField] private DialogueLineSO[] _lines;
        [SerializeField] private DialogueChoiceSO[] _choices;
        [SerializeField] private FlagSO _setFlagOnComplete;

        public DialogueLineSO[] Lines => _lines;
        public DialogueChoiceSO[] Choices => _choices;
        public FlagSO SetFlagOnComplete => _setFlagOnComplete;

        public bool CanPlay()
        {
            return ConditionUtility.AreFlagsMet(_requiredFlags);
        }
    }
}
