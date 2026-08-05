using UnityEngine;
using UnityEngine.UI;

namespace Zzinmak.UI
{
    public class SettingsController : MonoBehaviour
    {
        private const string MasterVolumeKey = "MasterVolume";
        private const string BgmVolumeKey = "BGMVolume";
        private const string SfxVolumeKey = "SFXVolume";
        private const string BgmMutedKey = "BGMMuted";
        private const string SfxMutedKey = "SFXMuted";
        private const string BgmPreMuteVolumeKey = "BGMVolumePreMute";
        private const string SfxPreMuteVolumeKey = "SFXVolumePreMute";

        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle bgmMuteToggle;
        [SerializeField] private Toggle sfxMuteToggle;

        private void Start()
        {
            float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            float bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
            float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

            if (masterSlider != null)
            {
                masterSlider.value = masterVolume;
                masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (bgmSlider != null)
            {
                bgmSlider.value = bgmVolume;
                bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = sfxVolume;
                sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            bool bgmMuted = PlayerPrefs.GetInt(BgmMutedKey, 0) == 1;
            bool sfxMuted = PlayerPrefs.GetInt(SfxMutedKey, 0) == 1;

            if (bgmMuteToggle != null)
            {
                bgmMuteToggle.SetIsOnWithoutNotify(bgmMuted);
                bgmMuteToggle.onValueChanged.AddListener(OnBgmMuteToggleChanged);
            }

            if (sfxMuteToggle != null)
            {
                sfxMuteToggle.SetIsOnWithoutNotify(sfxMuted);
                sfxMuteToggle.onValueChanged.AddListener(OnSfxMuteToggleChanged);
            }
        }

        public void Open()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void Close()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void OnMasterVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
        }

        private void OnBgmVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(BgmVolumeKey, value);

            // 슬라이더를 직접 조작하면 음소거 체크를 해제한다.
            if (value > 0f && bgmMuteToggle != null && bgmMuteToggle.isOn)
            {
                PlayerPrefs.SetInt(BgmMutedKey, 0);
                bgmMuteToggle.SetIsOnWithoutNotify(false);
            }
        }

        private void OnSfxVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, value);

            if (value > 0f && sfxMuteToggle != null && sfxMuteToggle.isOn)
            {
                PlayerPrefs.SetInt(SfxMutedKey, 0);
                sfxMuteToggle.SetIsOnWithoutNotify(false);
            }
        }

        private void OnBgmMuteToggleChanged(bool muted)
        {
            if (muted)
            {
                float current = PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
                PlayerPrefs.SetFloat(BgmPreMuteVolumeKey, current);
                PlayerPrefs.SetFloat(BgmVolumeKey, 0f);
                if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(0f);
            }
            else
            {
                float restored = PlayerPrefs.GetFloat(BgmPreMuteVolumeKey, 1f);
                PlayerPrefs.SetFloat(BgmVolumeKey, restored);
                if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(restored);
            }

            PlayerPrefs.SetInt(BgmMutedKey, muted ? 1 : 0);
        }

        private void OnSfxMuteToggleChanged(bool muted)
        {
            if (muted)
            {
                float current = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
                PlayerPrefs.SetFloat(SfxPreMuteVolumeKey, current);
                PlayerPrefs.SetFloat(SfxVolumeKey, 0f);
                if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(0f);
            }
            else
            {
                float restored = PlayerPrefs.GetFloat(SfxPreMuteVolumeKey, 1f);
                PlayerPrefs.SetFloat(SfxVolumeKey, restored);
                if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(restored);
            }

            PlayerPrefs.SetInt(SfxMutedKey, muted ? 1 : 0);
        }
    }
}
