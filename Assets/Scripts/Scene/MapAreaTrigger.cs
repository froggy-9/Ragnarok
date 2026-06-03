using DeadLetterOffice.Core;
using DeadLetterOffice.State;
using UnityEngine;

namespace DeadLetterOffice.Scene
{
    public class MapAreaTrigger : MonoBehaviour
    {
        [SerializeField] private string _areaId;
        [SerializeField] private FlagSO _setFlagOnEnter;
        [SerializeField] private AudioCueSO _ambientCue;
        [SerializeField] private string _playerTag = "Player";

        public string AreaId => _areaId;

        private void OnTriggerEnter(Collider other)
        {
            if (!string.IsNullOrEmpty(_playerTag) && !other.CompareTag(_playerTag))
            {
                return;
            }

            if (_setFlagOnEnter != null)
            {
                _setFlagOnEnter.Value = true;
            }

            if (_ambientCue != null)
            {
                GameEventBus.Publish(new AudioPlayEvent(_ambientCue));
            }
        }
    }
}
