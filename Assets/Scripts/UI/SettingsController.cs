using UnityEngine;
using UnityEngine.UI;
using Zzinmak.Core;

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

        // 체크마크 그래픽만으로는 체크 여부가 잘 안 보여서, 체크박스 배경 자체도
        // 켜짐/꺼짐에 따라 흰색과 이 색 사이를 오간다 (메뉴 버튼과 같은 톤).
        private static readonly Color MuteCheckedColor = new Color(0.6045301f, 0.8962264f, 0.6931695f, 1f);

        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle bgmMuteToggle;
        [SerializeField] private Toggle sfxMuteToggle;
        [SerializeField] private Image bgmMuteCheckbox;
        [SerializeField] private Image sfxMuteCheckbox;

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
            SetMuteCheckboxVisual(bgmMuteCheckbox, bgmMuted);

            if (sfxMuteToggle != null)
            {
                sfxMuteToggle.SetIsOnWithoutNotify(sfxMuted);
                sfxMuteToggle.onValueChanged.AddListener(OnSfxMuteToggleChanged);
            }
            SetMuteCheckboxVisual(sfxMuteCheckbox, sfxMuted);
        }

        private static void SetMuteCheckboxVisual(Image checkbox, bool muted)
        {
            if (checkbox != null) checkbox.color = muted ? MuteCheckedColor : Color.white;
        }

        public void Open()
        {
            if (UiClickSound.Instance != null) UiClickSound.Instance.Play();
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void Close()
        {
            if (UiClickSound.Instance != null) UiClickSound.Instance.Play();
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
                SetMuteCheckboxVisual(bgmMuteCheckbox, false);
            }

            GameEvents.RaiseBgmVolumeChanged(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, value);

            if (value > 0f && sfxMuteToggle != null && sfxMuteToggle.isOn)
            {
                PlayerPrefs.SetInt(SfxMutedKey, 0);
                sfxMuteToggle.SetIsOnWithoutNotify(false);
                SetMuteCheckboxVisual(sfxMuteCheckbox, false);
            }

            GameEvents.RaiseSfxVolumeChanged(value);
        }

        private void OnBgmMuteToggleChanged(bool muted)
        {
            SetMuteCheckboxVisual(bgmMuteCheckbox, muted);

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
            SetMuteCheckboxVisual(sfxMuteCheckbox, muted);

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
