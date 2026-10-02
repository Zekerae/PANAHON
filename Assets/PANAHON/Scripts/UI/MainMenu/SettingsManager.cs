using System;
using UnityEngine;
using UnityEngine.UI;
using Panahon.Networking;

namespace Panahon.UI.MainMenu
{
    public class SettingsManager : MonoBehaviour
    {
        [Header("Audio Controls")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle muteToggle;

        [Header("Graphics Controls")]
        [SerializeField] private Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Dropdown qualityDropdown;

        [Header("Photon Fusion Server Controls")]
        [SerializeField] private InputField photonAppIdInput;
        [SerializeField] private Dropdown photonRegionDropdown;
        [SerializeField] private Button testPhotonButton;
        [SerializeField] private Text photonStatusText;

        [Header("Navigation")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetDefaultsButton;

        private const string PREF_MASTER_VOL = "Setting_MasterVol";
        private const string PREF_MUSIC_VOL = "Setting_MusicVol";
        private const string PREF_SFX_VOL = "Setting_SFXVol";
        private const string PREF_MUTE = "Setting_Mute";
        private const string PREF_FULLSCREEN = "Setting_Fullscreen";
        private const string PREF_QUALITY = "Setting_Quality";

        private void Start()
        {
            LoadSettings();
            BindListeners();
        }

        private void BindListeners()
        {
            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            if (muteToggle != null) muteToggle.onValueChanged.AddListener(SetMute);

            if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(SetQualityLevel);

            if (testPhotonButton != null) testPhotonButton.onClick.AddListener(TestPhotonConnection);
            if (photonAppIdInput != null) photonAppIdInput.onEndEdit.AddListener(OnAppIdChanged);
            if (photonRegionDropdown != null) photonRegionDropdown.onValueChanged.AddListener(OnRegionChanged);

            if (closeButton != null) closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            if (resetDefaultsButton != null) resetDefaultsButton.onClick.AddListener(ResetToDefaults);
        }

        public void LoadSettings()
        {
            // Audio
            float master = PlayerPrefs.GetFloat(PREF_MASTER_VOL, 0.8f);
            float music = PlayerPrefs.GetFloat(PREF_MUSIC_VOL, 0.7f);
            float sfx = PlayerPrefs.GetFloat(PREF_SFX_VOL, 0.9f);
            bool isMuted = PlayerPrefs.GetInt(PREF_MUTE, 0) == 1;

            if (masterVolumeSlider != null) masterVolumeSlider.value = master;
            if (musicVolumeSlider != null) musicVolumeSlider.value = music;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfx;
            if (muteToggle != null) muteToggle.isOn = isMuted;

            AudioListener.volume = isMuted ? 0 : master;

            // Graphics
            bool fs = PlayerPrefs.GetInt(PREF_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
            if (fullscreenToggle != null) fullscreenToggle.isOn = fs;

            int quality = PlayerPrefs.GetInt(PREF_QUALITY, QualitySettings.GetQualityLevel());
            if (qualityDropdown != null) qualityDropdown.value = quality;

            // Photon Fusion Server
            if (NetworkManager.Instance != null && NetworkManager.Instance.Config != null)
            {
                var cfg = NetworkManager.Instance.Config;
                if (photonAppIdInput != null)
                {
                    photonAppIdInput.text = cfg.FusionAppId;
                }
                if (photonStatusText != null)
                {
                    photonStatusText.text = cfg.HasValidAppId 
                        ? $"<color=#50fa7b>Configured ({cfg.Region.ToUpper()}): {cfg.FusionAppId.Substring(0, Mathf.Min(8, cfg.FusionAppId.Length))}...</color>"
                        : "<color=#ffb86c>App ID Not Set (Offline Mode)</color>";
                }
            }
        }

        public void SetMasterVolume(float val)
        {
            PlayerPrefs.SetFloat(PREF_MASTER_VOL, val);
            if (muteToggle == null || !muteToggle.isOn)
            {
                AudioListener.volume = val;
            }
        }

        public void SetMusicVolume(float val)
        {
            PlayerPrefs.SetFloat(PREF_MUSIC_VOL, val);
        }

        public void SetSFXVolume(float val)
        {
            PlayerPrefs.SetFloat(PREF_SFX_VOL, val);
        }

        public void SetMute(bool isMuted)
        {
            PlayerPrefs.SetInt(PREF_MUTE, isMuted ? 1 : 0);
            AudioListener.volume = isMuted ? 0 : (masterVolumeSlider != null ? masterVolumeSlider.value : 0.8f);
        }

        public void SetFullscreen(bool fs)
        {
            Screen.fullScreen = fs;
            PlayerPrefs.SetInt(PREF_FULLSCREEN, fs ? 1 : 0);
        }

        public void SetQualityLevel(int index)
        {
            QualitySettings.SetQualityLevel(index);
            PlayerPrefs.SetInt(PREF_QUALITY, index);
        }

        private void OnAppIdChanged(string newAppId)
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.UpdateAppId(newAppId.Trim());
                if (photonStatusText != null)
                {
                    photonStatusText.text = $"App ID Saved: {newAppId.Substring(0, Mathf.Min(8, newAppId.Length))}...";
                }
            }
        }

        private void OnRegionChanged(int regionIndex)
        {
            string[] regions = { "asia", "us", "eu", "jp", "kr" };
            if (regionIndex >= 0 && regionIndex < regions.Length && NetworkManager.Instance != null)
            {
                NetworkManager.Instance.UpdateRegion(regions[regionIndex]);
            }
        }

        public void TestPhotonConnection()
        {
            if (photonStatusText != null)
            {
                photonStatusText.text = "<color=#8be9fd>Pinging Photon Fusion Cloud...</color>";
            }

            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.ConnectToPhotonCloud();
            }
        }

        public void ResetToDefaults()
        {
            PlayerPrefs.DeleteKey(PREF_MASTER_VOL);
            PlayerPrefs.DeleteKey(PREF_MUSIC_VOL);
            PlayerPrefs.DeleteKey(PREF_SFX_VOL);
            PlayerPrefs.DeleteKey(PREF_MUTE);
            PlayerPrefs.DeleteKey(PREF_FULLSCREEN);
            PlayerPrefs.DeleteKey(PREF_QUALITY);
            LoadSettings();
        }
    }
}
