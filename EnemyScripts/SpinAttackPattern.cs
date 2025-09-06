using UnityEngine;

[CreateAssetMenu(fileName = "New Spin Attack Pattern", menuName = "Enemy/Spin Attack Pattern")]
public class SpinAttackPattern : ScriptableObject
{
    [Header("基本設定")]
    [Tooltip("攻撃パターンの名前")]
    public string patternName = "回転きり";
    
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
    
    /// <summary>
    /// パターンが有効かどうかチェック
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(patternName)) return false;
        if (prepareDuration <= 0) return false;
        if (spinDuration <= 0) return false;
        if (postSpinDuration <= 0) return false;
        if (attackDamageMultiplier <= 0) return false;
        if (attackRange <= 0) return false;
        if (attackSprite == null) return false;
        
        return true;
    }
    
    /// <summary>
    /// 総実行時間を取得
    /// </summary>
    public float GetTotalExecutionTime()
    {
        return prepareDuration + spinDuration + postSpinDuration;
    }
    
    /// <summary>
    /// パターン情報を取得
    /// </summary>
    public string GetPatternInfo()
    {
        return $"回転きりパターン: {patternName}\n" +
               $"総実行時間: {GetTotalExecutionTime():F2}秒\n" +
               $"攻撃力倍率: {attackDamageMultiplier}\n" +
               $"攻撃範囲: {attackRange}\n" +
               $"準備時間: {prepareDuration:F1}秒\n" +
               $"回転時間: {spinDuration:F1}秒\n" +
               $"後処理時間: {postSpinDuration:F1}秒";
    }
    
    /// <summary>
    /// 攻撃パターンをSpinAttackMovementに適用
    /// </summary>
    public void ApplyToMovement(SpinAttackMovement movement)
    {
        if (movement == null) return;
        
        // 基本設定を適用
        movement.attackDamageMultiplier = attackDamageMultiplier;
        movement.attackRange = attackRange;
        
        // 回転きり設定を適用
        movement.prepareDuration = prepareDuration;
        movement.spinDuration = spinDuration;
        movement.postSpinDuration = postSpinDuration;
        
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
    /// パターンのコピーを作成
    /// </summary>
    public SpinAttackPattern CreateCopy()
    {
        SpinAttackPattern copy = CreateInstance<SpinAttackPattern>();
        
        // 基本設定をコピー
        copy.patternName = patternName;
        
        // 回転きり設定をコピー
        copy.prepareDuration = prepareDuration;
        copy.spinDuration = spinDuration;
        copy.postSpinDuration = postSpinDuration;
        
        // 攻撃判定設定をコピー
        copy.attackDamageMultiplier = attackDamageMultiplier;
        copy.attackRange = attackRange;
        
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
