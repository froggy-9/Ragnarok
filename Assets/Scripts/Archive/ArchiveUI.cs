using System.Text;
using DeadLetterOffice.Core;
using TMPro;
using UnityEngine;

namespace DeadLetterOffice.Archive
{
    public class ArchiveUI : MonoBehaviour
    {
        [SerializeField] private ArchiveSystem _archiveSystem;
        [SerializeField] private TMP_Text _lettersText;
        [SerializeField] private TMP_Text _itemsText;
        [SerializeField] private TMP_Text _charactersText;

        private void OnEnable()
        {
            GameEventBus.Subscribe<ArchiveRefreshedEvent>(OnArchiveRefreshed);
            Refresh();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<ArchiveRefreshedEvent>(OnArchiveRefreshed);
        }

        public void Refresh()
        {
            if (_archiveSystem == null)
            {
                return;
            }

            StringBuilder letters = new();
            foreach (Letter.LetterSO letter in _archiveSystem.KnownLetters)
            {
                if (letter != null)
                {
                    letters.AppendLine($"{letter.LetterNumber}  {letter.Sender} -> {letter.Recipient}");
                }
            }

            StringBuilder items = new();
            foreach (Letter.CollectibleSO item in _archiveSystem.KnownItems)
            {
                if (item != null)
                {
                    items.AppendLine(item.DisplayName);
                }
            }

            StringBuilder characters = new();
            foreach (CharacterEntrySO entry in _archiveSystem.UnlockedCharacterEntries)
            {
                if (entry != null && entry.CharacterFile != null)
                {
                    characters.AppendLine(entry.CharacterFile.DisplayName);
                    characters.AppendLine(entry.EntryText);
                    characters.AppendLine();
                }
            }

            SetText(_lettersText, letters.ToString());
            SetText(_itemsText, items.ToString());
            SetText(_charactersText, characters.ToString());
        }

        private void OnArchiveRefreshed(ArchiveRefreshedEvent evt)
        {
            Refresh();
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
