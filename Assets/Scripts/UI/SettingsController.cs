using UnityEngine;
using UnityEngine.UI;

namespace Zzinmak.UI
{
    public class SettingsController : MonoBehaviour
    {
        private const string BgmVolumeKey = "BGMVolume";
        private const string SfxVolumeKey = "SFXVolume";

        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        private void Start()
        {
            float bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
            float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

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
        }

        public void Open()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void Close()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void OnBgmVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(BgmVolumeKey, value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
        }
    }
}
