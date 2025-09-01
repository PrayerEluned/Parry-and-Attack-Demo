using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyAttackSystem : MonoBehaviour
{
    [Header("攻撃設定")]
    [SerializeField] private List<ThrustAttackMovement> availableThrustAttacks = new List<ThrustAttackMovement>();
    [SerializeField] private List<SpinAttackMovement> availableSpinAttacks = new List<SpinAttackMovement>();
    [SerializeField] private float attackGaugeFillTime = 1.5f; // 攻撃ゲージがたまる時間
    [SerializeField] private bool useRandomAttacks = true;
    [SerializeField] private bool showDebugInfo = false;
    
    [Header("コンポーネント参照")]
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private GameObject uiPrefab;
    
    // 内部状態
    private MonoBehaviour currentAttack; // ThrustAttackMovement または SpinAttackMovement
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    
    // UI管理
    private Slider attackSlider;
    
    // イベント
    public System.Action OnAttackStart;
    public System.Action OnAttackEnd;
    
    private void Start()
    {
        // アニメーターの取得
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }
        
        // UI設定
        SetupUI();
        
        // 利用可能な攻撃の初期化
        InitializeAvailableAttacks();
    }
    
    private void Update()
    {
        // UI更新
        UpdateUI();
        
        // 攻撃の更新
        UpdateAttack();
    }
    
    /// <summary>
    /// 利用可能な攻撃の初期化
    /// </summary>
    private void InitializeAvailableAttacks()
    {
        // 子オブジェクトからThrustAttackMovementを取得
        ThrustAttackMovement[] thrustAttacks = GetComponentsInChildren<ThrustAttackMovement>();
        if (thrustAttacks.Length > 0)
        {
            availableThrustAttacks.AddRange(thrustAttacks);
            
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAttackSystem: {thrustAttacks.Length}個の突き攻撃パターンを初期化しました");
            }
        }
        
        // 子オブジェクトからSpinAttackMovementを取得
        SpinAttackMovement[] spinAttacks = GetComponentsInChildren<SpinAttackMovement>();
        if (spinAttacks.Length > 0)
        {
            availableSpinAttacks.AddRange(spinAttacks);
            
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAttackSystem: {spinAttacks.Length}個の回転きり攻撃パターンを初期化しました");
            }
        }
        
        if (availableThrustAttacks.Count == 0 && availableSpinAttacks.Count == 0)
        {
            Debug.LogWarning("EnemyAttackSystem: 利用可能な攻撃パターンが見つかりません");
        }
    }
    
    /// <summary>
    /// 攻撃を開始
    /// </summary>
    public void StartAttack()
    {
        if (isAttacking || Time.time - lastAttackTime < attackGaugeFillTime)
        {
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAttackSystem: 攻撃開始をスキップ - 攻撃中: {isAttacking}, ゲージ蓄積中: {Time.time - lastAttackTime < attackGaugeFillTime}");
            }
            return;
        }
        
        // 利用可能な攻撃がない場合
        int totalAttacks = availableThrustAttacks.Count + availableSpinAttacks.Count;
        if (totalAttacks == 0)
        {
            Debug.LogWarning("EnemyAttackSystem: 利用可能な攻撃がありません");
            return;
        }
        
        // 攻撃パターンを選択（突き攻撃と回転きり攻撃からランダム選択）
        if (useRandomAttacks)
        {
            int randomIndex = Random.Range(0, totalAttacks);
            if (randomIndex < availableThrustAttacks.Count)
            {
                currentAttack = availableThrustAttacks[randomIndex];
            }
            else
            {
                currentAttack = availableSpinAttacks[randomIndex - availableThrustAttacks.Count];
            }
        }
        else
        {
            // 突き攻撃を優先
            if (availableThrustAttacks.Count > 0)
            {
                currentAttack = availableThrustAttacks[0];
            }
            else
            {
                currentAttack = availableSpinAttacks[0];
            }
        }
        
        // 攻撃開始
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // 攻撃スプライトの表示とソート順設定
        SetupAttackSprite();
        
        // 攻撃タイプに応じて開始
        if (currentAttack is ThrustAttackMovement thrustAttack)
        {
            thrustAttack.StartThrustAttack();
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAttackSystem: 突き攻撃開始 - {currentAttack.name}");
            }
        }
        else if (currentAttack is SpinAttackMovement spinAttack)
        {
            spinAttack.StartSpinAttack();
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAttackSystem: 回転きり攻撃開始 - {currentAttack.name}");
            }
        }
        
        // イベント発火
        OnAttackStart?.Invoke();
    }
    
    /// <summary>
    /// 攻撃スプライトの設定
    /// </summary>
    private void SetupAttackSprite()
    {
        if (currentAttack == null) return;
        
        // 攻撃スプライトを表示
        if (currentAttack is ThrustAttackMovement thrustAttack)
        {
            thrustAttack.SetAttackSpriteVisible(true);
            
            // 敵のSpriteRendererのソート順を取得して+1に設定
            SpriteRenderer enemySprite = GetComponent<SpriteRenderer>();
            if (enemySprite != null)
            {
                int enemySortingOrder = enemySprite.sortingOrder;
                thrustAttack.SetAttackSpriteSortingOrder(enemySortingOrder + 1);
            }
        }
        else if (currentAttack is SpinAttackMovement spinAttack)
        {
            spinAttack.SetAttackSpriteVisible(true);
            
            // 敵のSpriteRendererのソート順を取得して+1に設定
            SpriteRenderer enemySprite = GetComponent<SpriteRenderer>();
            if (enemySprite != null)
            {
                int enemySortingOrder = enemySprite.sortingOrder;
                spinAttack.SetAttackSpriteSortingOrder(enemySortingOrder + 1);
            }
        }
    }
    
    /// <summary>
    /// 攻撃の更新
    /// </summary>
    private void UpdateAttack()
    {
        if (currentAttack == null) return;
        
        // 攻撃が完了したかチェック（攻撃タイプに応じて）
        bool isAttackComplete = false;
        
        if (currentAttack is ThrustAttackMovement thrustAttack)
        {
            isAttackComplete = !thrustAttack.IsAttacking;
        }
        else if (currentAttack is SpinAttackMovement spinAttack)
        {
            isAttackComplete = !spinAttack.IsAttacking;
        }
        
        if (isAttackComplete)
        {
            CompleteAttack();
        }
    }
    
    /// <summary>
    /// 攻撃完了
    /// </summary>
    private void CompleteAttack()
    {
        if (currentAttack == null) return;
        
        // 攻撃スプライトを非表示
        if (currentAttack is ThrustAttackMovement thrustAttack)
        {
            thrustAttack.SetAttackSpriteVisible(false);
            thrustAttack.ResetAttack();
        }
        else if (currentAttack is SpinAttackMovement spinAttack)
        {
            spinAttack.SetAttackSpriteVisible(false);
            spinAttack.ResetAttack();
        }
        
        // 状態をリセット
        isAttacking = false;
        currentAttack = null;
        
        // クールタイム開始時刻を記録（これが重要！）
        lastAttackTime = Time.time;
        
        // イベント発火
        OnAttackEnd?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log($"EnemyAttackSystem: 攻撃完了 - クールタイム開始: {lastAttackTime:F2}, 次の攻撃可能時刻: {lastAttackTime + attackGaugeFillTime:F2}");
        }
    }
    
    /// <summary>
    /// 攻撃中かどうか
    /// </summary>
    public bool IsAttacking => isAttacking;
    
    /// <summary>
    /// 攻撃可能かどうか
    /// </summary>
    public bool CanAttack => !isAttacking && Time.time - lastAttackTime >= attackGaugeFillTime;
    
    /// <summary>
    /// 攻撃ゲージの進行度を取得（0.0-1.0）
    /// </summary>
    public float GetAttackGaugeProgress()
    {
        if (isAttacking) return 0f;
        float timeSinceLastAttack = Time.time - lastAttackTime;
        return Mathf.Clamp01(timeSinceLastAttack / attackGaugeFillTime);
    }
    
    /// <summary>
    /// UI設定
    /// </summary>
    private void SetupUI()
    {
        // UI Prefabから攻撃ゲージ用のSliderを取得
        if (uiPrefab != null)
        {
            GameObject uiInstance = Instantiate(uiPrefab, transform);
            uiInstance.transform.localPosition = new Vector3(0, 0.5f, 0);

            Slider[] sliders = uiInstance.GetComponentsInChildren<Slider>();
            foreach (Slider slider in sliders)
            {
                if (slider.name.Contains("Attack")) 
                {
                    attackSlider = slider;
                    attackSlider.maxValue = 1f;
                    attackSlider.value = 0f;
                    
                    if (showDebugInfo)
                    {
                        Debug.Log("EnemyAttackSystem: 攻撃ゲージ用のSliderを設定しました");
                    }
                }
                else if (slider.name.Contains("HP"))
                {
                    // 体力ゲージが見つかった場合、EnemyHealthに通知
                    var enemyHealth = GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        // EnemyHealthのSetupUIを再実行して、既存のUIを使用するようにする
                        enemyHealth.ReinitializeUI();
                        if (showDebugInfo)
                        {
                            Debug.Log("EnemyAttackSystem: 体力ゲージ用のSliderを検出し、EnemyHealthに通知しました");
                        }
                    }
                }
            }
        }
        
        // UIが見つからない場合の警告
        if (attackSlider == null)
        {
            Debug.LogWarning("EnemyAttackSystem: 攻撃ゲージ用のSliderが見つかりません。UI Prefabを設定してください。");
        }
    }
    
    /// <summary>
    /// UI更新
    /// </summary>
    private void UpdateUI()
    {
        if (attackSlider != null)
        {
            if (isAttacking)
            {
                // 攻撃中はゲージを0にする
                attackSlider.maxValue = 1f;
                attackSlider.value = 0f;
            }
            else
            {
                // 攻撃待機中は攻撃ゲージの蓄積を表示
                float gaugeProgress = GetAttackGaugeProgress();
                
                attackSlider.maxValue = 1f;
                attackSlider.value = gaugeProgress;
                
                // デバッグ情報（1秒に1回程度）
                if (showDebugInfo && Time.frameCount % 60 == 0)
                {
                    float timeSinceLastAttack = Time.time - lastAttackTime;
                    Debug.Log($"EnemyAttackSystem: ゲージ更新 - 進行度: {gaugeProgress:F2}, 経過時間: {timeSinceLastAttack:F2}, 次の攻撃まで: {attackGaugeFillTime - timeSinceLastAttack:F2}");
                }
            }
        }
    }
    
    /// <summary>
    /// 利用可能な突き攻撃パターンを追加
    /// </summary>
    public void AddThrustAttackPattern(ThrustAttackMovement attackPattern)
    {
        if (attackPattern != null && !availableThrustAttacks.Contains(attackPattern))
        {
            availableThrustAttacks.Add(attackPattern);
            
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAttackSystem: 突き攻撃パターンを追加 - {attackPattern.name}");
            }
        }
    }
    
    /// <summary>
    /// 利用可能な回転きり攻撃パターンを追加
    /// </summary>
    public void AddSpinAttackPattern(SpinAttackMovement attackPattern)
    {
        if (attackPattern != null && !availableSpinAttacks.Contains(attackPattern))
        {
            availableSpinAttacks.Add(attackPattern);
            
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAttackSystem: 回転きり攻撃パターンを追加 - {attackPattern.name}");
            }
        }
    }
    
    /// <summary>
    /// 利用可能な突き攻撃パターンを削除
    /// </summary>
    public void RemoveThrustAttackPattern(ThrustAttackMovement attackPattern)
    {
        if (availableThrustAttacks.Remove(attackPattern))
        {
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAttackSystem: 突き攻撃パターンを削除 - {attackPattern.name}");
            }
        }
    }
    
    /// <summary>
    /// 利用可能な回転きり攻撃パターンを削除
    /// </summary>
    public void RemoveSpinAttackPattern(SpinAttackMovement attackPattern)
    {
        if (availableSpinAttacks.Remove(attackPattern))
        {
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAttackSystem: 回転きり攻撃パターンを削除 - {attackPattern.name}");
            }
        }
    }
    
    /// <summary>
    /// 攻撃パターンの総数を取得
    /// </summary>
    public int GetAttackPatternCount()
    {
        return availableThrustAttacks.Count + availableSpinAttacks.Count;
    }
    
    /// <summary>
    /// 突き攻撃パターンの数を取得
    /// </summary>
    public int GetThrustAttackPatternCount()
    {
        return availableThrustAttacks.Count;
    }
    
    /// <summary>
    /// 回転きり攻撃パターンの数を取得
    /// </summary>
    public int GetSpinAttackPatternCount()
    {
        return availableSpinAttacks.Count;
    }
} 