using UnityEngine;

namespace DeadLetterOffice.Archive
{
    [CreateAssetMenu(menuName = "DLO/Archive/Character File")]
    public class CharacterFileSO : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _shortName;
        [SerializeField] private string _codeName;
        [SerializeField, TextArea(2, 8)] private string _description;

        public string DisplayName => _displayName;
        public string ShortName => _shortName;
        public string CodeName => _codeName;
        public string Description => _description;
    }
}
