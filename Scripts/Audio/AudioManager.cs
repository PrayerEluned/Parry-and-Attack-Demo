using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    /// <summary>
    /// 全体の音声管理を行うマネージャー
    /// BGMとSEの音量を一元管理し、設定を保存・読み込みする
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmAudioSource;
        [SerializeField] private AudioSource seAudioSource;
        
        [Header("BGM Clips")]
        [SerializeField] private AudioClip[] bgmClips;
        
        [Header("SE Clips")]
        [SerializeField] private AudioClip[] seClips;
        
        // シングルトンインスタンス
        private static AudioManager instance;
        public static AudioManager Instance => instance;
        
        // 音量設定
        private float bgmVolume = 1.0f;
        private float seVolume = 1.0f;
        
        // 設定キー
        private const string BGM_VOLUME_KEY = "BGMVolume";
        private const string SE_VOLUME_KEY = "SEVolume";
        
        // イベント
        public System.Action<float> OnBGMVolumeChanged;
        public System.Action<float> OnSEVolumeChanged;
        
        private void Awake()
        {
            // シングルトンの設定
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // AudioSourceの初期化
            InitializeAudioSources();
            
            // 保存された設定を読み込み
            LoadAudioSettings();
            
            Debug.Log("AudioManager: 初期化完了 - BGM音量: " + bgmVolume + ", SE音量: " + seVolume);
        }
        
        /// <summary>
        /// AudioSourceの初期化
        /// </summary>
        private void InitializeAudioSources()
        {
            // BGM用AudioSourceの設定
            if (!bgmAudioSource)
            {
                bgmAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            if (bgmAudioSource)
            {
                bgmAudioSource.playOnAwake = false;
                bgmAudioSource.loop = true;
                bgmAudioSource.outputAudioMixerGroup = GetBGMGroup();
            }
            
            // SE用AudioSourceの設定
            if (!seAudioSource)
            {
                seAudioSource = gameObject.AddComponent<AudioSource>();
            }
            
            if (seAudioSource)
            {
                seAudioSource.playOnAwake = false;
                seAudioSource.loop = false;
                seAudioSource.outputAudioMixerGroup = GetSEGroup();
            }
        }
        
        /// <summary>
        /// BGM用のAudioMixerGroupを取得
        /// </summary>
        private AudioMixerGroup GetBGMGroup()
        {
            if (audioMixer)
            {
                AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("BGM");
                return groups.Length > 0 ? groups[0] : null;
            }
            return null;
        }
        
        /// <summary>
        /// SE用のAudioMixerGroupを取得
        /// </summary>
        private AudioMixerGroup GetSEGroup()
        {
            if (audioMixer)
            {
                AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("SE");
                return groups.Length > 0 ? groups[0] : null;
            }
            return null;
        }
        
        /// <summary>
        /// 保存された音声設定を読み込み
        /// </summary>
        private void LoadAudioSettings()
        {
            bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1.0f);
            seVolume = PlayerPrefs.GetFloat(SE_VOLUME_KEY, 1.0f);
            
            ApplyBGMVolume(bgmVolume);
            ApplySEVolume(seVolume);
        }
        
        /// <summary>
        /// BGM音量を設定
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            ApplyBGMVolume(bgmVolume);
            PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
            PlayerPrefs.Save();
            
            OnBGMVolumeChanged?.Invoke(bgmVolume);
            Debug.Log("AudioManager: BGM音量を設定 - " + bgmVolume);
        }
        
        /// <summary>
        /// SE音量を設定
        /// </summary>
        public void SetSEVolume(float volume)
        {
            seVolume = Mathf.Clamp01(volume);
            ApplySEVolume(seVolume);
            PlayerPrefs.SetFloat(SE_VOLUME_KEY, seVolume);
            PlayerPrefs.Save();
            
            OnSEVolumeChanged?.Invoke(seVolume);
            Debug.Log("AudioManager: SE音量を設定 - " + seVolume);
        }
        
        /// <summary>
        /// BGM音量を適用
        /// </summary>
        private void ApplyBGMVolume(float volume)
        {
            if (bgmAudioSource)
            {
                bgmAudioSource.volume = volume;
            }
            
            if (audioMixer)
            {
                // AudioMixerを使用している場合
                float mixerVolume = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat("BGMVolume", mixerVolume);
            }
        }
        
        /// <summary>
        /// SE音量を適用
        /// </summary>
        private void ApplySEVolume(float volume)
        {
            if (seAudioSource)
            {
                seAudioSource.volume = volume;
            }
            
            if (audioMixer)
            {
                // AudioMixerを使用している場合
                float mixerVolume = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat("SEVolume", mixerVolume);
            }
        }
        
        /// <summary>
        /// BGMを再生
        /// </summary>
        public void PlayBGM(AudioClip clip)
        {
            if (bgmAudioSource && clip)
            {
                bgmAudioSource.clip = clip;
                bgmAudioSource.Play();
                Debug.Log("AudioManager: BGM再生開始 - " + clip.name);
            }
        }
        
        /// <summary>
        /// BGMを停止
        /// </summary>
        public void StopBGM()
        {
            if (bgmAudioSource)
            {
                bgmAudioSource.Stop();
                Debug.Log("AudioManager: BGM停止");
            }
        }
        
        /// <summary>
        /// SEを再生
        /// </summary>
        public void PlaySE(AudioClip clip)
        {
            if (seAudioSource && clip)
            {
                seAudioSource.PlayOneShot(clip);
                Debug.Log("AudioManager: SE再生 - " + clip.name);
            }
        }
        
        /// <summary>
        /// BGM音量を取得
        /// </summary>
        public float GetBGMVolume()
        {
            return bgmVolume;
        }
        
        /// <summary>
        /// SE音量を取得
        /// </summary>
        public float GetSEVolume()
        {
            return seVolume;
        }
        
        /// <summary>
        /// テスト用：BGM再生
        /// </summary>
        [ContextMenu("BGMテスト再生")]
        public void TestBGM()
        {
            if (bgmClips.Length > 0)
            {
                PlayBGM(bgmClips[0]);
            }
        }
        
        /// <summary>
        /// テスト用：SE再生
        /// </summary>
        [ContextMenu("SEテスト再生")]
        public void TestSE()
        {
            if (seClips.Length > 0)
            {
                PlaySE(seClips[0]);
            }
        }
    }
} 