using System.Collections.Generic;
using DeadLetterOffice.Letter;
using UnityEngine;

namespace DeadLetterOffice.State
{
    [CreateAssetMenu(menuName = "DLO/GameState")]
    public class GameStateSO : ScriptableObject
    {
        [Header("Chapter 1 - Letter Flags")]
        [SerializeField] private FlagSO _letter01Found;
        [SerializeField] private FlagSO _letter02Found;
        [SerializeField] private FlagSO _letter03Found;
        [SerializeField] private FlagSO _letter04Found;
        [SerializeField] private FlagSO _letter05Found;
        [SerializeField] private FlagSO _letter06Found;
        [SerializeField] private FlagSO _letter07Found;
        [SerializeField] private FlagSO _letter08Found;

        [Header("Chapter 1 - Board Flags")]
        [SerializeField] private FlagSO _boardCrowSparrow;
        [SerializeField] private FlagSO _boardSparrowManager;
        [SerializeField] private FlagSO _boardMorelNetwork;
        [SerializeField] private FlagSO _boardPlayerLetter7;

        [Header("Chapter 1 - NPC Flags")]
        [SerializeField] private FlagSO _npcJanitorTalked;
        [SerializeField] private FlagSO _npcUmbrellaTalked;
        [SerializeField] private FlagSO _umbrellaShown;

        [Header("Chapter 1 - Beat Flags")]
        [SerializeField] private FlagSO _beat04LucaMet;
        [SerializeField] private FlagSO _beat05Triggered;
        [SerializeField] private FlagSO _chapter01End;

        [Header("Collections")]
        [SerializeField] private List<CollectibleSO> _collectedItems = new();
        [SerializeField] private List<LetterSO> _collectedLetters = new();

        public IReadOnlyList<CollectibleSO> CollectedItems => _collectedItems;
        public IReadOnlyList<LetterSO> CollectedLetters => _collectedLetters;

        public IEnumerable<FlagSO> GetAllFlags()
        {
            yield return _letter01Found;
            yield return _letter02Found;
            yield return _letter03Found;
            yield return _letter04Found;
            yield return _letter05Found;
            yield return _letter06Found;
            yield return _letter07Found;
            yield return _letter08Found;
            yield return _boardCrowSparrow;
            yield return _boardSparrowManager;
            yield return _boardMorelNetwork;
            yield return _boardPlayerLetter7;
            yield return _npcJanitorTalked;
            yield return _npcUmbrellaTalked;
            yield return _umbrellaShown;
            yield return _beat04LucaMet;
            yield return _beat05Triggered;
            yield return _chapter01End;
        }

        public void AddLetter(LetterSO letter)
        {
            if (letter != null && !_collectedLetters.Contains(letter))
            {
                _collectedLetters.Add(letter);
            }
        }

        public void AddItem(CollectibleSO item)
        {
            if (item != null && !_collectedItems.Contains(item))
            {
                _collectedItems.Add(item);
            }
        }

        public void RestoreCollections(IEnumerable<LetterSO> letters, IEnumerable<CollectibleSO> items)
        {
            _collectedLetters.Clear();
            _collectedItems.Clear();

            if (letters != null)
            {
                _collectedLetters.AddRange(letters);
            }

            if (items != null)
            {
                _collectedItems.AddRange(items);
            }
        }

        public void ResetRuntimeState()
        {
            foreach (FlagSO flag in GetAllFlags())
            {
                if (flag != null)
                {
                    flag.ResetValue();
                }
            }

            _collectedItems.Clear();
            _collectedLetters.Clear();
        }
    }
}
