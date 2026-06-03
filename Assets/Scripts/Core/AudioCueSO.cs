using UnityEngine;

namespace DeadLetterOffice.Core
{
    public enum AudioType
    {
        BGM,
        Ambient,
        SFX,
        Voice
    }

    [CreateAssetMenu(menuName = "DLO/Audio/Cue")]
    public class AudioCueSO : ScriptableObject
    {
        [SerializeField] private AudioClip[] _clips;
        [SerializeField] private AudioType _audioType;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField, Range(-3f, 3f)] private float _pitch = 1f;
        [SerializeField] private bool _loop;
        [SerializeField] private bool _randomizePitch;
        [SerializeField, Range(0f, 0.5f)] private float _pitchVariance = 0.1f;

        public AudioType AudioType => _audioType;
        public float Volume => _volume;
        public float Pitch => _pitch;
        public bool Loop => _loop;
        public bool RandomizePitch => _randomizePitch;
        public float PitchVariance => _pitchVariance;

        public AudioClip GetClip()
        {
            if (_clips == null || _clips.Length == 0)
            {
                return null;
            }

            return _clips[Random.Range(0, _clips.Length)];
        }
    }
}
