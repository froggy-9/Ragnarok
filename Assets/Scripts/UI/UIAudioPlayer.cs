using DeadLetterOffice.Core;
using UnityEngine;

namespace DeadLetterOffice.UI
{
    public class UIAudioPlayer : MonoBehaviour
    {
        private static UIAudioPlayer _instance;

        [SerializeField] private UIAudioProfileSO _profile;

        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public static void Play(UIAudioKind kind)
        {
            if (_instance == null || _instance._profile == null)
            {
                return;
            }

            AudioCueSO cue = _instance._profile.GetCue(kind);
            if (cue != null)
            {
                GameEventBus.Publish(new AudioPlayEvent(cue));
            }
        }
    }
}
