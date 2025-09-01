using UnityEngine;

/// <summary>
/// 回復スキルのプレハブを作成するためのヘルパー
/// </summary>
public class HealSkillPrefab : MonoBehaviour
{
    [Header("回復スキル設定")]
    [SerializeField] private int healAmount = 50;
    [SerializeField] private float castTime = 1.0f;
    [SerializeField] private AudioClip healSound;
    
    private void Awake()
    {
        // HealSkillコンポーネントを自動追加
        if (GetComponent<HealSkill>() == null)
        {
            var healSkill = gameObject.AddComponent<HealSkill>();
            
            // 設定値を反映
            var healAmountField = typeof(HealSkill).GetField("healAmount", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var castTimeField = typeof(HealSkill).GetField("castTime", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var healSoundField = typeof(HealSkill).GetField("healSound", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (healAmountField != null) healAmountField.SetValue(healSkill, healAmount);
            if (castTimeField != null) castTimeField.SetValue(healSkill, castTime);
            if (healSoundField != null) healSoundField.SetValue(healSkill, healSound);
        }
    }
} 