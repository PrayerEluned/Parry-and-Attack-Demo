using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーの体力を回復するスキル
/// </summary>
public class HealSkill : MonoBehaviour, ISkillBehaviour
{
    [Header("回復設定")]
    [SerializeField] private int healAmount = 50; // 回復量
    [SerializeField] private float castTime = 1.0f; // キャスト時間
    [SerializeField] private AudioClip healSound; // 回復音
    
    private GameObject owner;
    private SkillData skillData;
    
    public float Duration => castTime;
    
    public void Init(GameObject owner, SkillData data)
    {
        this.owner = owner;
        this.skillData = data;
        
        // 回復量は直接フィールドで設定（SkillDataにcustomParametersがないため）
        // 必要に応じてSkillDataにhealAmountフィールドを追加するか、
        // このスキル専用のSkillData派生クラスを作成することを推奨
        
        StartCoroutine(HealRoutine());
    }
    
    private IEnumerator HealRoutine()
    {
        // キャスト時間待機
        yield return new WaitForSeconds(castTime);
        
        // プレイヤーの体力を回復
        var playerStats = owner.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.Heal(healAmount);
            
            // 回復エフェクト表示
            var damageTextComponent = owner.GetComponent<DamageTextComponent>();
            if (damageTextComponent != null)
            {
                damageTextComponent.OnHeal(healAmount);
            }
            else
            {
                // DamageTextComponentがない場合は直接マネージャーを使用
                Vector3 healPosition = owner.transform.position + Vector3.up * 0.8f;
                DamageTextManager.ShowDamageAt(healPosition, healAmount, false, true);
            }
            
            Debug.Log($"HealSkill: プレイヤーの体力を {healAmount} 回復しました");
        }
        else
        {
            Debug.LogWarning("HealSkill: PlayerStatsコンポーネントが見つかりません");
        }
        
        // 回復音を再生
        var audioManager = AudioSystem.AudioManager.Instance;
        if (audioManager != null && healSound != null)
        {
            audioManager.PlaySE(healSound);
        }
    }
} 