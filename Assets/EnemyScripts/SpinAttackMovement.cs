using UnityEngine;
using System.Collections;

public class SpinAttackMovement : MonoBehaviour
{
    [Header("回転きり設定")]
    [Tooltip("刀を表示してから耐える時間")]
    [Range(0.1f, 2.0f)]
    public float prepareDuration = 0.5f;
    
    [Tooltip("π回転を終えるまでの時間")]
    [Range(0.5f, 3.0f)]
    public float spinDuration = 1.0f;
    
    [Tooltip("回転終了後、刀をそのままにする時間")]
    [Range(0.1f, 2.0f)]
    public float postSpinDuration = 0.5f;
    
    [Header("攻撃判定設定")]
    [Tooltip("攻撃力倍率（敵の攻撃力に対して）")]
    [Range(0.1f, 5.0f)]
    public float attackDamageMultiplier = 1.0f;
    
    [Tooltip("攻撃範囲")]
    [Range(0.5f, 3.0f)]
    public float attackRange = 1.5f;
    
    [Header("回転位置設定")]
    [Tooltip("エネミー中心からの回転半径（スプライトの中心がこの距離に配置される）")]
    [Range(0.1f, 5.0f)]
    public float rotationRadius = 1.0f;
    
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
    public enum SpinState
    {
        Prepare,        // 刀を表示して耐える（判定なし）
        Spinning,       // π回転中（判定あり）
        PostSpin,       // 回転終了後、刀をそのまま（判定あり）
        Complete        // 完了
    }
    
    private SpinState currentState = SpinState.Complete;
    private bool isAttackActive = false;
    private float stateStartTime = 0f;
    
