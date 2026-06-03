using UnityEngine;
using DeadLetterOffice.Core;
using DeadLetterOffice.State;

namespace DeadLetterOffice.Board
{
    public enum BoardCardType
    {
        Letter,
        Character,
        Place,
        Event,
        Object
    }

    [CreateAssetMenu(menuName = "DLO/Board/Card")]
    public class BoardCardSO : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private BoardCardType _cardType;
        [SerializeField, TextArea(2, 6)] private string _summary;
        [SerializeField] private Sprite _icon;
        [SerializeField] private FlagSO[] _requiredFlags;

        public string CardId => name;
        public string DisplayName => _displayName;
        public BoardCardType CardType => _cardType;
        public string Summary => _summary;
        public Sprite Icon => _icon;

        public bool IsUnlocked()
        {
            return ConditionUtility.AreFlagsMet(_requiredFlags);
        }
    }
}
