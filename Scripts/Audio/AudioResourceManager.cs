using UnityEngine;

namespace AudioSystem
{
    /// <summary>
    /// 音声リソースとアニメーションを管理するマネージャー
    /// AudioManagerと連携してSEを再生
    /// </summary>
    public class AudioResourceManager : MonoBehaviour
    {
        [Header("防御音效")]
        [SerializeField] private AudioClip guardSuccessSound;
        [SerializeField] private AudioClip parrySuccessSound;
        
        [Header("防御アニメーション")]
        [SerializeField] private AnimationClip guardSuccessAnimation;
        [SerializeField] private AnimationClip parrySuccessAnimation;
        
        [Header("アニメーション設定")]
        [SerializeField] private Animator playerAnimator;
        
        // AudioManagerの参照
        private AudioManager audioManager;
        
        private static AudioResourceManager instance;
        public static AudioResourceManager Instance => instance;
        
        private void Awake()
        {
            // シングルトンの設定
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            
            // AudioManagerの取得
            audioManager = AudioManager.Instance;
            
            // プレイヤーのAnimatorを自動取得
            if (!playerAnimator)
            {
                var player = FindFirstObjectByType<PlayerStats>();
                if (player)
                {
                    playerAnimator = player.GetComponent<Animator>();
                    Debug.Log("AudioResourceManager: プレイヤーのAnimatorを自動取得 - " + (playerAnimator != null));
                }
            }
            
            Debug.Log("AudioResourceManager: 初期化完了 - AudioManager: " + (audioManager != null));
        }
        
        /// <summary>
        /// ガード成功音を再生
        /// </summary>
        public void PlayGuardSuccessSound()
        {
            if (audioManager && guardSuccessSound)
            {
                audioManager.PlaySE(guardSuccessSound);
                Debug.Log("AudioResourceManager: ガード成功音を再生");
            }
            else
            {
                Debug.LogWarning("AudioResourceManager: ガード成功音が設定されていません - AudioManager: " + (audioManager != null) + ", GuardSound: " + (guardSuccessSound != null));
            }
        }
        
        /// <summary>
        /// パリィ成功音を再生
        /// </summary>
        public void PlayParrySuccessSound()
        {
            if (audioManager && parrySuccessSound)
            {
                audioManager.PlaySE(parrySuccessSound);
                Debug.Log("AudioResourceManager: パリィ成功音を再生");
            }
            else
            {
                Debug.LogWarning("AudioResourceManager: パリィ成功音が設定されていません - AudioManager: " + (audioManager != null) + ", ParrySound: " + (parrySuccessSound != null));
            }
        }
        
        /// <summary>
        /// ガード成功アニメーションを再生
        /// </summary>
        public void PlayGuardSuccessAnimation()
        {
            if (playerAnimator && guardSuccessAnimation)
            {
                // アニメーションを再生
                playerAnimator.Play(guardSuccessAnimation.name);
                Debug.Log("AudioResourceManager: ガード成功アニメーションを再生 - " + guardSuccessAnimation.name);
            }
            else
            {
                Debug.LogWarning("AudioResourceManager: ガード成功アニメーションが設定されていません - Animator: " + (playerAnimator != null) + ", Animation: " + (guardSuccessAnimation != null));
            }
        }
        
        /// <summary>
        /// パリィ成功アニメーションを再生
        /// </summary>
        public void PlayParrySuccessAnimation()
        {
            if (playerAnimator && parrySuccessAnimation)
            {
                // アニメーションを再生
                playerAnimator.Play(parrySuccessAnimation.name);
                Debug.Log("AudioResourceManager: パリィ成功アニメーションを再生 - " + parrySuccessAnimation.name);
            }
            else
            {
                Debug.LogWarning("AudioResourceManager: パリィ成功アニメーションが設定されていません - Animator: " + (playerAnimator != null) + ", Animation: " + (parrySuccessAnimation != null));
            }
        }
        
        /// <summary>
        /// ガード成功時の音とアニメーションを再生
        /// </summary>
        public void PlayGuardSuccess()
        {
            PlayGuardSuccessSound();
            PlayGuardSuccessAnimation();
        }
        
        /// <summary>
        /// パリィ成功時の音とアニメーションを再生
        /// </summary>
        public void PlayParrySuccess()
        {
            PlayParrySuccessSound();
            PlayParrySuccessAnimation();
        }
        
        /// <summary>
        /// テスト用：ガード成功音を再生
        /// </summary>
        [ContextMenu("ガード成功音テスト")]
        public void TestGuardSound()
        {
            Debug.Log("AudioResourceManager: ガード成功音テスト開始");
            PlayGuardSuccessSound();
        }
        
        /// <summary>
        /// テスト用：パリィ成功音を再生
        /// </summary>
        [ContextMenu("パリィ成功音テスト")]
        public void TestParrySound()
        {
            Debug.Log("AudioResourceManager: パリィ成功音テスト開始");
            PlayParrySuccessSound();
        }
        
        /// <summary>
        /// テスト用：AudioManagerの状態を確認
        /// </summary>
        [ContextMenu("AudioManager状態確認")]
        public void CheckAudioManagerStatus()
        {
            if (audioManager)
            {
                Debug.Log("AudioManager状態: BGM音量=" + audioManager.GetBGMVolume() + 
                         ", SE音量=" + audioManager.GetSEVolume() + 
                         ", 音ファイル数=" + (guardSuccessSound != null ? "1" : "0") + "/" + (parrySuccessSound != null ? "1" : "0"));
            }
            else
            {
                Debug.LogError("AudioManagerが存在しません！");
            }
        }
    }
}