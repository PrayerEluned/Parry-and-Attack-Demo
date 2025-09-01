using UnityEngine;

/// <summary>
/// MonoBehaviourにダメージテキスト機能を追加するためのコンポーネント
/// </summary>
public class DamageTextComponent : MonoBehaviour
{
    [Header("Damage Text Settings")]
    public bool showDamageOnTakeDamage = true;
    public bool showDamageOnDealDamage = false;
    public Vector3 damageTextOffset = Vector3.up * 0.5f;
    
    /// <summary>
    /// ダメージを受けた時に呼び出す
    /// </summary>
    /// <param name="damage">受けたダメージ</param>
    /// <param name="isCritical">クリティカルかどうか</param>
    public void OnTakeDamage(int damage, bool isCritical = false)
    {
        if (showDamageOnTakeDamage)
        {
            // ダメージを受けた時の位置に固定（現在の位置 + オフセット）
            Vector3 position = transform.position + damageTextOffset;
            DamageTextManager.ShowDamageAt(position, damage, isCritical, false);
        }
    }
    
    /// <summary>
    /// ダメージを与えた時に呼び出す
    /// </summary>
    /// <param name="damage">与えたダメージ</param>
    /// <param name="isCritical">クリティカルかどうか</param>
    public void OnDealDamage(int damage, bool isCritical = false)
    {
        if (showDamageOnDealDamage)
        {
            Vector3 position = transform.position + damageTextOffset;
            DamageTextManager.ShowDamageAt(position, damage, isCritical, false);
        }
    }
    
    /// <summary>
    /// 回復を受けた時に呼び出す
    /// </summary>
    /// <param name="healAmount">回復量</param>
    public void OnHeal(int healAmount)
    {
        if (showDamageOnTakeDamage)
        {
            Vector3 position = transform.position + damageTextOffset;
            DamageTextManager.ShowDamageAt(position, healAmount, false, true);
        }
    }
    
    /// <summary>
    /// 回復を与えた時に呼び出す
    /// </summary>
    /// <param name="healAmount">回復量</param>
    public void OnGiveHeal(int healAmount)
    {
        if (showDamageOnDealDamage)
        {
            Vector3 position = transform.position + damageTextOffset;
            DamageTextManager.ShowDamageAt(position, healAmount, false, true);
        }
    }
} 