using DeadLetterOffice.Core;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Chapter
{
    [CreateAssetMenu(menuName = "DLO/Chapter/Beat")]
    public class BeatSO : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(2, 8)] private string _description;
        [SerializeField] private FlagSO[] _requiredFlags;
        [SerializeField] private FlagSO _completionFlag;

        public string BeatId => name;
        public string DisplayName => _displayName;
        public string Description => _description;
        public FlagSO CompletionFlag => _completionFlag;

        public bool CanEnter()
        {
            return ConditionUtility.AreFlagsMet(_requiredFlags);
        }
    }
}
