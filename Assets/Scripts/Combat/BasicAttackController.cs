using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using AudioSystem;

public class BasicAttackController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private WeaponItem weaponItem;
    [SerializeField] private AudioClip attackSE;
    [SerializeField] private AttackGaugeUI gaugeUI;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform attackOrigin;

    private float attackTimer = 0f;
    private float attackInterval;
    private bool isCharging = false;
    private bool canAttack = true;
    private float gauge = 0f;
    private float range;
    private AudioManager audioManager;

    // 攻撃ゲージが満タンになったかの判定用
    private bool isGaugeFull = false;

    private void Awake()
    {
        if (!playerStats) playerStats = GetComponent<PlayerStats>();
        if (!weaponItem) Debug.LogError("WeaponItem未設定");
        if (!attackOrigin) attackOrigin = transform;
        audioManager = AudioManager.Instance;
        range = weaponItem ? weaponItem.range : 1.2f;
    }

    private void OnEnable()
    {
        canAttack = true;
        gauge = 0f;
        isGaugeFull = false;
        if (gaugeUI) gaugeUI.SetFill(0f);
    }

    private void Update()
    {
        if (!enabled || playerStats == null) return;
        
        try
        {
            UpdateAttackGauge();
            HandleBasicAttack();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BasicAttackController: Update エラー - {e.Message}");
        }
    }
    
    private void UpdateAttackGauge()
    {
        if (!canAttack) return;

        // 武器とプレイヤー統計値から攻撃間隔を計算
        float baseInterval = weaponItem ? weaponItem.attackInterval : 1f;
        float playerSpeed = playerStats.stats.TotalSpeed;
        attackInterval = AttackHelpers.CalcInterval(baseInterval, playerSpeed);

        // 敵が近くにいる時のみゲージを更新
        bool hasEnemies = HasNearbyEnemies();
        
        if (!isGaugeFull && hasEnemies)
        {
            gauge += Time.deltaTime;
            float gaugeRatio = Mathf.Clamp01(gauge / attackInterval);
            
            // ゲージUIを更新
            if (gaugeUI) gaugeUI.SetFill(gaugeRatio);
            
            // ゲージが満タンになったら攻撃可能状態に
            if (gauge >= attackInterval)
            {
                isGaugeFull = true;
                gauge = attackInterval; // 満タン状態を維持
            }
        }
        else if (!hasEnemies)
        {
            // 敵がいない場合はゲージを少しずつ減らす（元の挙動に近づける）
            if (gauge > 0)
            {
                gauge = Mathf.Max(0, gauge - Time.deltaTime * 0.5f); // ゆっくりと減少
                isGaugeFull = false;
                float gaugeRatio = Mathf.Clamp01(gauge / attackInterval);
                if (gaugeUI) gaugeUI.SetFill(gaugeRatio);
            }
        }
        else
        {
            // ゲージ満タン状態でもUIを更新
            if (gaugeUI) gaugeUI.SetFill(1f);
        }
    }
    
    private void HandleBasicAttack()
    {
        // オートモードの場合のみ自動攻撃を実行
        if (CombatModeController.Instance && !CombatModeController.Instance.IsAutoMode)
            return;
        
        // 防御中は攻撃を無効にする
        var defenseController = FindFirstObjectByType<DefenseController>();
        if (defenseController != null && defenseController.IsDefenseActive)
        {
            Debug.Log("BasicAttackController: 防御中のため攻撃不可");
            return; // 防御中は攻撃不可
        }
        
        // 手動攻撃入力（左クリックまたはJキー）- オートモード時のみ
        bool attackInput = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J);
        if (attackInput && isGaugeFull)
        {
            TryPerformAttack();
        }
        
        // 自動攻撃（敵が近くにいる場合）
        if (isGaugeFull && HasNearbyEnemies())
        {
            TryPerformAttack();
        }
    }
    
    /// <summary>
    /// マニュアル攻撃実行（CombatModeControllerから呼び出される）
    /// </summary>
    public void PerformManualAttack()
    {
        // マニュアルモード時のみ実行
        if (CombatModeController.Instance && CombatModeController.Instance.IsAutoMode)
            return;
        
        // 防御中は攻撃を無効にする
        var defenseController = FindFirstObjectByType<DefenseController>();
        if (defenseController != null && defenseController.IsDefenseActive)
        {
            // 防御中に攻撃ボタンを押した場合、防御を解除
            if (defenseController.CurrentState == DefenseController.DefenseState.Defending)
            {
                defenseController.EndDefense();
                Debug.Log("BasicAttackController: 攻撃ボタン押下 - 防御解除");
            }
            else
            {
                Debug.Log("BasicAttackController: 防御中のため攻撃不可");
                return; // パリィウィンドウ中などは攻撃不可
            }
        }
        
        // ゲージが満タンでなくても攻撃可能（マニュアルモード特典）
        if (canAttack)
        {
            PerformBasicAttack();
            // マニュアル攻撃後は短いクールダウンを設定（オプション）
            StartCoroutine(ManualAttackCooldown());
        }
    }
    
    /// <summary>
    /// マニュアル攻撃のクールダウン
    /// </summary>
    private System.Collections.IEnumerator ManualAttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(0.2f); // 0.2秒のクールダウン
        canAttack = true;
    }
    
    private void TryPerformAttack()
    {
        if (!canAttack || !isGaugeFull) return;
        
        PerformBasicAttack();
        
        // 攻撃後にゲージをリセット
        ResetAttackGauge();
    }
    
    private void PerformBasicAttack()
    {
        if (playerStats != null)
        {
            Debug.Log("BasicAttackController: 基本攻撃実行");
            
            // 攻撃範囲内の敵を検索
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, range);
            foreach (var enemy in enemies)
            {
                if (IsEnemyObject(enemy))
                {
                    // 敵にダメージを与える
                    var enemyHealth = enemy.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        float baseDamage = playerStats.stats.TotalAttack;
                        float finalDamage = baseDamage;
                        
                        // パリィボーナスの適用チェック（一旦コメントアウト）
                        // TODO: DefenseControllerとの連携を後で実装
                        /*
                        var defenseController = FindFirstObjectByType<DefenseController>();
                        if (defenseController && defenseController.HasParryBonus)
                        {
                            finalDamage = defenseController.CalculateParryBonusDamage(baseDamage, enemyHealth);
                            Debug.Log($"BasicAttackController: パリィボーナス適用！ {baseDamage} -> {finalDamage}");
                        }
                        */
                        
                        // 特殊効果パッチによる敵HP減少効果を適用
                        var specialEffect = playerStats.GetComponent<SpecialPatchEffect>();
                        if (specialEffect != null)
                        {
                            specialEffect.ApplyEnemyHPReduceEffect(enemyHealth);
                        }
                        
                        enemyHealth.TakeDamage(finalDamage);
                        Debug.Log($"BasicAttackController: 敵に {finalDamage} ダメージ");
                        
                        // ダメージテキスト表示はEnemyHealth.TakeDamage()内で処理されるため、ここでは削除
                        
                        // 攻撃音を再生
                        if (attackSE && audioManager) audioManager.PlaySE(attackSE);
                    }
                    break; // 一体だけ攻撃
                }
            }
        }
    }
    
    private void ResetAttackGauge()
    {
        gauge = 0f;
        isGaugeFull = false;
        if (gaugeUI) gaugeUI.SetFill(0f);
    }
    
    private bool HasNearbyEnemies()
    {
        // 近くに敵がいるかチェック
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, range);
        foreach (var enemy in enemies)
        {
            if (IsEnemyObject(enemy))
            {
                return true;
            }
        }
        return false;
    }
    
    private bool IsEnemyObject(Collider2D target)
    {
        // 複数の方法で敵を識別（安全性向上）
        
        // 方法1: EnemyHealthコンポーネントが付いているかチェック
        if (target.GetComponent<EnemyHealth>() != null)
        {
            return true;
        }
        
        // 方法2: タグが存在する場合のみチェック
        try
        {
            if (target.CompareTag("Enemy"))
            {
                return true;
            }
        }
        catch (UnityException)
        {
            // タグが存在しない場合はスキップ
            Debug.LogWarning("BasicAttackController: 'Enemy'タグが定義されていません。EnemyHealthコンポーネントで敵を識別します。");
        }
        
        // 方法3: オブジェクト名による識別（フォールバック）
        if (target.name.ToLower().Contains("enemy"))
        {
            return true;
        }
        
        return false;
    }

    public void SetEnabled(bool enabled)
    {
        canAttack = enabled;
        if (!enabled)
        {
            ResetAttackGauge();
        }
    }

    // 外部からの攻撃間隔取得用（デバッグやUI表示用）
    public float GetCurrentAttackInterval()
    {
        if (weaponItem && playerStats != null)
        {
            return AttackHelpers.CalcInterval(weaponItem.attackInterval, playerStats.stats.TotalSpeed);
        }
        return 1f;
    }

    // 現在のゲージ進行度を取得（0-1）
    public float GetGaugeProgress()
    {
        if (attackInterval <= 0) return 1f;
        return Mathf.Clamp01(gauge / attackInterval);
    }

    private void OnDrawGizmos()
    {
        if (attackOrigin == null || weaponItem == null) return;
        Gizmos.color = Color.red;
        float range = this.range;
        Vector2 center = (Vector2)attackOrigin.position + (Vector2)attackOrigin.right * range * 0.5f;
        Vector2 size = new Vector2(range, 0.1f);
        Gizmos.matrix = Matrix4x4.TRS(center, attackOrigin.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
} 