using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Added for List

/// <summary>
/// 特殊効果パッチの処理を担当するクラス
/// 
/// 【使用方法】
/// 1. パッチのSOでisSpecialEffect = trueに設定
/// 2. specialEffectTypeで効果を選択
/// 3. affectedStatTypeで対象ステータスを選択
/// 4. このコンポーネントをプレイヤーにアタッチ
/// 5. パッチの強化時にApplySpecialEffect()を呼び出し
/// 
/// 【実際の適用方法】
/// - プレイヤーのGameObjectにこのコンポーネントをアタッチ
/// - PatchEnhancementManagerから特殊効果を適用
/// - 各効果は自動的に発動する（AutoHeal、CounterAttack等）
/// 
/// 特殊効果の詳細:
/// - HPToAttack: HPを減らしてATKに変換（強化レベル1: HP-12% → ATK+24）
/// - FixedStatsWithParry: 固定ステータス+パリィダメージ（パリィ時敵HP1.5%ダメージ）
/// - CounterAttack: 防御中カウンター（強化レベル1: 防御力12%ダメージ、レベル5: 防御力20%ダメージ）
/// - AutoHeal: 自動回復（強化レベル1: 最大HP1.2%/3秒、レベル5: 最大HP2%/3秒）
/// - ExpBonus: 経験値増加（強化レベル1: +1%、レベル5: +5%）
/// - EnemyHPReduce: 敵HP減少（強化レベル1: 敵HP-1%、レベル5: 敵HP-5%）
/// - AFDropRate: AFドロップ率1.5倍
/// - MaterialDropRate: 素材ドロップ量2倍
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("")]
public class SpecialPatchEffect : MonoBehaviour
{
    [Header("特殊効果設定")]
    [Tooltip("特殊効果の種類を選択")]
    public SpecialEffectType effectType;
    
    [Tooltip("パッチの強化レベル（0-10）")]
    public int patchLevel = 0;
    
    [Tooltip("影響を受けるステータス（HPToAttack効果で使用）")]
    public StatType affectedStatType = StatType.Attack;
    
    [Tooltip("対象ステータス（HPToAttack効果で使用）")]
    public StatType targetStatType = StatType.HP;
    
    [Header("カウンターアタック設定")]
    [Tooltip("カウンターアタックの範囲（メートル）")]
    [SerializeField] private float counterAttackRange = 3f;
    
    [Tooltip("カウンターアタックのダメージ倍率")]
    [SerializeField] private float counterDamageMultiplier = 1.0f;
    
    [Tooltip("範囲攻撃かどうか")]
    [SerializeField] private bool isRangeAttack = false;
    
    [Tooltip("範囲攻撃時の最大対象数")]
    [SerializeField] private int maxTargets = 5;
    
