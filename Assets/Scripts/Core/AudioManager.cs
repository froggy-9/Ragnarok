using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeadLetterOffice.Core
{
    public class AudioManager : MonoBehaviour
    {
        private const string MasterVolumeKey = "DLO_Audio_MasterVolume";
        private const string BgmVolumeKey = "DLO_Audio_BgmVolume";
        private const string SfxVolumeKey = "DLO_Audio_SfxVolume";

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _ambientSource;
        [SerializeField] private AudioSource _voiceSource;

        [Header("SFX Pool")]
        [SerializeField] private int _sfxPoolSize = 10;
        [SerializeField] private AudioSource _sfxSourcePrefab;

        [Header("Volume")]
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _bgmVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;

        private readonly Queue<AudioSource> _sfxPool = new();
        private float _currentBgmCueVolume = 1f;
        private float _currentAmbientCueVolume = 1f;
        private float _currentVoiceCueVolume = 1f;

        public float MasterVolume => _masterVolume;
        public float BgmVolume => _bgmVolume;
        public float SfxVolume => _sfxVolume;

        private void Awake()
        {
            if (_bgmSource == null || _ambientSource == null || _voiceSource == null || _sfxSourcePrefab == null)
            {
                Debug.LogError("[AudioManager] Audio sources and SFX prefab are required.");
                enabled = false;
                return;
            }

            ServiceLocator.Register(this);
            LoadVolumeSettings();
            ApplySourceVolumes();
            InitSfxPool();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<AudioPlayEvent>(OnAudioPlay);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<AudioPlayEvent>(OnAudioPlay);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<AudioManager>();
        }

        public void Play(AudioCueSO cue)
        {
            if (cue == null)
            {
                return;
            }

            switch (cue.AudioType)
            {
                case AudioType.BGM:
                    PlayBgm(cue);
                    break;
                case AudioType.Ambient:
                    PlayAmbient(cue);
                    break;
                case AudioType.SFX:
                    PlaySfx(cue);
                    break;
                case AudioType.Voice:
                    PlayVoice(cue);
                    break;
            }
        }

        private void OnAudioPlay(AudioPlayEvent evt)
        {
            Play(evt.Cue);
        }

        private void PlayBgm(AudioCueSO cue)
        {
            AudioClip clip = cue.GetClip();
            if (clip == null)
            {
                return;
            }

            _bgmSource.clip = clip;
            _currentBgmCueVolume = cue.Volume;
            _bgmSource.volume = _currentBgmCueVolume * _bgmVolume * _masterVolume;
            _bgmSource.pitch = cue.Pitch;
            _bgmSource.loop = cue.Loop;
            _bgmSource.Play();
        }

        private void PlayAmbient(AudioCueSO cue)
        {
            AudioClip clip = cue.GetClip();
            if (clip == null)
            {
                return;
            }

            _ambientSource.clip = clip;
            _currentAmbientCueVolume = cue.Volume;
            _ambientSource.volume = _currentAmbientCueVolume * _masterVolume;
            _ambientSource.pitch = cue.Pitch;
            _ambientSource.loop = true;
            _ambientSource.Play();
        }

        private void PlayVoice(AudioCueSO cue)
        {
            AudioClip clip = cue.GetClip();
            if (clip == null)
            {
                return;
            }

            _voiceSource.Stop();
            _voiceSource.clip = clip;
            _currentVoiceCueVolume = cue.Volume;
            _voiceSource.volume = _currentVoiceCueVolume * _masterVolume;
            _voiceSource.pitch = cue.Pitch;
            _voiceSource.loop = false;
            _voiceSource.Play();
        }

        private void PlaySfx(AudioCueSO cue)
        {
            AudioClip clip = cue.GetClip();
            if (clip == null)
            {
                return;
            }

            AudioSource source = GetPooledSfx();
            source.clip = clip;
            source.volume = cue.Volume * _sfxVolume * _masterVolume;
            source.pitch = cue.RandomizePitch
                ? cue.Pitch + Random.Range(-cue.PitchVariance, cue.PitchVariance)
                : cue.Pitch;
            source.loop = false;
            source.Play();

            StartCoroutine(ReturnToPool(source, clip.length));
        }

        private void InitSfxPool()
        {
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                AudioSource source = Instantiate(_sfxSourcePrefab, transform);
                source.gameObject.SetActive(false);
                _sfxPool.Enqueue(source);
            }
        }

        private AudioSource GetPooledSfx()
        {
            if (_sfxPool.Count > 0)
            {
                AudioSource source = _sfxPool.Dequeue();
                source.gameObject.SetActive(true);
                return source;
            }

            Debug.LogWarning("[AudioManager] SFX pool exhausted. Consider increasing pool size.");
            return Instantiate(_sfxSourcePrefab, transform);
        }

        private IEnumerator ReturnToPool(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            _sfxPool.Enqueue(source);
        }

        public void SetMasterVolume(float value)
        {
            _masterVolume = Mathf.Clamp01(value);
            SaveVolumeSettings();
            ApplySourceVolumes();
        }

        public void SetBgmVolume(float value)
        {
            _bgmVolume = Mathf.Clamp01(value);
            SaveVolumeSettings();
            ApplySourceVolumes();
        }

        public void SetSfxVolume(float value)
        {
            _sfxVolume = Mathf.Clamp01(value);
            SaveVolumeSettings();
        }

        private void ApplySourceVolumes()
        {
            if (_bgmSource != null)
            {
                _bgmSource.volume = _currentBgmCueVolume * _bgmVolume * _masterVolume;
            }

            if (_ambientSource != null)
            {
                _ambientSource.volume = _currentAmbientCueVolume * _masterVolume;
            }

            if (_voiceSource != null)
            {
                _voiceSource.volume = _currentVoiceCueVolume * _masterVolume;
            }
        }

        private void LoadVolumeSettings()
        {
            _masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, _masterVolume);
            _bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, _bgmVolume);
            _sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, _sfxVolume);
        }

        private void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, _masterVolume);
            PlayerPrefs.SetFloat(BgmVolumeKey, _bgmVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, _sfxVolume);
            PlayerPrefs.Save();
        }
    }
}
