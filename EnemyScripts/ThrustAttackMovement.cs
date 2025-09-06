using UnityEngine;
using System.Collections;

public class ThrustAttackMovement : MonoBehaviour
{
    [Header("引く動作設定")]
    [Tooltip("プレイヤーから遠ざかる距離")]
    [Range(0.5f, 3.0f)]
    public float pullbackDistance = 1f;
    
    [Tooltip("引く動作の速度")]
    [Range(1.0f, 5.0f)]
    public float pullbackSpeed = 2f;
    
    [Header("停止設定")]
    [Tooltip("引いた後の停止時間")]
    [Range(0.1f, 1.0f)]
    public float stopDuration = 0.3f;
    
    [Header("突く動作設定")]
    [Tooltip("突く距離（敵の中心からスプライトの端まで）")]
    [Range(2.0f, 6.0f)]
    public float thrustDistance = 3f;
    
    [Tooltip("突く動作の最大速度")]
    [Range(3.0f, 8.0f)]
    public float thrustSpeed = 5f;
    
    [Tooltip("突く動作の加速時間")]
    [Range(0.1f, 1.0f)]
    public float thrustAcceleration = 0.3f;
    
    [Tooltip("スプライトベースの距離計算を使用")]
    public bool useSpriteBasedDistance = true;
    
    [Header("ついた後の停止設定")]
    [Tooltip("ついた後の停止時間（攻撃判定あり）")]
    [Range(0.1f, 2.0f)]
    public float postThrustStopDuration = 0.5f;
    
    [Header("攻撃判定設定")]
    [Tooltip("攻撃力倍率（敵の攻撃力に対して）")]
    [Range(0.1f, 5.0f)]
    public float attackDamageMultiplier = 1.0f;
    
    [Tooltip("攻撃範囲")]
    [Range(0.5f, 3.0f)]
    public float attackRange = 1.5f;
    
    [Header("スプライト設定")]
    [Tooltip("攻撃に使用するスプライト")]
    public Sprite attackSprite;
    
    [Tooltip("攻撃スプライトの色")]
    public Color attackSpriteColor = Color.white;
    
    [Header("音響設定")]
    [Tooltip("攻撃開始時の音")]
    public AudioClip attackStartSound;
    
    [Tooltip("攻撃判定時の音")]
    public AudioClip attackHitSound;
    
    [Header("エフェクト設定")]
    [Tooltip("攻撃開始時のエフェクト")]
    public GameObject attackStartEffect;
    
    [Tooltip("攻撃判定時のエフェクト")]
    public GameObject attackHitEffect;
    
    [Header("アニメーション設定")]
    [Tooltip("攻撃開始時のアニメーショントリガー（空文字列で無効化）")]
    public string attackStartTrigger = "";
    
    [Tooltip("攻撃判定時のアニメーショントリガー（空文字列で無効化）")]
    public string attackHitTrigger = "";
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグ情報の表示")]
    public bool showDebugInfo = false;
    
    [Tooltip("攻撃判定の可視化")]
    public bool showAttackHitbox = false;
    
    // 内部状態
    public enum ThrustState
    {
        Pullback,           // 引く動作中（判定なし）
        Stop,               // 停止中（判定なし）
        Thrusting,          // 突く動作中（判定あり）
        PostThrustStop,     // ついた後の停止中（判定あり）
        Complete            // 完了
    }
    
    private ThrustState currentState = ThrustState.Complete;
    private bool isAttackActive = false;
    private float stateStartTime = 0f;
    
    // 位置管理
    private Vector3 startPosition;
    private Vector3 pullbackPosition;
    private Vector3 thrustTargetPosition;
    private Vector3 playerDirection;
    
    // コンポーネント参照
    private SpriteRenderer attackSpriteRenderer;
    private Transform enemyTransform;
    private AudioSource audioSource;
    private Animator enemyAnimator;
    
    // 攻撃判定管理（一回のみ判定するため）
    private bool hasHitPlayer = false;
    private GameObject lastHitPlayer = null;
    
    // イベント
    public System.Action OnAttackStart;
    public System.Action OnAttackEnd;
    public System.Action OnThrustComplete;
    
    private void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        // 攻撃スプライトのSpriteRendererを取得
        attackSpriteRenderer = GetComponent<SpriteRenderer>();
        if (attackSpriteRenderer == null)
        {
            Debug.LogError("ThrustAttackMovement: SpriteRendererが見つかりません");
        }
        
        // 敵のTransformを取得
        enemyTransform = transform.parent;
        if (enemyTransform == null)
        {
            Debug.LogError("ThrustAttackMovement: 親のTransformが見つかりません");
        }
        
