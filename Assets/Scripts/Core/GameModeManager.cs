using UnityEngine;

namespace DeadLetterOffice.Core
{
    public class GameModeManager : MonoBehaviour
    {
        [SerializeField] private GameMode _initialMode = GameMode.Exploration;

        public GameMode CurrentMode { get; private set; }
        public bool CanControlPlayer => CurrentMode == GameMode.Exploration;
        public bool CanControlCamera => CurrentMode == GameMode.Exploration;

        private void Awake()
        {
            ServiceLocator.Register(this);
            SetMode(_initialMode);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<GameModeManager>();
        }

        public void SetExplorationMode()
        {
            SetMode(GameMode.Exploration);
        }

        public void SetDialogueMode()
        {
            SetMode(GameMode.Dialogue);
        }

        public void SetUiMode()
        {
            SetMode(GameMode.UI);
        }

        public void SetCinematicMode()
        {
            SetMode(GameMode.Cinematic);
        }

        public void SetMode(GameMode mode)
        {
            if (CurrentMode == mode)
            {
                return;
            }

            CurrentMode = mode;
            GameEventBus.Publish(new GameModeChangedEvent(mode));
        }
    }
}
