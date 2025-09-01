using UnityEngine;
using System.Collections.Generic;

public class AttackHitbox : MonoBehaviour
{
    [Header("攻撃判定設定")]
    [SerializeField] private LayerMask targetLayers = -1; // 全てのレイヤーを対象とする（デバッグ用）
    [SerializeField] private bool useTrigger = true;
    
    private int attackIndex;
    private ComboAttackData comboData;
    private EnemyAttackSystem attackSystem;
    private EnemyStats enemyStats;
    private List<GameObject> hitTargets = new List<GameObject>();
    
    public AttackPattern Pattern => null; // 新しい構造ではAttackPatternは使用しない
    
    /// <summary>
    /// 攻撃判定を初期化
    /// </summary>
    public void Initialize(int index, ComboAttackData combo, EnemyAttackSystem system)
    {
        attackIndex = index;
        comboData = combo;
        attackSystem = system;
        
        // 敵のステータスを取得（EnemyHealthから）
        EnemyHealth enemyHealth = system.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyStats = enemyHealth.Stats;
        }
        
        // コライダーを設定
        SetupCollider();
        
        // 攻撃範囲に合わせてスケールを調整（attackRangeは半径として使用）
        float attackRange = comboData.GetAttackRange(attackIndex);
        transform.localScale = Vector3.one * attackRange * 2f; // 直径として設定
        
        Debug.Log($"AttackHitbox: 初期化完了 - 攻撃{attackIndex + 1}, 範囲: {attackRange}, レイヤー: {LayerMask.LayerToName(gameObject.layer)} ({gameObject.layer}), 対象レイヤー: {targetLayers}");
    }
    
    /// <summary>
    /// コライダーを設定
    /// </summary>
    private void SetupCollider()
    {
        // 既存のコライダーを削除
        Collider existingCollider = GetComponent<Collider>();
        if (existingCollider != null)
        {
            DestroyImmediate(existingCollider);
        }
        
        // 新しいコライダーを追加
        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = 0.5f; // スケールで調整するので0.5
        sphereCollider.isTrigger = useTrigger;
        
        // レイヤーを設定（攻撃判定用）- レイヤーが存在しない場合はデフォルトを使用
        int enemyAttackLayer = LayerMask.NameToLayer("EnemyAttack");
        if (enemyAttackLayer != -1)
        {
            gameObject.layer = enemyAttackLayer;
        }
        else
        {
            // EnemyAttackレイヤーが存在しない場合はデフォルトレイヤーを使用
            gameObject.layer = 0; // Default layer
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (useTrigger)
        {
            HandleHit(other.gameObject);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!useTrigger)
        {
            HandleHit(collision.gameObject);
        }
    }
    
    /// <summary>
    /// ヒット処理
    /// </summary>
    private void HandleHit(GameObject target)
    {
        Debug.Log($"AttackHitbox: ヒット検出 - ターゲット: {target.name}, レイヤー: {LayerMask.LayerToName(target.layer)} ({target.layer})");
        
        // 既にヒットしたターゲットは無視
        if (hitTargets.Contains(target))
        {
            Debug.Log($"AttackHitbox: 既にヒット済みのターゲットを無視: {target.name}");
            return;
        }
        
        // レイヤーチェック
        if (((1 << target.layer) & targetLayers) == 0)
        {
            Debug.Log($"AttackHitbox: レイヤー不一致 - ターゲットレイヤー: {LayerMask.LayerToName(target.layer)} ({target.layer}), 対象レイヤー: {targetLayers}");
            return;
        }
        
        // プレイヤーのダメージ処理
        PlayerStats playerStats = target.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            Debug.Log($"AttackHitbox: プレイヤーにダメージを適用: {target.name}");
            ApplyDamage(playerStats);
            hitTargets.Add(target);
        }
        else
        {
            Debug.Log($"AttackHitbox: PlayerStatsコンポーネントが見つかりません: {target.name}");
        }
    }
    
    /// <summary>
    /// ダメージを適用
    /// </summary>
    private void ApplyDamage(PlayerStats playerStats)
    {
        Debug.Log($"AttackHitbox: ApplyDamage開始 - comboData: {comboData != null}, playerStats: {playerStats != null}, enemyStats: {enemyStats != null}");
        
        if (comboData == null || playerStats == null || enemyStats == null)
        {
            Debug.LogError($"AttackHitbox: 必要なコンポーネントがnull - comboData: {comboData == null}, playerStats: {playerStats == null}, enemyStats: {enemyStats == null}");
            return;
        }
        
        // ダメージ計算（敵の攻撃力を参照）
        float baseDamage = comboData.GetAttackDamage(attackIndex, enemyStats.TotalAttack);
        int damage = DamageCalculator.CalculatePhysicalDamage(
            baseDamage,
            playerStats.stats.TotalDefense
        );
        
        Debug.Log($"AttackHitbox: ダメージ計算 - ベースダメージ: {baseDamage}, 最終ダメージ: {damage}, プレイヤー防御力: {playerStats.stats.TotalDefense}");
        
        // ダメージを適用（攻撃者を渡す）
        playerStats.TakeDamage(damage, GetComponentInParent<EnemyHealth>());
        
        Debug.Log($"AttackHitbox: プレイヤーにダメージ {damage} を適用完了 - 攻撃{attackIndex + 1} (敵攻撃力: {enemyStats.TotalAttack}, 倍率: {comboData.damageMultiplier})");
    }
    
    /// <summary>
    /// 攻撃判定を無効化
    /// </summary>
    public void DisableHitbox()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }
    
    /// <summary>
    /// 攻撃判定を有効化
    /// </summary>
    public void EnableHitbox()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }
    
    private void OnDestroy()
    {
        hitTargets.Clear();
    }
} 