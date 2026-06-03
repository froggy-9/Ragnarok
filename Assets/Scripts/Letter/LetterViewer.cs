using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeadLetterOffice.Letter
{
    public class LetterViewer : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _letterNumberText;
        [SerializeField] private TMP_Text _recipientText;
        [SerializeField] private TMP_Text _senderText;
        [SerializeField] private TMP_Text _postmarkText;
        [SerializeField] private TMP_Text _bodyText;
        [SerializeField] private Image _letterImage;

        private LetterSO _currentLetter;

        private void Awake()
        {
            Hide();
        }

        public void Show(LetterSO letter)
        {
            if (letter == null)
            {
                Debug.LogWarning("[LetterViewer] Cannot show null letter.");
                return;
            }

            _currentLetter = letter;
            Refresh();

            if (_root != null)
            {
                _root.SetActive(true);
            }
        }

        public void Refresh()
        {
            if (_currentLetter == null)
            {
                return;
            }

            SetText(_letterNumberText, _currentLetter.LetterNumber);
            SetText(_recipientText, _currentLetter.Recipient);
            SetText(_senderText, _currentLetter.Sender);
            SetText(_postmarkText, _currentLetter.Postmark);
            SetText(_bodyText, BuildBodyText(_currentLetter));

            if (_letterImage != null)
            {
                _letterImage.sprite = _currentLetter.LetterImage;
                _letterImage.enabled = _currentLetter.LetterImage != null;
            }
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        private static string BuildBodyText(LetterSO letter)
        {
            StringBuilder builder = new();

            if (letter.Segments == null)
            {
                return string.Empty;
            }

            foreach (LetterSegmentSO segment in letter.Segments)
            {
                if (segment == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine();
                }

                builder.Append(segment.GetVisibleText());
            }

            return builder.ToString();
        }

        private static void SetText(TMP_Text target, string text)
        {
            if (target != null)
            {
                target.text = text;
            }
        }
    }
}
