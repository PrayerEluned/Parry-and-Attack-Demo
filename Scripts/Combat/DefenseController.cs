using UnityEngine;
using UnityEngine.Events;
using AudioSystem;

/// <summary>
/// 防御システムのコントローラー（安定設計版）
/// </summary>
public class DefenseController : MonoBehaviour
{
    [Header("防御設定")]
    [Tooltip("防御時のダメージ軽減率（0=完全無効、0.75=75%軽減、1=軽減なし）")]
    [SerializeField] private float defenseReduction = 0.75f; // 防御時のダメージ軽減率（75%オフ）
    
    [Header("パリィ設定")]
    [Tooltip("パリィ可能時間（秒）- 短すぎると失敗率が高くなる")]
    [SerializeField] private float parryWindowTime = 0.13f; // パリィ可能時間（約0.13秒）
    
    [Tooltip("パリィクールダウン時間（秒）（連打防止）")]
    [SerializeField] private float parryCooldownTime = 0.25f; // パリィクールダウン時間（約0.25秒）
    
    [Tooltip("防御クールダウン時間（秒）（短すぎると連続防御スパム可能）")]
    [SerializeField] private float defenseCooldownTime = 0.13f; // 防御クールダウン時間（約0.13秒）
    
    [Header("パリィ効果")]
    [SerializeField] private float parryDamageMultiplier = 1.2f; // パリィ成功時のダメージ倍率
    [SerializeField] private float parryHpPercentDamage = 0.007f; // 敵の最大HP7%を追加ダメージ
    
    [Header("音效設定")]
    [SerializeField] private AudioClip guardSuccessSound; // ガード成功時の音
    [SerializeField] private AudioClip parrySuccessSound; // パリィ成功時の音
    
    [Header("アニメーション設定")]
    [SerializeField] private Animator playerAnimator; // プレイヤーアニメーター
    [SerializeField] private Animator parryEffectAnimator; // パリィエフェクト専用アニメーター
    
    [Header("エフェクト位置設定")]
    [Tooltip("右向き時のエフェクトオフセット")]
    [SerializeField] private Vector2 effectOffsetRight = new Vector2(1.0f, 0.0f); // 右向きオフセット
    
    [Tooltip("左向き時のエフェクトオフセット")]
    [SerializeField] private Vector2 effectOffsetLeft = new Vector2(-1.0f, 0.0f); // 左向きオフセット
    
    [Tooltip("上向き時のエフェクトオフセット")]
    [SerializeField] private Vector2 effectOffsetUp = new Vector2(0.0f, 0.5f); // 上向きオフセット
    
    [Tooltip("下向き時のエフェクトオフセット")]
    [SerializeField] private Vector2 effectOffsetDown = new Vector2(0.0f, -0.5f); // 下向きオフセット
    
    [Header("エフェクト色設定")]
    [Tooltip("ブロック成功時のエフェクト色")]
    [SerializeField] private Color blockSuccessColor = new Color(0.5f, 0.7f, 1.0f, 1.0f); // 青みがかった色
    
    [Header("エフェクト高速化設定")]
    [Tooltip("エフェクトの色リセット時間（秒）")]
    [SerializeField] private float effectColorResetTime = 0.2f; // 色リセット時間
    
    [Tooltip("エフェクトの即座実行を有効にする")]
    [SerializeField] private bool enableInstantEffect = true; // 即座実行フラグ
    
    [Header("イベント")]
    public UnityEvent OnDefenseStart;
    public UnityEvent OnDefenseEnd;
    public UnityEvent OnParrySuccess;
    public UnityEvent<float> OnDamageReduced; // 軽減されたダメージ量
    
    // 防御状態
    public enum DefenseState
    {
        Idle,           // 待機中
        Defending,      // 防御中（パリィボタン待機）
        ParryWindow,    // パリィウィンドウ中（パリィボタン押下後）
        ParryCooldown,  // パリィクールダウン中
        DefenseCooldown // 防御クールダウン中
    }
    
