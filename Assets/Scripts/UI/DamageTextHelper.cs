using UnityEngine;

/// <summary>
/// ダメージテキスト表示のためのヘルパークラス
/// 既存の戦闘システムに簡単に統合できます
/// </summary>
public static class DamageTextHelper
{
    /// <summary>
    /// 通常ダメージを表示
    /// </summary>
    /// <param name="target">ダメージを受けた対象</param>
    /// <param name="damage">ダメージ量</param>
    public static void ShowNormalDamage(Transform target, int damage)
    {
        DamageTextManager.ShowDamageOnTarget(target, damage, false, false);
    }
    
    /// <summary>
    /// クリティカルダメージを表示
    /// </summary>
    /// <param name="target">ダメージを受けた対象</param>
    /// <param name="damage">ダメージ量</param>
    public static void ShowCriticalDamage(Transform target, int damage)
    {
        DamageTextManager.ShowDamageOnTarget(target, damage, true, false);
    }
    
    /// <summary>
    /// 回復を表示
    /// </summary>
    /// <param name="target">回復を受けた対象</param>
    /// <param name="healAmount">回復量</param>
    public static void ShowHeal(Transform target, int healAmount)
    {
        DamageTextManager.ShowDamageOnTarget(target, healAmount, false, true);
    }
    
    /// <summary>
    /// ワールド座標でダメージを表示
    /// </summary>
    /// <param name="worldPosition">表示位置</param>
    /// <param name="damage">ダメージ量</param>
    /// <param name="isCritical">クリティカルかどうか</param>
    public static void ShowDamageAtPosition(Vector3 worldPosition, int damage, bool isCritical = false)
    {
        DamageTextManager.ShowDamageAt(worldPosition, damage, isCritical, false);
    }
    
    /// <summary>
    /// ワールド座標で回復を表示
    /// </summary>
    /// <param name="worldPosition">表示位置</param>
    /// <param name="healAmount">回復量</param>
    public static void ShowHealAtPosition(Vector3 worldPosition, int healAmount)
    {
        DamageTextManager.ShowDamageAt(worldPosition, healAmount, false, true);
    }
}

 