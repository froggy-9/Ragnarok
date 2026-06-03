using TMPro;
using UnityEngine;

namespace DeadLetterOffice.Board
{
    public class BoardDebugConnector : MonoBehaviour
    {
        [SerializeField] private BoardManager _boardManager;
        [SerializeField] private BoardCardSO _fromCard;
        [SerializeField] private BoardCardSO _toCard;
        [SerializeField] private TMP_Text _resultText;

        public void TryConnect()
        {
            if (_boardManager == null)
            {
                SetResult("BoardManager missing");
                return;
            }

            bool isCorrect = _boardManager.TryConnect(_fromCard, _toCard);
            SetResult(isCorrect ? "Connection recorded" : "No confirmed connection");
        }

        private void SetResult(string text)
        {
            if (_resultText != null)
            {
                _resultText.text = text;
            }
        }
    }
}
