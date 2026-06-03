using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Letter
{
    [CreateAssetMenu(menuName = "DLO/Letter/Segment")]
    public class LetterSegmentSO : ScriptableObject
    {
        [SerializeField, TextArea(2, 8)] private string _text;
        [SerializeField, TextArea(2, 8)] private string _redactedText;
        [SerializeField] private FlagSO _revealFlag;

        public string GetVisibleText()
        {
            if (_revealFlag == null || _revealFlag.Value)
            {
                return _text;
            }

            return string.IsNullOrEmpty(_redactedText) ? "████" : _redactedText;
        }
    }
}