        // AudioSourceを取得
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 敵のAnimatorを取得
        if (enemyTransform != null)
        {
            enemyAnimator = enemyTransform.GetComponent<Animator>();
        }
        
        // 初期位置を設定
        startPosition = transform.position;
        
        // 攻撃スプライトの初期設定
        SetupAttackSprite();
        
        // スプライトの位置を調整（上端をエネミーの中心に）
        AdjustSpritePosition();
        
        // スプライトのピボットポイントを上端に設定
        SetSpritePivotToTop();
    }
    
    /// <summary>
    /// スプライトの位置を調整（上端をエネミーの中心に）
    /// </summary>
    private void AdjustSpritePosition()
    {
        if (attackSpriteRenderer == null || attackSpriteRenderer.sprite == null) return;
        
        // スプライトの境界を取得
        Bounds spriteBounds = attackSpriteRenderer.sprite.bounds;
        
        // スプライトの高さの半分を計算
        float spriteHalfHeight = spriteBounds.size.y * 0.5f;
        
        // 上端をエネミーの中心に合わせるため、Y座標を調整
        Vector3 adjustedPosition = transform.localPosition;
        adjustedPosition.y = spriteHalfHeight;
        transform.localPosition = adjustedPosition;
        
        if (showDebugInfo)
        {
            Debug.Log($"ThrustAttackMovement: スプライト位置調整 - 高さ: {spriteBounds.size.y}, 調整後Y: {adjustedPosition.y}");
        }
    }
    
    /// <summary>
    /// スプライトのピボットポイントを上端に設定し、π/2回転
    /// </summary>
    private void SetSpritePivotToTop()
    {
        if (attackSpriteRenderer == null || attackSpriteRenderer.sprite == null) return;
        
        // スプライトのピボットポイントを上端に設定
        // これはTransformの位置調整で実現する
        Vector3 currentPos = transform.localPosition;
        currentPos.y = 0; // エネミーの中心に配置
        transform.localPosition = currentPos;
        
        // 子オブジェクトの画像をπ/2（90度）回転
        transform.rotation = Quaternion.Euler(0, 0, 90f);
        
        if (showDebugInfo)
        {
            Debug.Log("ThrustAttackMovement: スプライトピボットを上端に設定し、90度回転");
        }
    }
    
    /// <summary>
    /// スプライトをプレイヤーの方向に回転
    /// </summary>
    private void RotateSpriteTowardsPlayer()
    {
        if (playerDirection == Vector3.zero) return;
        
        // プレイヤーの方向を計算（2D平面での角度）
        float angle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;
        
        // スプライトを回転（持ち手を軸として）
        // 初期のπ/2回転（90度）を考慮して回転
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
        
        if (showDebugInfo)
        {
            Debug.Log($"ThrustAttackMovement: スプライト回転 - 角度: {angle:F1}度 + 90度 = {angle + 90f:F1}度, 方向: {playerDirection}");
        }
    }
    
    /// <summary>
    /// スプライトの回転を強制更新
    /// </summary>
    private void ForceUpdateSpriteRotation()
    {
        if (playerDirection == Vector3.zero) return;
        
        // プレイヤーの方向を計算（2D平面での角度）
        float angle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;
        
        // スプライトを回転（持ち手を軸として）
        // 初期のπ/2回転（90度）を考慮して回転
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
        
        if (showDebugInfo)
        {
            Debug.Log($"ThrustAttackMovement: 強制回転更新 - 角度: {angle:F1}度 + 90度 = {angle + 90f:F1}度");
        }
    }
    
    /// <summary>
    /// 攻撃スプライトの初期設定
    /// </summary>
    private void SetupAttackSprite()
    {
        if (attackSpriteRenderer == null) return;
        
        // スプライトを設定
        if (attackSprite != null)
        {
            attackSpriteRenderer.sprite = attackSprite;
        }
        
        // 色を設定
        attackSpriteRenderer.color = attackSpriteColor;
        
        // 初期状態では非表示
        attackSpriteRenderer.enabled = false;
    }
    
    /// <summary>
    /// 突き攻撃を開始
    /// </summary>
    public void StartThrustAttack()
    {
        if (currentState != ThrustState.Complete)
        {
            Debug.LogWarning("ThrustAttackMovement: 既に攻撃中です");
            return;
        }
        
        // プレイヤーの方向を取得
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("ThrustAttackMovement: プレイヤーが見つかりません");
            return;
        }
        
        playerDirection = (player.transform.position - enemyTransform.position).normalized;
        
        // 初期位置を設定
        startPosition = enemyTransform.position;
        
        // 引く位置を計算
        pullbackPosition = startPosition - playerDirection * pullbackDistance;
        
        // 突く目標位置を計算
        float actualThrustDistance = CalculateActualThrustDistance();
        thrustTargetPosition = startPosition + playerDirection * actualThrustDistance;
        
        // 攻撃判定をリセット
        hasHitPlayer = false;
        lastHitPlayer = null;
        
        // 状態を初期化
        ChangeState(ThrustState.Pullback);
        
        // 攻撃開始イベント
        OnAttackStart?.Invoke();
        
        // 攻撃開始時の音を再生
        PlayAttackStartSound();
        
        // 攻撃開始時のエフェクトを再生
        PlayAttackStartEffect();
        
        // スプライトをプレイヤーの方向に回転
        RotateSpriteTowardsPlayer();
        
        // 攻撃開始時のアニメーションを再生
        PlayAttackStartAnimation();
        
        if (showDebugInfo)
        {
            Debug.Log($"ThrustAttackMovement: 突き攻撃開始 - 方向: {playerDirection}, 引く距離: {pullbackDistance}, 突く距離: {actualThrustDistance}");
            Debug.Log($"ThrustAttackMovement: スプライト回転設定 - 角度: {transform.rotation.eulerAngles.z:F1}度");
        }
    }
    
    /// <summary>
    /// 実際の突く距離を計算（スプライトサイズを考慮）
    /// </summary>
    private float CalculateActualThrustDistance()
    {
        if (!useSpriteBasedDistance || attackSpriteRenderer == null || attackSpriteRenderer.sprite == null)
            return thrustDistance;
        
        // スプライトの境界を取得
        Bounds spriteBounds = attackSpriteRenderer.sprite.bounds;
        
        // スプライトの幅の半分を計算
        float spriteHalfWidth = spriteBounds.size.x * 0.5f;
        
        // 敵の中心からスプライトの遠い端までの距離
        float actualDistance = thrustDistance + spriteHalfWidth;
        
        return actualDistance;
    }
    
    private void Update()
    {
        if (currentState != ThrustState.Complete)
        {
            UpdateThrustMovement();
            
            // 攻撃判定の更新
            if (isAttackActive)
            {
                UpdateAttackCollision();
            }
        }
    }
    
    /// <summary>
    /// 突きの移動を更新
    /// </summary>
    private void UpdateThrustMovement()
    {
        float elapsedTime = Time.time - stateStartTime;
        
        switch (currentState)
        {
            case ThrustState.Pullback:
                UpdatePullback(elapsedTime);
                break;
                
            case ThrustState.Stop:
                UpdateStop(elapsedTime);
                break;
                
            case ThrustState.Thrusting:
                UpdateThrusting(elapsedTime);
                break;
                
            case ThrustState.PostThrustStop:
                UpdatePostThrustStop(elapsedTime);
                break;
        }
    }
    
    /// <summary>
    /// 引く動作の更新
    /// </summary>
    private void UpdatePullback(float elapsedTime)
    {
        float duration = pullbackDistance / pullbackSpeed;
        float progress = elapsedTime / duration;
        
        if (progress >= 1f)
        {
            // 引く動作完了、停止状態に移行
            transform.position = pullbackPosition;
            ChangeState(ThrustState.Stop);
        }
        else
        {
            // 引く動作の実行
            transform.position = Vector3.Lerp(startPosition, pullbackPosition, progress);
            
            // スプライトの回転を維持
            ForceUpdateSpriteRotation();
        }
    }
    
    /// <summary>
    /// 停止中の更新
    /// </summary>
    private void UpdateStop(float elapsedTime)
    {
        if (elapsedTime >= stopDuration)
        {
            // 停止完了、突く動作開始
            ChangeState(ThrustState.Thrusting);
        }
    }
    
    /// <summary>
    /// 突く動作の更新
    /// </summary>
    private void UpdateThrusting(float elapsedTime)
    {
        float duration = CalculateActualThrustDistance() / thrustSpeed;
        float progress = elapsedTime / duration;
        
        if (progress >= 1f)
        {
            // 突く動作完了、ついた後の停止状態に移行
            transform.position = thrustTargetPosition;
            ChangeState(ThrustState.PostThrustStop);
        }
        else
        {
            // 突く動作の実行（イージング付き）
            float easedProgress = GetEasedProgress(progress);
            transform.position = Vector3.Lerp(pullbackPosition, thrustTargetPosition, easedProgress);
            
            // スプライトの回転を維持
            ForceUpdateSpriteRotation();
        }
    }
    
    /// <summary>
    /// ついた後の停止中の更新
    /// </summary>
    private void UpdatePostThrustStop(float elapsedTime)
    {
        if (elapsedTime >= postThrustStopDuration)
        {
            // 停止完了
            ChangeState(ThrustState.Complete);
            
            // 攻撃完了イベントを発火
            OnAttackEnd?.Invoke();
        }
    }
    
    /// <summary>
    /// イージング付きの進行度を取得
    /// </summary>
    private float GetEasedProgress(float progress)
    {
        // SmoothStepで滑らかな加速を実現
        return Mathf.SmoothStep(0f, 1f, progress);
    }
    
    /// <summary>
    /// 状態を変更
    /// </summary>
    private void ChangeState(ThrustState newState)
    {
        currentState = newState;
        stateStartTime = Time.time;
        UpdateAttackState();
        
        if (showDebugInfo)
        {
            Debug.Log($"ThrustAttackMovement: 状態変更 - {newState}, 時間: {Time.time:F2}");
            
            // 完了状態になった場合の詳細ログ
            if (newState == ThrustState.Complete)
            {
                Debug.Log($"ThrustAttackMovement: 攻撃完了 - 次の攻撃可能時間: {Time.time + 0.1f:F2}");
            }
        }
    }
    
    /// <summary>
    /// 攻撃状態を更新
    /// </summary>
    private void UpdateAttackState()
    {
        bool wasAttackActive = isAttackActive;
        
        switch (currentState)
        {
            case ThrustState.Pullback:
            case ThrustState.Stop:
                isAttackActive = false; // 判定無効
                break;
                
            case ThrustState.Thrusting:
            case ThrustState.PostThrustStop:
                isAttackActive = true;  // 判定有効
                break;
                
            case ThrustState.Complete:
                isAttackActive = false; // 判定終了
                break;
        }
        
        // 攻撃状態が変化した場合の処理
        if (wasAttackActive != isAttackActive)
        {
            if (isAttackActive)
            {
                if (showDebugInfo) Debug.Log("ThrustAttackMovement: 攻撃判定開始");
            }
            else
            {
                if (showDebugInfo) Debug.Log("ThrustAttackMovement: 攻撃判定終了");
            }
        }
    }
    
    /// <summary>
    /// 攻撃判定の更新（スプライト境界ベース、一回のみ判定）
    /// </summary>
    private void UpdateAttackCollision()
    {
        // 既にプレイヤーに当たっている場合は判定しない
        if (hasHitPlayer) return;
        
        // プレイヤーとの距離チェック
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // スプライトのalpha値ベースで攻撃判定
        if (IsPlayerInAttackSprite(player))
        {
            // 攻撃判定が当たった場合（一回のみ）
            hasHitPlayer = true;
            lastHitPlayer = player;
            
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // 敵のステータスを取得してダメージ計算
                var enemyHealth = enemyTransform.GetComponent<EnemyHealth>();
                if (enemyHealth != null && enemyHealth.Stats != null)
                {
                    float baseDamage = enemyHealth.Stats.TotalAttack * attackDamageMultiplier;
                    int finalDamage = DamageCalculator.CalculatePhysicalDamage(
                        baseDamage,
                        playerStats.stats.TotalDefense
                    );
                    
                    // ダメージを与える
                    playerStats.TakeDamage(finalDamage, enemyHealth);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"ThrustAttackMovement: 攻撃判定 - ベースダメージ: {baseDamage}, 最終ダメージ: {finalDamage}");
                    }
                }
                else
                {
                    Debug.LogWarning("ThrustAttackMovement: 敵のステータスが見つかりません");
                }
                
                // 攻撃判定時の音を再生
                PlayAttackHitSound();
                
                // 攻撃判定時のエフェクトを再生
                PlayAttackHitEffect();
                
                // 攻撃判定時のアニメーションを再生
                PlayAttackHitAnimation();
            }
        }
    }
    
    /// <summary>
    /// プレイヤーが攻撃スプライトの範囲内にいるかチェック（簡略化版）
    /// </summary>
    private bool IsPlayerInAttackSprite(GameObject player)
    {
        if (attackSpriteRenderer == null || attackSpriteRenderer.sprite == null) return false;
        
        // プレイヤーの位置をスプライトのローカル座標に変換
        Vector3 playerLocalPos = transform.InverseTransformPoint(player.transform.position);
        
        // スプライトの境界を取得
        Bounds spriteBounds = attackSpriteRenderer.sprite.bounds;
        
        // プレイヤーがスプライトの境界内にいるかチェック
        // テクスチャの読み取りエラーを避けるため、境界チェックのみで判定
        if (spriteBounds.Contains(playerLocalPos))
        {
            if (showDebugInfo)
            {
                Debug.Log($"ThrustAttackMovement: プレイヤーが攻撃スプライト内に検出 - 位置: {playerLocalPos}");
            }
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 攻撃開始時の音を再生
    /// </summary>
    private void PlayAttackStartSound()
    {
        if (audioSource != null && attackStartSound != null)
        {
            audioSource.PlayOneShot(attackStartSound);
        }
    }
    
    /// <summary>
    /// 攻撃判定時の音を再生
    /// </summary>
    private void PlayAttackHitSound()
    {
        if (audioSource != null && attackHitSound != null)
        {
            audioSource.PlayOneShot(attackHitSound);
        }
    }
    
    /// <summary>
    /// 攻撃開始時のエフェクトを再生
    /// </summary>
    private void PlayAttackStartEffect()
    {
        if (attackStartEffect != null)
        {
            GameObject effect = Instantiate(attackStartEffect, transform.position, transform.rotation);
            Destroy(effect, 2f); // 2秒後に自動削除
        }
    }
    
    /// <summary>
    /// 攻撃判定時のエフェクトを再生
    /// </summary>
    private void PlayAttackHitEffect()
    {
        if (attackHitEffect != null)
        {
            GameObject effect = Instantiate(attackHitEffect, transform.position, transform.rotation);
            Destroy(effect, 2f); // 2秒後に自動削除
        }
    }
    
    /// <summary>
    /// 攻撃開始時のアニメーションを再生
    /// </summary>
    private void PlayAttackStartAnimation()
    {
        if (enemyAnimator != null && !string.IsNullOrEmpty(attackStartTrigger))
        {
            enemyAnimator.SetTrigger(attackStartTrigger);
        }
    }
    
    /// <summary>
    /// 攻撃判定時のアニメーションを再生
    /// </summary>
    private void PlayAttackHitAnimation()
    {
        if (enemyAnimator != null && !string.IsNullOrEmpty(attackHitTrigger))
        {
            enemyAnimator.SetTrigger(attackHitTrigger);
        }
    }
    
    /// <summary>
    /// 攻撃中かどうか
    /// </summary>
    public bool IsAttacking => currentState != ThrustState.Complete;
    
    /// <summary>
    /// 攻撃可能かどうか
    /// </summary>
    public bool CanAttack => currentState == ThrustState.Complete;
    
    /// <summary>
    /// 現在の状態
    /// </summary>
    public ThrustState CurrentState => currentState;
    
    /// <summary>
    /// 攻撃判定が有効かどうか
    /// </summary>
    public bool IsAttackActive => isAttackActive;
    
    /// <summary>
    /// 攻撃をリセット
    /// </summary>
    public void ResetAttack()
    {
        currentState = ThrustState.Complete;
        isAttackActive = false;
        hasHitPlayer = false;
        lastHitPlayer = null;
        transform.position = startPosition;
        
        // 攻撃スプライトを非表示にする
        SetAttackSpriteVisible(false);
        
        if (showDebugInfo)
        {
            Debug.Log("ThrustAttackMovement: 攻撃をリセット");
        }
    }
    
    /// <summary>
    /// 攻撃スプライトの表示/非表示を設定
    /// </summary>
    public void SetAttackSpriteVisible(bool visible)
    {
        if (attackSpriteRenderer != null)
        {
            attackSpriteRenderer.enabled = visible;
        }
    }
    
    /// <summary>
    /// 攻撃スプライトのソート順を設定
    /// </summary>
    public void SetAttackSpriteSortingOrder(int order)
    {
        if (attackSpriteRenderer != null)
        {
            attackSpriteRenderer.sortingOrder = order;
        }
    }
    
    /// <summary>
    /// ThrustAttackPatternから設定を適用
    /// </summary>
    public void ApplyPattern(ThrustAttackPattern pattern)
    {
        if (pattern == null) return;
        
        // ThrustAttackPatternのApplyToMovementメソッドを使用
        pattern.ApplyToMovement(this);
        
        // スプライトの再設定
        SetupAttackSprite();
        
        // スプライトの位置を再調整
        AdjustSpritePosition();
        
        if (showDebugInfo)
        {
            Debug.Log($"ThrustAttackMovement: パターンを適用 - {pattern.patternName}");
        }
    }
    
    // デバッグ用のGizmos描画
    private void OnDrawGizmos()
    {
        if (!showAttackHitbox) return;
        
        // 攻撃範囲を可視化
        Gizmos.color = isAttackActive ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 引く位置と突く位置を可視化
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pullbackPosition, 0.2f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(thrustTargetPosition, 0.2f);
        }
    }
}
