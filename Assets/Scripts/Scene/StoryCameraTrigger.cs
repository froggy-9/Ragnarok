using DeadLetterOffice.Core;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Scene
{
    public class StoryCameraTrigger : MonoBehaviour
    {
        [SerializeField] private Transform _shotTransform;
        [SerializeField] private FlagSO[] _requiredFlags;
        [SerializeField] private FlagSO _setFlagAfterPlay;
        [SerializeField] private float _duration = 1.2f;
        [SerializeField] private bool _lockPlayer = true;
        [SerializeField] private bool _playOnce = true;
        [SerializeField] private string _playerTag = "Player";

        private bool _hasPlayed;

        private void OnTriggerEnter(Collider other)
        {
            if (_playOnce && _hasPlayed)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_playerTag) && !other.CompareTag(_playerTag))
            {
                return;
            }

            if (!ConditionUtility.AreFlagsMet(_requiredFlags))
            {
                return;
            }

            _hasPlayed = true;

            if (_setFlagAfterPlay != null)
            {
                _setFlagAfterPlay.Value = true;
            }

            GameEventBus.Publish(new CameraShotRequestedEvent(_shotTransform, _duration, _lockPlayer));
        }
    }
}
