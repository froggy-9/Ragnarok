using DeadLetterOffice.Core;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Archive
{
    [CreateAssetMenu(menuName = "DLO/Archive/Character Entry")]
    public class CharacterEntrySO : ScriptableObject
    {
        [SerializeField] private CharacterFileSO _characterFile;
        [SerializeField] private FlagSO[] _requiredFlags;
        [SerializeField, TextArea(2, 6)] private string _entryText;

        public CharacterFileSO CharacterFile => _characterFile;
        public string EntryText => _entryText;

        public bool IsUnlocked()
        {
            return ConditionUtility.AreFlagsMet(_requiredFlags);
        }
    }
}
