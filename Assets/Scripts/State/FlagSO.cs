using DeadLetterOffice.Core;
using UnityEngine;

namespace DeadLetterOffice.State
{
    [CreateAssetMenu(menuName = "DLO/Flag")]
    public class FlagSO : ScriptableObject
    {
        [SerializeField, TextArea(1, 3)] private string _description;

        private bool _value;

        public string Description => _description;

        public bool Value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                _value = value;
                GameEventBus.Publish(new FlagChangedEvent(this, value));
            }
        }

        public void RestoreValue(bool savedValue)
        {
            _value = savedValue;
        }

        public void ResetValue()
        {
            _value = false;
        }

        private void OnDisable()
        {
            _value = false;
        }
    }
}