    [HideInInspector] private PlayerStats playerStats;
    [HideInInspector] private EnemyHealth enemyHealth;
    [HideInInspector] private float autoHealTimer = 0f;
    [HideInInspector] private bool isDefending = false;
    
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }
        
        // 自動回復の開始
        if (effectType == SpecialEffectType.AutoHeal)
        {
            StartCoroutine(AutoHealCoroutine());
        }
    }
    
    void Update()
    {
        // 防御状態の監視
        if (effectType == SpecialEffectType.CounterAttack)
        {
            // ここで防御状態を監視（実際の実装に合わせて調整）
            // isDefending = playerStats.IsDefending();
        }
    }
    
    /// <summary>
    /// HPを減らしてATKに変換する効果
    /// 強化レベル1: HP-12% → ATK+24
    /// 強化レベル5: HP-20% → ATK+40
    /// </summary>
    public void ApplyHPToAttackEffect()
    {
        if (effectType != SpecialEffectType.HPToAttack || playerStats == null) return;
        
        float hpReduction = 10f + 2f * patchLevel;
        float attackBonus = hpReduction * 2f;
        
        // HPを減らす
        int hpReductionAmount = Mathf.RoundToInt(playerStats.stats.TotalHP * (hpReduction / 100f));
        playerStats.TakeDamage(hpReductionAmount);
        
        // ATKを増やす
        if (affectedStatType == StatType.Attack)
        {
            playerStats.stats.additionalAttack += attackBonus;
        }
        else if (affectedStatType == StatType.MagicAttack)
        {
            playerStats.stats.additionalMagicAttack += attackBonus;
        }
        
        // 効果を適用
        playerStats.ApplyAllEffects();
    }
    
    /// <summary>
    /// 固定ステータス+パリィダメージ効果
    /// 攻撃、防御、魔法攻撃、魔法防御を10固定
    /// パリィ成功時に敵の最大HPの1.5%ダメージ
    /// </summary>
    public void ApplyFixedStatsWithParryEffect()
    {
        if (effectType != SpecialEffectType.FixedStatsWithParry || playerStats == null) return;
        
        // 固定ステータスを設定
        playerStats.stats.baseAttack = 10f;
        playerStats.stats.baseDefense = 10f;
        playerStats.stats.baseMagicAttack = 10f;
        playerStats.stats.baseMagicDefense = 10f;
        
        // 効果を適用
        playerStats.ApplyAllEffects();
    }
    
    /// <summary>
    /// パリィ時のダメージ処理
    /// 敵の最大HPの1.5%ダメージ
    /// </summary>
    public void OnParrySuccess(EnemyHealth enemy)
    {
        if (effectType == SpecialEffectType.FixedStatsWithParry && enemy != null)
        {
            float damage = enemy.MaxHP * 0.015f; // 1.5%
            enemy.TakeDamage(damage);
        }
    }
    
    /// <summary>
    /// 防御中カウンター攻撃効果
    /// 強化レベル1: 防御力12%ダメージ
    /// 強化レベル5: 防御力20%ダメージ
    /// 強化レベル10: 防御力30%ダメージ
    /// </summary>
    public void OnDefendCounter(EnemyHealth enemy)
    {
        Debug.Log($"[SpecialPatchEffect] OnDefendCounter呼び出し - enemy={enemy}, effectType={effectType}");
        
        if (effectType != SpecialEffectType.CounterAttack) 
        {
            Debug.Log($"[SpecialPatchEffect] カウンターアタック: 効果タイプが違います - {effectType}");
            return;
        }
        
        if (enemy == null)
        {
            Debug.LogWarning("[SpecialPatchEffect] カウンターアタック: 対象敵がnullです");
            return;
        }
        
        Debug.Log($"[SpecialPatchEffect] カウンターアタック開始: 対象={enemy.name}, 範囲攻撃={isRangeAttack}");
        
        if (isRangeAttack)
        {
            // 範囲攻撃
            PerformRangeCounterAttack();
        }
        else
        {
            // 単体攻撃
            PerformSingleCounterAttack(enemy);
        }
    }
    
    /// <summary>
    /// 単体カウンターアタック
    /// </summary>
    private void PerformSingleCounterAttack(EnemyHealth enemy)
    {
        float counterDamage = playerStats.stats.TotalDefense * ((10f + 2f * patchLevel) / 100f) * counterDamageMultiplier;
        enemy.TakeDamage(counterDamage);
        Debug.Log($"単体カウンターアタック: 防御力{playerStats.stats.TotalDefense}の{(10f + 2f * patchLevel)}% × {counterDamageMultiplier} = {counterDamage}ダメージ → {enemy.name}");
    }
    
    /// <summary>
    /// 範囲カウンターアタック
    /// </summary>
    private void PerformRangeCounterAttack()
    {
        // プレイヤーの位置を中心に範囲内の敵を検索
        Vector3 playerPosition = playerStats.transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerPosition, counterAttackRange);
        
        List<EnemyHealth> targets = new List<EnemyHealth>();
        
        foreach (Collider2D collider in colliders)
        {
            EnemyHealth enemy = collider.GetComponent<EnemyHealth>();
            if (enemy != null && enemy != playerStats.GetComponent<EnemyHealth>())
            {
                targets.Add(enemy);
            }
        }
        
        // 最大対象数まで制限
        if (targets.Count > maxTargets)
        {
            targets = targets.GetRange(0, maxTargets);
        }
        
        float counterDamage = playerStats.stats.TotalDefense * ((10f + 2f * patchLevel) / 100f) * counterDamageMultiplier;
        
        Debug.Log($"範囲カウンターアタック: {targets.Count}体の敵に{counterDamage}ダメージ");
        
        foreach (EnemyHealth target in targets)
        {
            target.TakeDamage(counterDamage);
            Debug.Log($"範囲カウンターアタック: {target.name}に{counterDamage}ダメージ");
        }
    }
    
    /// <summary>
    /// 自動回復効果
    /// 強化レベル1: 最大HPの1.2%/3秒
    /// 強化レベル5: 最大HPの2%/3秒
    /// 強化レベル10: 最大HPの3%/3秒
    /// </summary>
    private IEnumerator AutoHealCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            
            if (effectType == SpecialEffectType.AutoHeal && playerStats != null)
            {
                float healAmount = playerStats.stats.TotalHP * ((1f + 0.2f * patchLevel) / 100f);
                int healAmountInt = Mathf.RoundToInt(healAmount);
                playerStats.Heal(healAmountInt);
            }
        }
    }
    
    /// <summary>
    /// 経験値獲得量増加効果
    /// 強化レベル1: +1%
    /// 強化レベル5: +5%
    /// 強化レベル10: +10%
    /// </summary>
    public float GetExpBonus()
    {
        if (effectType != SpecialEffectType.ExpBonus) return 0f;
        return patchLevel; // 強化レベル分の%増加
    }
    
    /// <summary>
    /// 敵の体力減少効果
    /// 強化レベル1: 敵HP-1%
    /// 強化レベル5: 敵HP-5%
    /// 強化レベル10: 敵HP-10%
    /// </summary>
    public void ApplyEnemyHPReduceEffect(EnemyHealth enemy)
    {
        if (effectType != SpecialEffectType.EnemyHPReduce || enemy == null) return;
        
        float reduction = patchLevel; // 強化レベル分の%減少
        float damage = enemy.MaxHP * (reduction / 100f);
        enemy.TakeDamage(damage);
    }
    
    /// <summary>
    /// AFドロップ率増加効果
    /// AFのドロップ率を1.5倍にする
    /// </summary>
    public float GetAFDropRateBonus()
    {
        if (effectType != SpecialEffectType.AFDropRate) return 1f;
        return 1.5f; // 1.5倍
    }
    
    /// <summary>
    /// 素材ドロップ量増加効果
    /// 素材のドロップ量を2倍にする
    /// </summary>
    public float GetMaterialDropRateBonus()
    {
        if (effectType != SpecialEffectType.MaterialDropRate) return 1f;
        return 2f; // 2倍
    }
    
    /// <summary>
    /// 防御状態の更新
    /// DefenseControllerから呼び出される
    /// </summary>
    public void SetDefendingState(bool defending)
    {
        isDefending = defending;
    }
    
    /// <summary>
    /// 特殊効果の適用
    /// パッチの強化時に呼び出される
    /// </summary>
    public void ApplySpecialEffect()
    {
        switch (effectType)
        {
            case SpecialEffectType.HPToAttack:
                ApplyHPToAttackEffect();
                break;
            case SpecialEffectType.FixedStatsWithParry:
                ApplyFixedStatsWithParryEffect();
                break;
            case SpecialEffectType.CounterAttack:
                // 防御状態の監視はUpdateで行う
                break;
            case SpecialEffectType.AutoHeal:
                // 自動回復はStartで開始
                break;
            case SpecialEffectType.ExpBonus:
            case SpecialEffectType.EnemyHPReduce:
            case SpecialEffectType.AFDropRate:
            case SpecialEffectType.MaterialDropRate:
                // これらの効果は外部から呼び出される
                break;
        }
    }
} 