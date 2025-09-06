using UnityEngine;

[System.Serializable]
public class EnemyStats
{
    public float baseHP = 50;
    public float baseAttack = 8;
    public float baseDefense = 4;
    public float baseSpeed = 1.0f;

    // 経験値システム
    [Header("経験値設定")]
    public int baseExperience = 10; // 基本経験値
    public float experienceMultiplier = 1.0f; // 経験値倍率

    public float TotalHP => baseHP;
    public float TotalAttack => baseAttack;
    public float TotalDefense => baseDefense;
    public float TotalSpeed => baseSpeed;
    
    // 経験値計算
    public int GetExperienceReward()
    {
        return Mathf.RoundToInt(baseExperience * experienceMultiplier);
    }
}
