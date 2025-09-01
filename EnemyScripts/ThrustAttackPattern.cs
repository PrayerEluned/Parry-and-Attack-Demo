using UnityEngine;

[CreateAssetMenu(fileName = "New Thrust Attack Pattern", menuName = "Enemy/Thrust Attack Pattern")]
public class ThrustAttackPattern : ScriptableObject
{
    [Header("基本設定")]
    [Tooltip("攻撃パターンの名前")]
    public string patternName = "Default Thrust Attack";
    
    [Tooltip("攻撃パターンの説明")]
    [TextArea(3, 5)]
    public string description = "基本的な突き攻撃パターン";
    
    [Header("攻撃設定")]
    [Tooltip("攻撃力倍率（敵の攻撃力に対して）")]
    [Range(0.1f, 5.0f)]
    public float attackDamageMultiplier = 1.0f;
    
    [Tooltip("攻撃範囲")]
    [Range(0.5f, 3.0f)]
    public float attackRange = 1.5f;
    
    [Tooltip("攻撃のクールダウン時間")]
    [Range(1f, 10f)]
    public float attackCooldown = 2f;
    
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
    [Tooltip("攻撃開始時のアニメーショントリガー")]
    public string attackStartTrigger = "Attack";
    
    [Tooltip("攻撃判定時のアニメーショントリガー")]
    public string attackHitTrigger = "Hit";
    
    /// <summary>
    /// 攻撃パターンの設定が有効かチェック
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(patternName)) return false;
        if (attackDamageMultiplier <= 0) return false;
        if (attackRange <= 0) return false;
        if (attackCooldown <= 0) return false;
        if (pullbackDistance < 0) return false;
        if (pullbackSpeed <= 0) return false;
        if (stopDuration < 0) return false;
        if (thrustDistance <= 0) return false;
        if (thrustSpeed <= 0) return false;
        if (thrustAcceleration <= 0) return false;
        if (postThrustStopDuration < 0) return false;
        
        return true;
    }
    
    /// <summary>
    /// 攻撃パターンの総実行時間を計算
    /// </summary>
    public float GetTotalExecutionTime()
    {
        float pullbackTime = pullbackDistance / pullbackSpeed;
        float thrustTime = thrustDistance / thrustSpeed;
        
        return pullbackTime + stopDuration + thrustTime + postThrustStopDuration;
    }
    
    /// <summary>
    /// 攻撃パターンの詳細情報を取得
    /// </summary>
    public string GetPatternInfo()
    {
        return $"攻撃パターン: {patternName}\n" +
               $"総実行時間: {GetTotalExecutionTime():F2}秒\n" +
               $"攻撃力倍率: {attackDamageMultiplier}\n" +
               $"攻撃範囲: {attackRange}\n" +
               $"引く距離: {pullbackDistance}\n" +
               $"突く距離: {thrustDistance}";
    }
    
    /// <summary>
    /// 攻撃パターンをThrustAttackMovementに適用
    /// </summary>
    public void ApplyToMovement(ThrustAttackMovement movement)
    {
        if (movement == null) return;
        
        // 基本設定を適用
        movement.attackDamageMultiplier = attackDamageMultiplier;
        movement.attackRange = attackRange;
        
        // 引く動作設定を適用
        movement.pullbackDistance = pullbackDistance;
        movement.pullbackSpeed = pullbackSpeed;
        
        // 停止設定を適用
        movement.stopDuration = stopDuration;
        
        // 突く動作設定を適用
        movement.thrustDistance = thrustDistance;
        movement.thrustSpeed = thrustSpeed;
        movement.thrustAcceleration = thrustAcceleration;
        movement.useSpriteBasedDistance = useSpriteBasedDistance;
        
        // ついた後の停止設定を適用
        movement.postThrustStopDuration = postThrustStopDuration;
        
        // スプライト設定を適用
        movement.attackSprite = attackSprite;
        movement.attackSpriteColor = attackSpriteColor;
        
        // 音響設定を適用
        movement.attackStartSound = attackStartSound;
        movement.attackHitSound = attackHitSound;
        
        // エフェクト設定を適用
        movement.attackStartEffect = attackStartEffect;
        movement.attackHitEffect = attackHitEffect;
        
        // アニメーション設定を適用
        movement.attackStartTrigger = attackStartTrigger;
        movement.attackHitTrigger = attackHitTrigger;
    }
    
    /// <summary>
    /// 攻撃パターンのコピーを作成
    /// </summary>
    public ThrustAttackPattern CreateCopy()
    {
        ThrustAttackPattern copy = CreateInstance<ThrustAttackPattern>();
        
        // 基本設定をコピー
        copy.patternName = patternName + " (Copy)";
        copy.description = description;
        copy.attackDamageMultiplier = attackDamageMultiplier;
        copy.attackRange = attackRange;
        copy.attackCooldown = attackCooldown;
        
        // 引く動作設定をコピー
        copy.pullbackDistance = pullbackDistance;
        copy.pullbackSpeed = pullbackSpeed;
        
        // 停止設定をコピー
        copy.stopDuration = stopDuration;
        
        // 突く動作設定をコピー
        copy.thrustDistance = thrustDistance;
        copy.thrustSpeed = thrustSpeed;
        copy.thrustAcceleration = thrustAcceleration;
        copy.useSpriteBasedDistance = useSpriteBasedDistance;
        
        // ついた後の停止設定をコピー
        copy.postThrustStopDuration = postThrustStopDuration;
        
        // スプライト設定をコピー
        copy.attackSprite = attackSprite;
        copy.attackSpriteColor = attackSpriteColor;
        
        // 音響設定をコピー
        copy.attackStartSound = attackStartSound;
        copy.attackHitSound = attackHitSound;
        
        // エフェクト設定をコピー
        copy.attackStartEffect = attackStartEffect;
        copy.attackHitEffect = attackHitEffect;
        
        // アニメーション設定をコピー
        copy.attackStartTrigger = attackStartTrigger;
        copy.attackHitTrigger = attackHitTrigger;
        
        return copy;
    }
}
