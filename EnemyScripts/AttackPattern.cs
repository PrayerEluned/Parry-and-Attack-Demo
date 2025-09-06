using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Pattern", menuName = "Enemy/Attack Pattern")]
public class AttackPattern : ScriptableObject
{
    [Header("攻撃設定")]
    public string attackName = "Default Attack";
    public float damage = 10f;
    public float attackRange = 2f;
    public float attackDuration = 0.5f;
    
    [Header("警告表示設定")]
    public float warningDisplayTime = 1f; // 赤い警告の表示時間（最初の攻撃ゲージと同じ）
    
    [Header("攻撃エフェクト")]
    public GameObject attackEffectPrefab;
    public AudioClip attackSound;
    
    [Header("攻撃アニメーション")]
    public string attackAnimationTrigger = "Attack";
    public float animationSpeed = 1f;
} 