    // 内部状態
    private DefenseState currentState = DefenseState.Idle;
    private bool hasParryBonus = false; // 次の攻撃にパリィボーナスがあるか
    private float pendingDamage = 0f; // 保留中のダメージ
    private EnemyHealth lastAttacker; // パリィ成功時の対象敵
    
    // 時間管理（フレーム管理から変更）
    private float defenseStartTime = -1f;
    private float parryStartTime = -1f; // パリィ開始時間
    private float cooldownEndTime = -1f;
    
    // 入力状態（安定版）
    private bool isDefenseActive = false; // 防御がアクティブかどうか
    private bool defenseButtonPressed = false; // 防御ボタンが押されているか
    
    // 参照
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private BasicAttackController attackController;
    [SerializeField] private CharacterMovement playerMovement; // プレイヤーの移動コンポーネント（自動取得されます）
    private AudioSystem.AudioManager audioManager;
    
    // プロパティ
    public bool IsDefending => currentState == DefenseState.Defending;
    public bool IsInParryWindow => currentState == DefenseState.ParryWindow;
    public bool IsInDefenseCooldown => currentState == DefenseState.DefenseCooldown;
    public bool IsInParryCooldown => currentState == DefenseState.ParryCooldown;
    public bool HasParryBonus => hasParryBonus;
    public bool IsDefenseActive => currentState != DefenseState.Idle;
    public DefenseState CurrentState => currentState;
    
    private void Awake()
    {
        // AudioManagerの取得
        audioManager = AudioManager.Instance;
        
        // PlayerMovementの自動取得
        if (playerMovement == null)
        {
            playerMovement = GetComponent<CharacterMovement>();
            if (playerMovement == null)
            {
                playerMovement = GetComponentInParent<CharacterMovement>();
            }
        }
        
        // 初期状態を設定
        SetState(DefenseState.Idle);
    }
    
    private void Start()
    {
        // AudioManagerが取得できていない場合は再試行
        if (audioManager == null)
        {
            audioManager = AudioManager.Instance;
            Debug.Log($"DefenseController: AudioManager再取得 = {audioManager}");
        }
    }
    
    private void Update()
    {
        // 時間管理を更新
        UpdateTimeManagement();
        
        // 入力状態を更新
        UpdateInputState();
        
        // 防御状態を更新
        UpdateDefenseState();
        
        // 防御入力処理
        HandleDefenseInput();
    }
    
    private void UpdateTimeManagement()
    {
        // 現在のフレームを取得
        int currentFrame = Time.frameCount;
        
        // 防御クールダウン終了チェック
        if (cooldownEndTime != -1f && Time.time >= cooldownEndTime)
        {
            SetState(DefenseState.Idle);
            cooldownEndTime = -1f;
        }
        
        // パリィウィンドウ終了チェック
        if (currentState == DefenseState.ParryWindow && 
            parryStartTime != -1f && Time.time >= parryStartTime + parryWindowTime) // フレームから秒に変換
        {
            EndParryWindow();
        }
    }
    
