using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Board
{
    [CreateAssetMenu(menuName = "DLO/Board/Connection")]
    public class BoardConnectionSO : ScriptableObject
    {
        [SerializeField] private BoardCardSO _fromCard;
        [SerializeField] private BoardCardSO _toCard;
        [SerializeField] private FlagSO _completionFlag;
        [SerializeField, TextArea(1, 4)] private string _successMemo;

        public string ConnectionId => name;
        public BoardCardSO FromCard => _fromCard;
        public BoardCardSO ToCard => _toCard;
        public FlagSO CompletionFlag => _completionFlag;
        public string SuccessMemo => _successMemo;
    }
}
