using System.Collections.Generic;
using DeadLetterOffice.Core;
using DeadLetterOffice.Letter;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Archive
{
    public class ArchiveSystem : MonoBehaviour
    {
        [SerializeField] private GameStateSO _gameState;
        [SerializeField] private CharacterEntrySO[] _characterEntries;

        private readonly HashSet<LetterSO> _knownLetters = new();
        private readonly HashSet<CollectibleSO> _knownItems = new();
        private readonly List<CharacterEntrySO> _unlockedCharacterEntries = new();

        public IReadOnlyCollection<LetterSO> KnownLetters => _knownLetters;
        public IReadOnlyCollection<CollectibleSO> KnownItems => _knownItems;
        public IReadOnlyList<CharacterEntrySO> UnlockedCharacterEntries => _unlockedCharacterEntries;

        private void Awake()
        {
            if (_gameState == null)
            {
                Debug.LogError("[ArchiveSystem] GameStateSO is required.");
                enabled = false;
                return;
            }

            RefreshFromGameState();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<LetterFoundEvent>(OnLetterFound);
            GameEventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
            GameEventBus.Subscribe<FlagChangedEvent>(OnFlagChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<LetterFoundEvent>(OnLetterFound);
            GameEventBus.Unsubscribe<ItemCollectedEvent>(OnItemCollected);
            GameEventBus.Unsubscribe<FlagChangedEvent>(OnFlagChanged);
        }

        public void RefreshFromGameState()
        {
            _knownLetters.Clear();
            _knownItems.Clear();

            foreach (LetterSO letter in _gameState.CollectedLetters)
            {
                if (letter != null)
                {
                    _knownLetters.Add(letter);
                }
            }

            foreach (CollectibleSO item in _gameState.CollectedItems)
            {
                if (item != null)
                {
                    _knownItems.Add(item);
                }
            }

            RefreshCharacterEntries();
            GameEventBus.Publish(new ArchiveRefreshedEvent());
        }

        private void RefreshCharacterEntries()
        {
            _unlockedCharacterEntries.Clear();

            if (_characterEntries == null)
            {
                return;
            }

            foreach (CharacterEntrySO entry in _characterEntries)
            {
                if (entry != null && entry.IsUnlocked())
                {
                    _unlockedCharacterEntries.Add(entry);
                }
            }
        }

        private void OnLetterFound(LetterFoundEvent evt)
        {
            if (evt.Letter != null)
            {
                _knownLetters.Add(evt.Letter);
            }

            RefreshCharacterEntries();
            GameEventBus.Publish(new ArchiveRefreshedEvent());
        }

        private void OnItemCollected(ItemCollectedEvent evt)
        {
            if (evt.Item != null)
            {
                _knownItems.Add(evt.Item);
            }

            RefreshCharacterEntries();
            GameEventBus.Publish(new ArchiveRefreshedEvent());
        }

        private void OnFlagChanged(FlagChangedEvent evt)
        {
            RefreshCharacterEntries();
            GameEventBus.Publish(new ArchiveRefreshedEvent());
        }
    }
}