    private void UpdateInputState()
    {
        // 現在の入力状態を取得（デバッグ用）
        isDefenseActive = Input.GetMouseButton(1) || Input.GetKey(KeyCode.L);
        defenseButtonPressed = Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.L);
    }
    
    private void UpdateDefenseState()
    {
        // クールダウン終了チェック
        if (cooldownEndTime != -1f && Time.time >= cooldownEndTime)
        {
            SetState(DefenseState.Idle);
            cooldownEndTime = -1f;
        }
        
        // パリィウィンドウ終了チェック
        if (currentState == DefenseState.ParryWindow && 
            parryStartTime != -1f && Time.time >= parryStartTime + parryWindowTime) // フレームから秒に変換
        {
            EndParryWindow();
        }
    }
    
    private void HandleDefenseInput()
    {
        // 防御開始（右クリックまたはLキーの押下瞬間）
        if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.L)) && 
            currentState == DefenseState.Idle)
        {
            Debug.Log($"防御開始: {currentState} -> Defending");
            StartDefense();
        }
        
        // 防御終了（右クリックまたはLキーの離し瞬間）
        if ((Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.L)) && 
            currentState == DefenseState.Defending)
        {
            Debug.Log($"防御終了: {currentState} -> DefenseCooldown");
            EndDefense();
        }
        
        // パリィ入力（スペースキー）
        if (Input.GetKeyDown(KeyCode.Space) && 
            (currentState == DefenseState.Defending || currentState == DefenseState.Idle))
        {
            Debug.Log($"パリィ入力: {currentState} -> ParryWindow");
            HandleParryInput();
        }
    }
    
    public void HandleParryInput()
    {
        if (currentState == DefenseState.ParryCooldown)
        {
            return; // クールダウン中は無視
        }
        
        if (currentState == DefenseState.Defending)
        {
            // 防御中にパリィボタンを押した場合、パリィウィンドウを開始
            SetState(DefenseState.ParryWindow);
            parryStartTime = Time.time; // フレームから時間に変換
        }
        else if (currentState == DefenseState.Idle)
        {
            // 待機中にパリィボタンを押した場合、直接パリィウィンドウを開始
            SetState(DefenseState.ParryWindow);
            parryStartTime = Time.time; // フレームから時間に変換
        }
    }
    
    public void StartDefense()
    {
        if (currentState == DefenseState.Idle)
        {
            SetState(DefenseState.Defending);
            defenseStartTime = Time.time; // フレームから時間に変換
            OnDefenseStart?.Invoke();
        }
    }
    
    public void EndDefense()
    {
        if (currentState == DefenseState.Defending)
        {
            // 防御クールダウンを開始
            SetState(DefenseState.DefenseCooldown);
            cooldownEndTime = Time.time + defenseCooldownTime; // フレームから秒に変換
            OnDefenseEnd?.Invoke();
        }
    }
    
    private void EndParryWindow()
    {
        if (currentState == DefenseState.ParryWindow)
        {
            // パリィ失敗時は短いクールダウンを設定（連打防止）
            SetState(DefenseState.ParryCooldown);
            cooldownEndTime = Time.time + (parryCooldownTime * 0.5f); // 通常の半分のクールダウン
            
            Debug.Log($"DefenseController: パリィウィンドウ終了 - 短いクールダウン開始: {parryCooldownTime * 0.5f}秒");
        }
    }
    
    public float ProcessIncomingDamage(float originalDamage, EnemyHealth attacker = null)
    {
        if (currentState == DefenseState.Idle)
        {
            return originalDamage; // 防御していない場合は全ダメージ
        }
        
        if (currentState == DefenseState.ParryWindow)
        {
            // パリィ成功！
            TriggerParrySuccess();
            lastAttacker = attacker;
            return 0f; // パリィ成功時はダメージ0
        }
        
        if (currentState == DefenseState.Defending)
        {
            // 通常防御（ダメージ軽減）
            float reducedDamage = originalDamage * defenseReduction;
            float damageReduced = originalDamage - reducedDamage;
            
            // カウンターアタック効果を適用
            Debug.Log($"[カウンターアタック] 防御中 - attacker={attacker}, playerStats={playerStats}");
            if (attacker != null)
            {
                var specialEffect = playerStats.GetComponent<SpecialPatchEffect>();
                Debug.Log($"[カウンターアタック] SpecialPatchEffect={specialEffect}");
                
                if (specialEffect != null)
                {
                    Debug.Log($"[カウンターアタック] 効果タイプ確認 - effectType={specialEffect.effectType}, patchLevel={specialEffect.patchLevel}");
                    
                    // 効果タイプがCounterAttackかチェック
                    if (specialEffect.effectType == SpecialEffectType.CounterAttack)
                    {
                        Debug.Log($"[カウンターアタック] カウンターアタック効果を実行します");
                        specialEffect.OnDefendCounter(attacker);
                    }
                    else
                    {
                        Debug.LogWarning($"[カウンターアタック] 効果タイプが違います: {specialEffect.effectType}");
                    }
                }
                else
                {
                    Debug.LogWarning("[カウンターアタック] SpecialPatchEffectが見つかりません！");
                    
                    // 子オブジェクトからも探してみる
                    specialEffect = playerStats.GetComponentInChildren<SpecialPatchEffect>();
                    Debug.Log($"[カウンターアタック] 子オブジェクトから検索: {specialEffect}");
                    if (specialEffect != null)
                    {
                        Debug.Log($"[カウンターアタック] 子オブジェクトで発見 - effectType={specialEffect.effectType}");
                        if (specialEffect.effectType == SpecialEffectType.CounterAttack)
                        {
                            Debug.Log($"[カウンターアタック] 子オブジェクトからカウンターアタック効果を実行します");
                            specialEffect.OnDefendCounter(attacker);
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("[カウンターアタック] attackerがnullです");
            }
            
            // ガード成功エフェクト
            PlayGuardSuccess();
            
            OnDamageReduced?.Invoke(damageReduced);
            return reducedDamage;
        }
        
        return originalDamage; // その他の状態では全ダメージ
    }
    
    private void TriggerParrySuccess()
    {
        hasParryBonus = true;
        OnParrySuccess?.Invoke();

        // 特殊効果パッチによるパリィダメージ効果を適用
        if (lastAttacker != null)
        {
            var specialEffect = playerStats.GetComponent<SpecialPatchEffect>();
            if (specialEffect != null)
            {
                specialEffect.OnParrySuccess(lastAttacker);
            }
        }

        // ★ここで必ずSEを鳴らす
        PlayParrySuccess();

        // パリィ成功時はクールダウンを0にする（即座に次のパリィが可能）
        SetState(DefenseState.Idle);
        cooldownEndTime = -1f; // クールダウンを即座に終了
        Debug.Log("DefenseController: パリィ成功 - クールダウンを0にリセット");
    }
    
    public float CalculateParryBonusDamage(float baseDamage, EnemyHealth target)
    {
        if (!hasParryBonus) return baseDamage;
        
        float bonusDamage = baseDamage * parryDamageMultiplier;
        
        if (target != null)
        {
            // 敵の最大HPの0.7%を追加
            float enemyMaxHp = target.GetComponent<EnemyStats>()?.TotalHP ?? 100f;
            float hpPercentDamage = enemyMaxHp * parryHpPercentDamage;
            bonusDamage += hpPercentDamage;
            
            Debug.Log($"DefenseController: パリィボーナス計算 - 基本ダメージ: {baseDamage}, 倍率: {parryDamageMultiplier}, HP%ダメージ: {hpPercentDamage}, 合計: {bonusDamage}");
        }
        
        hasParryBonus = false; // ボーナスを使用
        Debug.Log($"DefenseController: パリィボーナス使用 - 最終ダメージ: {bonusDamage}");
        return bonusDamage;
    }
    
    private void SetState(DefenseState newState)
    {
        DefenseState oldState = currentState;
        currentState = newState;
        
        // デバッグログ（状態変更時のみ）
        if (oldState != newState)
        {
            Debug.Log($"DefenseController: 状態変更 {oldState} -> {newState}");
        }
        
        // SpecialPatchEffectの防御状態を更新
        var specialEffect = playerStats?.GetComponent<SpecialPatchEffect>();
        if (specialEffect != null)
        {
            bool isDefending = (newState == DefenseState.Defending);
            specialEffect.SetDefendingState(isDefending);
        }
        
        // 注: プレイヤーのメインアニメーターは使用せず、
        // パリィエフェクト専用アニメーター（parryEffectAnimator）で
        // トリガー管理（PlayParry）を使用しています
    }
    
    private void PlayGuardSuccessSound()
    {
        Debug.Log("【SE】防御成功SE再生開始");
        
        // AudioManagerがnullの場合は再取得を試行
        if (audioManager == null)
        {
            audioManager = AudioManager.Instance;
            Debug.Log($"DefenseController: AudioManager再取得 = {audioManager}");
        }
        
        Debug.Log($"PlayGuardSuccessSound: guardSuccessSound={guardSuccessSound}, audioManager={audioManager}");
        if (guardSuccessSound != null && audioManager != null)
        {
            audioManager.PlaySE(guardSuccessSound);
            Debug.Log("【SE】防御成功SE再生完了");
        }
        else
        {
            Debug.LogWarning($"【SE】防御成功SE再生失敗: guardSuccessSound={guardSuccessSound}, audioManager={audioManager}");
        }
    }
    
    private void PlayParrySuccessSound()
    {
        Debug.Log("【SE】パリィ成功SE再生開始");
        
        // AudioManagerがnullの場合は再取得を試行
        if (audioManager == null)
        {
            audioManager = AudioManager.Instance;
            Debug.Log($"DefenseController: AudioManager再取得 = {audioManager}");
        }
        
        Debug.Log($"PlayParrySuccessSound: parrySuccessSound={parrySuccessSound}, audioManager={audioManager}");
        if (parrySuccessSound != null && audioManager != null)
        {
            audioManager.PlaySE(parrySuccessSound);
            Debug.Log("【SE】パリィ成功SE再生完了");
        }
        else
        {
            Debug.LogWarning($"【SE】パリィ成功SE再生失敗: parrySuccessSound={parrySuccessSound}, audioManager={audioManager}");
        }
    }
    
    private void PlayGuardSuccessAnimation()
    {
        Debug.Log("=== ガードアニメーション再生開始 ===");
        
        if (parryEffectAnimator == null)
        {
            Debug.LogError("ParryEffectAnimatorが見つかりません！");
            return;
        }
        
        try
        {
            Debug.Log($"ParryEffectAnimator有効: {parryEffectAnimator.enabled}");
            Debug.Log($"現在の状態: {parryEffectAnimator.GetCurrentAnimatorStateInfo(0).IsName("Parry")}");
            
            // エフェクトの位置を調整
            AdjustEffectPosition();
            
            // ブロック成功時の青色エフェクトを適用
            ApplyBlockSuccessColor();
            
            // アニメーターを確実に有効化
            parryEffectAnimator.enabled = true;
            
            // 即座実行が有効な場合、アニメーターを即座に更新
            if (enableInstantEffect)
            {
                parryEffectAnimator.Update(0f);
            }
            
            // トリガーを確実に実行
            parryEffectAnimator.ResetTrigger("PlayParry");
            parryEffectAnimator.SetTrigger("PlayParry");
            
            // 即座実行が有効な場合、再度アニメーションを更新
            if (enableInstantEffect)
            {
                parryEffectAnimator.Update(0f);
            }
            
            Debug.Log("ガードアニメーション再生完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ガードアニメーション再生エラー: {e.Message}");
        }
    }
    
    private void PlayParrySuccessAnimation()
    {
        Debug.Log("=== パリィアニメーション再生開始 ===");
        
        if (parryEffectAnimator == null)
        {
            Debug.LogError("ParryEffectAnimatorが見つかりません！");
            return;
        }
        
        try
        {
            Debug.Log($"ParryEffectAnimator有効: {parryEffectAnimator.enabled}");
            Debug.Log($"現在の状態: {parryEffectAnimator.GetCurrentAnimatorStateInfo(0).IsName("Parry")}");
            
            // エフェクトの位置を調整
            AdjustEffectPosition();
            
            // アニメーターを確実に有効化
            parryEffectAnimator.enabled = true;
            
            // 即座実行が有効な場合、アニメーターを即座に更新
            if (enableInstantEffect)
            {
                parryEffectAnimator.Update(0f);
            }
            
            // トリガーを確実に実行
            parryEffectAnimator.ResetTrigger("PlayParry");
            parryEffectAnimator.SetTrigger("PlayParry");
            
            // 即座実行が有効な場合、再度アニメーションを更新
            if (enableInstantEffect)
            {
                parryEffectAnimator.Update(0f);
            }
            
            Debug.Log("パリィアニメーション再生完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"パリィアニメーション再生エラー: {e.Message}");
        }
    }
    
    /// <summary>
    /// プレイヤーの向きに応じてエフェクトの位置を調整
    /// </summary>
    private void AdjustEffectPosition()
    {
        // Nullチェックを強化
        if (parryEffectAnimator == null)
        {
            Debug.LogWarning("エフェクト位置調整: ParryEffectAnimatorが設定されていません");
            return;
        }
        
        if (playerMovement == null)
        {
            Debug.LogWarning("エフェクト位置調整: CharacterMovementが設定されていません");
            return;
        }
        
        // プレイヤーの向きを取得
        Vector2 playerDirection = playerMovement.LastMoveDirection;
        Debug.Log($"プレイヤー方向: ({playerDirection.x:F3}, {playerDirection.y:F3})");
        
        // エフェクトの位置を計算
        Vector3 effectOffset = Vector3.zero;
        
        // 方向判定とオフセット適用
        if (Mathf.Abs(playerDirection.x) > Mathf.Abs(playerDirection.y))
        {
            // 左右方向が優勢
            Debug.Log("左右方向が優勢");
            if (playerDirection.x > 0.1f) // 右向き
            {
                effectOffset = effectOffsetRight;
                Debug.Log($"右向きオフセット適用: {effectOffsetRight}");
            }
            else if (playerDirection.x < -0.1f) // 左向き
            {
                effectOffset = effectOffsetLeft;
                Debug.Log($"左向きオフセット適用: {effectOffsetLeft}");
            }
            else
            {
                Debug.Log("左右方向の閾値未満");
            }
        }
        else
        {
            // 上下方向が優勢
            Debug.Log("上下方向が優勢");
            if (playerDirection.y > 0.1f) // 上向き
            {
                effectOffset = effectOffsetUp;
                Debug.Log($"上向きオフセット適用: {effectOffsetUp}");
            }
            else if (playerDirection.y < -0.1f) // 下向き
            {
                effectOffset = effectOffsetDown;
                Debug.Log($"下向きオフセット適用: {effectOffsetDown}");
            }
            else
            {
                Debug.Log("上下方向の閾値未満");
            }
        }
        
        // プレイヤーの位置にオフセットを加算
        Vector3 playerPosition = transform.position;
        Vector3 newEffectPosition = playerPosition + effectOffset;
        
        // TransformのNullチェック
        if (parryEffectAnimator.transform != null)
        {
            parryEffectAnimator.transform.position = newEffectPosition;
            Debug.Log($"エフェクト位置調整完了: プレイヤー位置({playerPosition.x:F2}, {playerPosition.y:F2}) + オフセット({effectOffset.x:F2}, {effectOffset.y:F2}) = エフェクト位置({newEffectPosition.x:F2}, {newEffectPosition.y:F2})");
        }
        else
        {
            Debug.LogError("エフェクト位置調整: Transformが見つかりません");
        }
    }
    
    /// <summary>
    /// ブロック成功時の青色エフェクトを適用
    /// </summary>
    private void ApplyBlockSuccessColor()
    {
        if (parryEffectAnimator == null)
        {
            return;
        }
        
        SpriteRenderer spriteRenderer = parryEffectAnimator.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 青色エフェクトを適用
            spriteRenderer.color = blockSuccessColor;
            
            // 一定時間後に色をリセット
            StartCoroutine(ResetEffectColor(spriteRenderer));
            
            Debug.Log($"ブロック成功エフェクト色適用: {blockSuccessColor}");
        }
    }
    
    /// <summary>
    /// エフェクトの色をリセット
    /// </summary>
    private System.Collections.IEnumerator ResetEffectColor(SpriteRenderer spriteRenderer)
    {
        yield return new WaitForSeconds(effectColorResetTime); // 設定された時間後にリセット
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            Debug.Log("エフェクト色をリセットしました");
        }
    }
    
    // リセット処理を削除（SetTriggerは自動でリセットされる）
    
    private void PlayGuardSuccess()
    {
        PlayGuardSuccessSound();
        PlayGuardSuccessAnimation();
    }
    
    private void PlayParrySuccess()
    {
        PlayParrySuccessSound();
        PlayParrySuccessAnimation();
    }
    
    [ContextMenu("エフェクト位置設定確認")]
    public void DebugEffectPositionSettings()
    {
        Debug.Log("=== エフェクト位置設定 ===");
        Debug.Log($"右向きオフセット: {effectOffsetRight}");
        Debug.Log($"左向きオフセット: {effectOffsetLeft}");
        Debug.Log($"上向きオフセット: {effectOffsetUp}");
        Debug.Log($"下向きオフセット: {effectOffsetDown}");
        
        if (parryEffectAnimator != null)
        {
            Debug.Log($"現在のエフェクト位置: {parryEffectAnimator.transform.position}");
        }
        
        if (playerMovement != null)
        {
            Debug.Log($"プレイヤー方向: {playerMovement.LastMoveDirection}");
        }
        
        Debug.Log($"プレイヤー位置: {transform.position}");
        Debug.Log("========================");
    }
    
    [ContextMenu("エフェクト位置強制調整")]
    public void ForceAdjustEffectPosition()
    {
        Debug.Log("=== エフェクト位置強制調整 ===");
        AdjustEffectPosition();
    }
    
    [ContextMenu("現在の防御状態確認")]
    public void DebugCurrentDefenseState()
    {
        Debug.Log("=== 現在の防御状態 ===");
        Debug.Log($"現在の状態: {currentState}");
        Debug.Log($"防御ボタン押下中: {isDefenseActive}");
        Debug.Log($"防御ボタン押下瞬間: {defenseButtonPressed}");
        Debug.Log($"パリィボーナス有無: {hasParryBonus}");
        Debug.Log($"防御開始時間: {defenseStartTime}");
        Debug.Log($"パリィ開始時間: {parryStartTime}");
        Debug.Log($"クールダウン終了時間: {cooldownEndTime}");
        Debug.Log($"現在時間: {Time.time}");
        Debug.Log("=====================");
    }
    
    [ContextMenu("防御状態リセット")]
    public void ResetDefenseState()
    {
        SetState(DefenseState.Idle);
        hasParryBonus = false;
        pendingDamage = 0f;
        lastAttacker = null;
        defenseStartTime = -1f;
        parryStartTime = -1f;
        cooldownEndTime = -1f;
    }
    
    [ContextMenu("パリィエフェクト情報確認")]
    public void DebugParryEffectInfo()
    {
        if (parryEffectAnimator == null)
        {
            Debug.LogError("ParryEffectAnimatorが設定されていません！");
            return;
        }
        
        Debug.Log("=== パリィエフェクト詳細情報 ===");
        
        // Animator情報
        Debug.Log($"Animator有効: {parryEffectAnimator.enabled}");
        Debug.Log($"Animator状態: {parryEffectAnimator.GetCurrentAnimatorStateInfo(0).IsName("Parry")}");
        Debug.Log($"現在の状態名: {(parryEffectAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") ? "Idle" : "その他")}");
        Debug.Log($"アニメーション再生中: {parryEffectAnimator.GetCurrentAnimatorStateInfo(0).length > 0}");
        
        // SpriteRenderer情報
        SpriteRenderer spriteRenderer = parryEffectAnimator.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Debug.Log($"SpriteRenderer有効: {spriteRenderer.enabled}");
            Debug.Log($"Sprite有無: {spriteRenderer.sprite != null}");
            if (spriteRenderer.sprite != null)
            {
                Debug.Log($"Sprite名: {spriteRenderer.sprite.name}");
            }
            Debug.Log($"SortingLayer: {spriteRenderer.sortingLayerName}");
            Debug.Log($"SortingOrder: {spriteRenderer.sortingOrder}");
            Debug.Log($"Color: {spriteRenderer.color}");
            Debug.Log($"Alpha: {spriteRenderer.color.a}");
        }
        else
        {
            Debug.LogError("SpriteRendererが見つかりません！");
        }
        
        // アニメーションクリップ情報（簡素化）
        if (parryEffectAnimator.runtimeAnimatorController != null)
        {
            Debug.Log($"AnimatorController名: {parryEffectAnimator.runtimeAnimatorController.name}");
        }
        else
        {
            Debug.LogError("AnimatorControllerが設定されていません！");
        }
        
        Debug.Log("=== 情報確認完了 ===");
    }
    
    [ContextMenu("パリィアニメーション強制再生")]
    public void ForcePlayParryAnimation()
    {
        Debug.Log("=== パリィアニメーション強制再生 ===");
        PlayParrySuccessAnimation();
    }
    
    private void OnDisable()
    {
        ResetDefenseState();
    }
} 