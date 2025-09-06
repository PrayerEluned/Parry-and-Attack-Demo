using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AudioSystem
{
    /// <summary>
    /// オプションメニューを管理するクラス
    /// 音量調整とメニューの開閉機能を提供
    /// </summary>
    public class OptionsMenu : MonoBehaviour
    {
        [Header("メニューUI")]
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private Button openOptionsButton;
        [SerializeField] private Button closeOptionsButton;
        
        [Header("音量調整スライダー")]
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider seVolumeSlider;
        
        [Header("音量表示テキスト")]
        [SerializeField] private TextMeshProUGUI bgmVolumeText;
        [SerializeField] private TextMeshProUGUI seVolumeText;
        
        private AudioManager audioManager;
        
        private void Awake()
        {
            audioManager = AudioManager.Instance;
            
            // ボタンのリスナーを設定
            if (openOptionsButton != null)
                openOptionsButton.onClick.AddListener(OpenOptionsMenu);
            
            if (closeOptionsButton != null)
                closeOptionsButton.onClick.AddListener(CloseOptionsMenu);
            
            // スライダーのリスナーを設定
            SetupSliderListeners();
        }
        
        private void SetupSliderListeners()
        {
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.onValueChanged.RemoveAllListeners();
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeSliderChanged);
                Debug.Log("OptionsMenu: BGMスライダーのリスナーを設定しました");
            }
            else
            {
                Debug.LogWarning("OptionsMenu: BGMスライダーが見つかりません");
            }
            
            if (seVolumeSlider != null)
            {
                seVolumeSlider.onValueChanged.RemoveAllListeners();
                seVolumeSlider.onValueChanged.AddListener(OnSEVolumeSliderChanged);
                Debug.Log("OptionsMenu: SEスライダーのリスナーを設定しました");
            }
            else
            {
                Debug.LogWarning("OptionsMenu: SEスライダーが見つかりません");
            }
        }
        
        private void Start()
        {
            // 初期値を設定
            LoadVolumeSettings();
            
            // オプションメニューを非表示にする
            if (optionsPanel != null)
                optionsPanel.SetActive(false);
        }
        
        public void OpenOptionsMenu()
        {
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(true);
                
                // UI要素の再設定
                SetupSliderListeners();
                LoadVolumeSettings();
                
                Debug.Log("OptionsMenu: オプションメニューを開きました");
            }
        }
        
        public void CloseOptionsMenu()
        {
            if (optionsPanel != null)
                optionsPanel.SetActive(false);
        }
        
        private void LoadVolumeSettings()
        {
            // BGM音量を設定
            float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.value = bgmVolume;
                UpdateBGMVolumeText(bgmVolume);
                Debug.Log($"OptionsMenu: BGM音量を設定しました - {bgmVolume}");
            }
            
            // SE音量を設定
            float seVolume = PlayerPrefs.GetFloat("SEVolume", 1f);
            if (seVolumeSlider != null)
            {
                seVolumeSlider.value = seVolume;
                UpdateSEVolumeText(seVolume);
                Debug.Log($"OptionsMenu: SE音量を設定しました - {seVolume}");
            }
        }
        
        private void OnBGMVolumeSliderChanged(float value)
        {
            Debug.Log($"OptionsMenu: BGMスライダーが変更されました - {value}");
            
            if (audioManager != null)
            {
                audioManager.SetBGMVolume(value);
                UpdateBGMVolumeText(value);
            }
        }
        
        private void OnSEVolumeSliderChanged(float value)
        {
            Debug.Log($"OptionsMenu: SEスライダーが変更されました - {value}");
            
            if (audioManager != null)
            {
                audioManager.SetSEVolume(value);
                UpdateSEVolumeText(value);
            }
        }
        
        private void UpdateBGMVolumeText(float volume)
        {
            if (bgmVolumeText != null)
            {
                int volumePercent = Mathf.RoundToInt(volume * 100);
                bgmVolumeText.text = $"BGM: {volumePercent}%";
                Debug.Log($"OptionsMenu: BGMテキストを更新しました - {volumePercent}%");
            }
        }
        
        private void UpdateSEVolumeText(float volume)
        {
            if (seVolumeText != null)
            {
                int volumePercent = Mathf.RoundToInt(volume * 100);
                seVolumeText.text = $"SE: {volumePercent}%";
                Debug.Log($"OptionsMenu: SEテキストを更新しました - {volumePercent}%");
            }
        }
    }
} 