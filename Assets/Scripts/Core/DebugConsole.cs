using DeadLetterOffice.Board;
using DeadLetterOffice.Chapter;
using DeadLetterOffice.Letter;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Core
{
    public class DebugConsole : MonoBehaviour
    {
        [SerializeField] private GameStateSO _gameState;
        [SerializeField] private LetterSO[] _allLetters;
        [SerializeField] private CollectibleSO[] _allCollectibles;
        [SerializeField] private BoardConnectionSO[] _allConnections;
        [SerializeField] private BeatSO _climaxBeat;

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (UnityEngine.InputSystem.Keyboard.current == null)
            {
                return;
            }

            if (UnityEngine.InputSystem.Keyboard.current.f1Key.wasPressedThisFrame)
            {
                UnlockAllFlags();
            }

            if (UnityEngine.InputSystem.Keyboard.current.f2Key.wasPressedThisFrame)
            {
                CollectAllLetters();
            }

            if (UnityEngine.InputSystem.Keyboard.current.f3Key.wasPressedThisFrame)
            {
                JumpToClimaxBeat();
            }
#endif
        }

        public void UnlockAllFlags()
        {
            if (_gameState == null)
            {
                return;
            }

            foreach (FlagSO flag in _gameState.GetAllFlags())
            {
                if (flag != null)
                {
                    flag.Value = true;
                }
            }
        }

        public void CollectAllLetters()
        {
            if (_gameState == null || _allLetters == null)
            {
                return;
            }

            foreach (LetterSO letter in _allLetters)
            {
                if (letter != null)
                {
                    _gameState.AddLetter(letter);
                    GameEventBus.Publish(new LetterFoundEvent(letter));
                }
            }

            if (_allCollectibles == null)
            {
                return;
            }

            foreach (CollectibleSO collectible in _allCollectibles)
            {
                if (collectible != null)
                {
                    _gameState.AddItem(collectible);
                    GameEventBus.Publish(new ItemCollectedEvent(collectible));
                }
            }
        }

        public void CompleteAllConnections()
        {
            if (_allConnections == null)
            {
                return;
            }

            foreach (BoardConnectionSO connection in _allConnections)
            {
                if (connection != null)
                {
                    if (connection.CompletionFlag != null)
                    {
                        connection.CompletionFlag.Value = true;
                    }

                    GameEventBus.Publish(new ConnectionMadeEvent(connection));
                }
            }
        }

        public void JumpToClimaxBeat()
        {
            if (_climaxBeat != null)
            {
                GameEventBus.Publish(new BeatAdvancedEvent(_climaxBeat));
            }
        }

        public void ResetAll()
        {
            if (_gameState != null)
            {
                _gameState.ResetRuntimeState();
                GameEventBus.Publish(new ArchiveRefreshedEvent());
            }
        }
    }
}
