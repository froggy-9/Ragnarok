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

        private void Awake()
        {
            TryBindPlayer();
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
    }
}
