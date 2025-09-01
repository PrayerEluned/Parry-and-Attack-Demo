using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Combo Attack", menuName = "Enemy/Combo Attack")]
public class ComboAttackData : ScriptableObject
{
    [Header("基本設定")]
    public string comboName = "Default Combo";
    
    [Header("攻撃設定")]
    [Tooltip("最初のゲージの長さ（赤い警告の表示時間の基準）")]
    public float firstAttackGaugeTime = 1f;
    
    [Tooltip("攻撃回数")]
    [Range(1, 10)]
    public int attackCount = 3;
    
    [Header("攻撃の間隔")]
    [Tooltip("各攻撃のゲージ時間（最初の攻撃以外）")]
    public List<float> attackIntervals = new List<float>();
    
    [Header("攻撃の範囲")]
    [Tooltip("各攻撃の攻撃範囲")]
    public List<float> attackRanges = new List<float>();
    
    [Header("ダメージ設定")]
    [Tooltip("敵の攻撃力に対する乗算値")]
    [Range(0.1f, 5f)]
    public float damageMultiplier = 1f;
    
    /// <summary>
    /// 連撃の総時間を取得
    /// </summary>
    public float GetTotalComboTime()
    {
        float totalTime = firstAttackGaugeTime;
        foreach (float interval in attackIntervals)
        {
            totalTime += interval;
        }
        return totalTime;
    }
    
    /// <summary>
    /// 各攻撃の開始時刻を取得
    /// </summary>
    public List<float> GetAttackStartTimes()
    {
        List<float> startTimes = new List<float>();
        float currentTime = firstAttackGaugeTime; // 最初の攻撃はゲージ時間後に実行
        
        // 最初の攻撃
        startTimes.Add(currentTime);
        
        // 後続の攻撃
        for (int i = 0; i < attackIntervals.Count && i < attackCount - 1; i++)
        {
            currentTime += attackIntervals[i];
            startTimes.Add(currentTime);
        }
        
        return startTimes;
    }
    
    /// <summary>
    /// 各攻撃の警告表示開始時刻を取得
    /// </summary>
    public List<float> GetWarningStartTimes()
    {
        List<float> warningTimes = new List<float>();
        List<float> attackStartTimes = GetAttackStartTimes();
        
        foreach (float attackTime in attackStartTimes)
        {
            // 警告表示時間は最初の攻撃ゲージと同じ
            float warningStartTime = Mathf.Max(0, attackTime - firstAttackGaugeTime);
            warningTimes.Add(warningStartTime);
        }
        
        return warningTimes;
    }
    
    /// <summary>
    /// 指定した攻撃の範囲を取得
    /// </summary>
    public float GetAttackRange(int attackIndex)
    {
        if (attackIndex >= 0 && attackIndex < attackRanges.Count)
        {
            return attackRanges[attackIndex];
        }
        return 2f; // デフォルト範囲
    }
    
    /// <summary>
    /// 指定した攻撃のダメージを取得
    /// </summary>
    public float GetAttackDamage(int attackIndex, float enemyAttackPower)
    {
        return enemyAttackPower * damageMultiplier;
    }
    
    /// <summary>
    /// 設定の妥当性をチェック
    /// </summary>
    public bool IsValid()
    {
        if (attackCount < 1) return false;
        if (firstAttackGaugeTime <= 0) return false;
        if (damageMultiplier <= 0) return false;
        
        // 攻撃範囲のチェック
        if (attackRanges.Count < attackCount)
        {
            Debug.LogWarning($"ComboAttackData: 攻撃範囲の設定が不足しています。必要: {attackCount}, 設定済み: {attackRanges.Count}");
        }
        
        // 攻撃間隔のチェック
        if (attackIntervals.Count < attackCount - 1)
        {
            Debug.LogWarning($"ComboAttackData: 攻撃間隔の設定が不足しています。必要: {attackCount - 1}, 設定済み: {attackIntervals.Count}");
        }
        
        return true;
    }
} 