    // 回転管理
    private float startRotation;
    private float targetRotation;
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
            Debug.LogError("SpinAttackMovement: SpriteRendererが見つかりません");
        }
        
        // 敵のTransformを取得
        enemyTransform = transform.parent;
        if (enemyTransform == null)
        {
            Debug.LogError("SpinAttackMovement: 親のTransformが見つかりません");
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
        
        // 攻撃スプライトの初期設定
        SetupAttackSprite();
        
        // スプライトの位置を調整（上端をエネミーの中心に）
        AdjustSpritePosition();
        
        // スプライトのピボットポイントを上端に設定し、π/2回転
        SetSpritePivotToTop();
    }
    
    /// <summary>
    /// スプライトの位置を調整（上端中央をエネミーの中心に）
    /// </summary>
    private void AdjustSpritePosition()
    {
        if (attackSpriteRenderer == null || attackSpriteRenderer.sprite == null) return;
        
        // スプライトの境界を取得
        Bounds spriteBounds = attackSpriteRenderer.sprite.bounds;
        
        // スプライトの高さの半分を計算
        float spriteHalfHeight = spriteBounds.size.y * 0.5f;
        
        // 上端中央をエネミーの中心に合わせるため、Y座標を調整
        Vector3 adjustedPosition = transform.localPosition;
        adjustedPosition.y = spriteHalfHeight;
        transform.localPosition = adjustedPosition;
        
        if (showDebugInfo)
        {
            Debug.Log($"SpinAttackMovement: スプライト位置調整 - 高さ: {spriteBounds.size.y}, 調整後Y: {adjustedPosition.y}");
        }
    }
    
    /// <summary>
    /// スプライトのピボットポイントを上端中央に設定し、π/2回転
    /// </summary>
    private void SetSpritePivotToTop()
    {
        if (attackSpriteRenderer == null || attackSpriteRenderer.sprite == null) return;
        
        // スプライトのピボットポイントを上端中央に設定
        // 上端中央がエネミーの中心に来るように位置調整
        Vector3 currentPos = transform.localPosition;
        currentPos.y = 0; // エネミーの中心に配置
        transform.localPosition = currentPos;
        
        // 子オブジェクトの画像をπ/2（90度）回転
        transform.rotation = Quaternion.Euler(0, 0, 90f);
        
        if (showDebugInfo)
        {
            Debug.Log("SpinAttackMovement: スプライトピボットを上端中央に設定し、90度回転");
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
    /// 回転きり攻撃を開始
    /// </summary>
    public void StartSpinAttack()
    {
        if (currentState != SpinState.Complete)
        {
            Debug.LogWarning("SpinAttackMovement: 既に攻撃中です");
            return;
        }
        
        // プレイヤーの方向を取得
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("SpinAttackMovement: プレイヤーが見つかりません");
            return;
        }
        
        playerDirection = (player.transform.position - enemyTransform.position).normalized;
        
        // 開始時の回転を設定（プレイヤーが正面になるように）
        startRotation = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg + 90f;
        targetRotation = startRotation - 180f; // 逆回転（-180度）
        
        // 回転方向を逆回転に設定
        // startRotation: プレイヤー正面
        // targetRotation: プレイヤー背面（逆回転）
        
        // 初期回転を設定（上端中央を軸として回転）
        // 開始位置を計算（プレイヤー正面から開始）
        Vector3 startRotationCenter = enemyTransform.position;
        float startRadius = rotationRadius;
        float startRadian = startRotation * Mathf.Deg2Rad;
        Vector3 startPosition = startRotationCenter + new Vector3(
            Mathf.Cos(startRadian) * startRadius,
            Mathf.Sin(startRadian) * startRadius,
            0
        );
        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(0, 0, startRotation + 90f); // π/2回転を追加
        
        if (showDebugInfo)
        {
            Debug.Log($"SpinAttackMovement: 回転軸設定 - 上端中央を原点として回転（逆回転）");
        }
        
        // 攻撃判定をリセット
        hasHitPlayer = false;
        lastHitPlayer = null;
        
        // 状態を初期化
        ChangeState(SpinState.Prepare);
        
        // 攻撃開始イベント
        OnAttackStart?.Invoke();
        
        // 攻撃開始時の音を再生
        PlayAttackStartSound();
        
        // 攻撃開始時のエフェクトを再生
        PlayAttackStartEffect();
        
        // 攻撃開始時のアニメーションを再生
        PlayAttackStartAnimation();
        
        if (showDebugInfo)
        {
            Debug.Log($"SpinAttackMovement: 回転きり攻撃開始（逆回転） - 開始角度: {startRotation:F1}度, 目標角度: {targetRotation:F1}度, 方向: {playerDirection}, 回転軸: 上端中央");
        }
    }
    
    private void Update()
    {
        if (currentState != SpinState.Complete)
        {
            UpdateSpinMovement();
            
            // 攻撃判定の更新
            if (isAttackActive)
            {
                UpdateAttackCollision();
            }
        }
    }
    
    /// <summary>
    /// 回転きりの移動を更新
    /// </summary>
    private void UpdateSpinMovement()
    {
        float elapsedTime = Time.time - stateStartTime;
        
        switch (currentState)
        {
            case SpinState.Prepare:
                UpdatePrepare(elapsedTime);
                break;
                
            case SpinState.Spinning:
                UpdateSpinning(elapsedTime);
                break;
                
            case SpinState.PostSpin:
                UpdatePostSpin(elapsedTime);
                break;
        }
    }
    
    /// <summary>
    /// 準備状態の更新（刀を表示して耐える）
    /// </summary>
    private void UpdatePrepare(float elapsedTime)
    {
        if (elapsedTime >= prepareDuration)
        {
            // 準備完了、回転開始
            ChangeState(SpinState.Spinning);
        }
    }
    
    /// <summary>
    /// 回転中の更新
    /// </summary>
    private void UpdateSpinning(float elapsedTime)
    {
        float duration = spinDuration;
        float progress = elapsedTime / duration;
        
        if (progress >= 1f)
        {
                    // 回転完了、後処理状態に移行
        // 最終位置を設定
        Vector3 finalRotationCenter = enemyTransform.position;
        float finalRadius = rotationRadius;
        float finalRadian = targetRotation * Mathf.Deg2Rad;
        Vector3 finalPosition = finalRotationCenter + new Vector3(
            Mathf.Cos(finalRadian) * finalRadius,
            Mathf.Sin(finalRadian) * finalRadius,
            0
        );
        transform.position = finalPosition;
        transform.rotation = Quaternion.Euler(0, 0, targetRotation + 90f); // π/2回転を追加
        
        ChangeState(SpinState.PostSpin);
        }
        else
        {
                    // 回転の実行（イージング付き）
        float easedProgress = GetEasedProgress(progress);
        float currentAngle = Mathf.Lerp(startRotation, targetRotation, easedProgress);
        
        // 上端中央を軸として回転（位置計算で実現）
        Vector3 rotationCenter = enemyTransform.position; // エネミーの中心（回転の中心）
        float radius = rotationRadius; // 回転半径（調整可能）
        
        // 回転角度に基づいて位置を計算
        float radian = currentAngle * Mathf.Deg2Rad;
        Vector3 newPosition = rotationCenter + new Vector3(
            Mathf.Cos(radian) * radius,
            Mathf.Sin(radian) * radius,
            0
        );
        
        // 位置を更新
        transform.position = newPosition;
        
        // スプライトの向きを回転角度に合わせる（π/2回転を追加）
        transform.rotation = Quaternion.Euler(0, 0, currentAngle + 90f);
            
            if (showDebugInfo && Time.frameCount % 30 == 0) // 0.5秒に1回程度ログ出力
            {
                Debug.Log($"SpinAttackMovement: 回転中 - 進行度: {easedProgress:F2}, 現在角度: {currentAngle:F1}度, 位置: {newPosition}");
            }
        }
    }
    
    /// <summary>
    /// 回転後の更新
    /// </summary>
    private void UpdatePostSpin(float elapsedTime)
    {
        if (elapsedTime >= postSpinDuration)
        {
            // 後処理完了
            ChangeState(SpinState.Complete);
            
            // 攻撃完了イベントを発火
            OnAttackEnd?.Invoke();
        }
    }
    
    /// <summary>
    /// イージング付きの進行度を取得
    /// </summary>
    private float GetEasedProgress(float progress)
    {
        // SmoothStepで滑らかな回転を実現
        return Mathf.SmoothStep(0f, 1f, progress);
    }
    
    /// <summary>
    /// 状態を変更
    /// </summary>
    private void ChangeState(SpinState newState)
    {
        currentState = newState;
        stateStartTime = Time.time;
        UpdateAttackState();
        
        if (showDebugInfo)
        {
            Debug.Log($"SpinAttackMovement: 状態変更 - {newState}, 時間: {Time.time:F2}");
            
            // 完了状態になった場合の詳細ログ
            if (newState == SpinState.Complete)
            {
                Debug.Log($"SpinAttackMovement: 攻撃完了 - 次の攻撃可能時間: {Time.time + 0.1f:F2}");
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
            case SpinState.Prepare:
                isAttackActive = false; // 判定無効
                break;
                
            case SpinState.Spinning:
            case SpinState.PostSpin:
                isAttackActive = true;  // 判定有効
                break;
                
            case SpinState.Complete:
                isAttackActive = false; // 判定終了
                break;
        }
        
        // 攻撃状態が変化した場合の処理
        if (wasAttackActive != isAttackActive)
        {
            if (isAttackActive)
            {
                if (showDebugInfo) Debug.Log("SpinAttackMovement: 攻撃判定開始");
            }
            else
            {
                if (showDebugInfo) Debug.Log("SpinAttackMovement: 攻撃判定終了");
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
        
        // スプライトの境界ベースで攻撃判定
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
                        Debug.Log($"SpinAttackMovement: 攻撃判定 - ベースダメージ: {baseDamage}, 最終ダメージ: {finalDamage}");
                    }
                }
                else
                {
                    Debug.LogWarning("SpinAttackMovement: 敵のステータスが見つかりません");
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
    /// プレイヤーが攻撃スプライトの範囲内にいるかチェック
    /// </summary>
    private bool IsPlayerInAttackSprite(GameObject player)
    {
        if (attackSpriteRenderer == null || attackSpriteRenderer.sprite == null) return false;
        
        // プレイヤーの位置をスプライトのローカル座標に変換
        Vector3 playerLocalPos = transform.InverseTransformPoint(player.transform.position);
        
        // スプライトの境界を取得
        Bounds spriteBounds = attackSpriteRenderer.sprite.bounds;
        
        // プレイヤーがスプライトの境界内にいるかチェック
        if (spriteBounds.Contains(playerLocalPos))
        {
            if (showDebugInfo)
            {
                Debug.Log($"SpinAttackMovement: プレイヤーが攻撃スプライト内に検出 - 位置: {playerLocalPos}");
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
    public bool IsAttacking => currentState != SpinState.Complete;
    
    /// <summary>
    /// 攻撃可能かどうか
    /// </summary>
    public bool CanAttack => currentState == SpinState.Complete;
    
    /// <summary>
    /// 現在の状態
    /// </summary>
    public SpinState CurrentState => currentState;
    
    /// <summary>
    /// 攻撃判定が有効かどうか
    /// </summary>
    public bool IsAttackActive => isAttackActive;
    
    /// <summary>
    /// 攻撃をリセット
    /// </summary>
    public void ResetAttack()
    {
        currentState = SpinState.Complete;
        isAttackActive = false;
        hasHitPlayer = false;
        lastHitPlayer = null;
        
        // 攻撃スプライトを非表示にする
        SetAttackSpriteVisible(false);
        
        // 位置を初期位置に戻す
        if (enemyTransform != null)
        {
            Vector3 resetPosition = enemyTransform.position;
            resetPosition.y += rotationRadius; // 回転半径に基づく位置
            transform.position = resetPosition;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("SpinAttackMovement: 攻撃をリセット");
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
    /// SpinAttackPatternから設定を適用
    /// </summary>
    public void ApplyPattern(SpinAttackPattern pattern)
    {
        if (pattern == null) return;
        
        // SpinAttackPatternのApplyToMovementメソッドを使用
        pattern.ApplyToMovement(this);
        
        // スプライトの再設定
        SetupAttackSprite();
        
        // スプライトの位置を再調整
        AdjustSpritePosition();
        
        if (showDebugInfo)
        {
            Debug.Log($"SpinAttackMovement: パターンを適用 - {pattern.patternName}");
        }
    }
    
    // デバッグ用のGizmos描画
    private void OnDrawGizmos()
    {
        if (!showAttackHitbox) return;
        
        // 攻撃範囲を可視化
        Gizmos.color = isAttackActive ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 回転角度を可視化
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Vector3 startDir = Quaternion.Euler(0, 0, startRotation) * Vector3.right;
            Gizmos.DrawRay(transform.position, startDir * 2f);
            
            Gizmos.color = Color.red;
            Vector3 endDir = Quaternion.Euler(0, 0, targetRotation) * Vector3.right;
            Gizmos.DrawRay(transform.position, endDir * 2f);
        }
    }
}
