using DeadLetterOffice.Core;
using DeadLetterOffice.State;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeadLetterOffice.UI
{
    public class SettingsPanelUI : MonoBehaviour
    {
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private Button[] _tabButtons;
        [SerializeField] private GameObject[] _tabSections;
        [SerializeField] private int _defaultChapter = 1;
        [SerializeField] private int _defaultBeat;

        private float _playTime;

        private void Awake()
        {
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }

            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
            }

            SetupTabs();
        }

        private void OnEnable()
        {
            RefreshAudioSliders();
        }

        private void Update()
        {
            _playTime += Time.unscaledDeltaTime;
        }

        public void SetMasterVolume(float value)
        {
            if (ServiceLocator.TryGet(out AudioManager audioManager))
            {
                audioManager.SetMasterVolume(value);
            }
        }

        public void SetBgmVolume(float value)
        {
            if (ServiceLocator.TryGet(out AudioManager audioManager))
            {
                audioManager.SetBgmVolume(value);
            }
        }

        public void SetSfxVolume(float value)
        {
            if (ServiceLocator.TryGet(out AudioManager audioManager))
            {
                audioManager.SetSfxVolume(value);
            }
        }

        public void SaveGameplay()
        {
            if (!ServiceLocator.TryGet(out SaveManager saveManager))
            {
                SetStatus("저장 시스템을 찾을 수 없습니다.");
                return;
            }

            saveManager.Save(SceneManager.GetActiveScene().name, _defaultChapter, _defaultBeat, _playTime);
            SetStatus("게임 플레이 현황을 저장했습니다.");
        }

        public void LoadGameplay()
        {
            if (!ServiceLocator.TryGet(out SaveManager saveManager))
            {
                SetStatus("저장 시스템을 찾을 수 없습니다.");
                return;
            }

            if (saveManager.Load(out SaveData data))
            {
                SetStatus($"저장 데이터를 불러왔습니다. {data.currentScene}");
            }
            else
            {
                SetStatus("불러올 저장 데이터가 없습니다.");
            }
        }

        public void ResetGameplay()
        {
            if (!ServiceLocator.TryGet(out SaveManager saveManager))
            {
                SetStatus("저장 시스템을 찾을 수 없습니다.");
                return;
            }

            saveManager.ResetGame();
            _playTime = 0f;
            SetStatus("저장 데이터를 지우고 새로 시작할 준비를 했습니다.");
        }

        private void SetStatus(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
            }
        }

        private void RefreshAudioSliders()
        {
            if (!ServiceLocator.TryGet(out AudioManager audioManager))
            {
                return;
            }

            SetSliderWithoutNotify(_masterVolumeSlider, audioManager.MasterVolume);
            SetSliderWithoutNotify(_bgmVolumeSlider, audioManager.BgmVolume);
            SetSliderWithoutNotify(_sfxVolumeSlider, audioManager.SfxVolume);
        }

        private static void SetSliderWithoutNotify(Slider slider, float value)
        {
            if (slider != null)
            {
                slider.SetValueWithoutNotify(value);
            }
        }

        private void SetupTabs()
        {
            if (_tabButtons == null)
            {
                return;
            }

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                int index = i;
                Button button = _tabButtons[i];
                if (button != null)
                {
                    button.onClick.AddListener(() => ShowTab(index));
                }
            }

            if (_tabSections != null && _tabSections.Length > 0)
            {
                ShowTab(0);
            }
            else
            {
                UpdateTabVisuals(0);
            }
        }

        public void ShowTab(int index)
        {
            UpdateTabVisuals(index);

            if (_tabSections == null)
            {
                return;
            }

            for (int i = 0; i < _tabSections.Length; i++)
            {
                if (_tabSections[i] != null)
                {
                    _tabSections[i].SetActive(i == index);
                }
            }
        }

        private void UpdateTabVisuals(int selectedIndex)
        {
            if (_tabButtons == null)
            {
                return;
            }

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                Button button = _tabButtons[i];
                if (button == null || button.targetGraphic == null)
                {
                    continue;
                }

                button.targetGraphic.color = i == selectedIndex
                    ? new Color(1f, 1f, 1f, 0.2f)
                    : new Color(1f, 1f, 1f, 0.04f);
            }
        }
    }
}
