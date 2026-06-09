using DeadLetterOffice.Core;
using UnityEngine;

namespace DeadLetterOffice.UI
{
    public class DLOHudAutoBinder : MonoBehaviour
    {
        [SerializeField] private MiniMapUI _miniMap;
        [SerializeField] private MapViewUI _mapView;
        [SerializeField] private QuestObjectiveHUD _questObjective;
        [SerializeField] private string _playerTag = "Player";

        private Transform _boundPlayer;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            EnsureCanvasGroup();
            TryBindPlayer();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<GameModeChangedEvent>(OnGameModeChanged);
            ApplyVisibility(GetCurrentMode());
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<GameModeChangedEvent>(OnGameModeChanged);
        }

        private void LateUpdate()
        {
            if (_boundPlayer == null)
            {
                TryBindPlayer();
            }
        }

        private void TryBindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag(_playerTag);
            if (player == null)
            {
                return;
            }

            _boundPlayer = player.transform;
            if (_miniMap != null)
            {
                _miniMap.SetPlayer(_boundPlayer);
            }

            if (_mapView != null)
            {
                _mapView.SetPlayer(_boundPlayer);
            }

            if (_questObjective != null)
            {
                _questObjective.SetPlayer(_boundPlayer);
            }
        }

        private void OnGameModeChanged(GameModeChangedEvent evt)
        {
            ApplyVisibility(evt.Mode);
        }

        private void ApplyVisibility(GameMode mode)
        {
            EnsureCanvasGroup();

            bool shouldShow = mode != GameMode.Dialogue && mode != GameMode.Cinematic;
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = shouldShow ? 1f : 0f;
            _canvasGroup.interactable = shouldShow;
            _canvasGroup.blocksRaycasts = shouldShow;
        }

        private void EnsureCanvasGroup()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private static GameMode GetCurrentMode()
        {
            return ServiceLocator.TryGet(out GameModeManager modeManager) ? modeManager.CurrentMode : GameMode.Exploration;
        }
    }
}
