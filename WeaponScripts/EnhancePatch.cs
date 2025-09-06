using UnityEngine;
using Game.Items;
using System.Collections.Generic;

public enum PatchEffectType
{
    None,
    Additive,         // 攻撃力強化
    Multiplicative,   // 攻撃力強化
    Lifesteal,        // 生命力吸収
    ExtendReach       // 射程距離強化
}

public enum SpecialEffectType
{
    None,
    HPToAttack,           // HPを減らしてATKに変換
    FixedStatsWithParry,  // 固定ステータス+パリィダメージ
    CounterAttack,        // 防御中カウンター攻撃
    AutoHeal,            // 自動回復
    ExpBonus,            // 経験値獲得量増加
    EnemyHPReduce,       // 敵の体力減少
    AFDropRate,          // AFドロップ率増加
    MaterialDropRate      // 素材ドロップ量増加
}



[System.Serializable]
public class PatchEnhancementMaterialRequirement
{
    public ConsumableItem material;
    public int requiredAmount = 1;
}

[CreateAssetMenu(fileName = "NewEnhancePatch", menuName = "Game/Enhance Patch")]
public class EnhancePatch : ScriptableObject, IPlayerItem
{
    [Header("基本情報")]
    public string patchID;
    public string patchName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("強化設定")]
    public bool canEnhance = true;           // 強化可能かどうか
    public int maxEnhanceLevel = 10;         // 強化上限

    [Header("効果タイプ選択")]
    public bool isSpecialEffect = false;     // 特殊効果かどうか
    public PatchEffectType effectType;       // ステータス系効果タイプ
    public SpecialEffectType specialEffectType; // 特殊効果タイプ

    [Header("強化素材リスト（複数指定可）")]
    public List<PatchEnhancementMaterialRequirement> enhancementMaterials = new List<PatchEnhancementMaterialRequirement>();

    [Header("強化幅（1回強化ごとの上昇値）")]
    public float enhanceHP = 0f;
    public float enhanceAttack = 0f;
    public float enhanceMagicAttack = 0f;
    public float enhanceDefense = 0f;
    public float enhanceMagicDefense = 0f;
    public float enhanceFate = 0f;
    public float enhanceSpeed = 0f;

    [Header("効果対象（特殊効果用）")]
    [SerializeField] private StatType affectedStatType = StatType.Attack;
    [SerializeField] private StatType targetStatType = StatType.HP;
    
    // 文字列プロパティ（後方互換性のため）
    public string affectedStat 
    { 
        get => affectedStatType.ToString(); 
        set => System.Enum.TryParse(value, out affectedStatType); 
    }
    public string targetStat 
    { 
        get => targetStatType.ToString(); 
        set => System.Enum.TryParse(value, out targetStatType); 
    }
    
    // StatTypeプロパティ（外部アクセス用）
    public StatType AffectedStatType
    {
        get => affectedStatType;
        set => affectedStatType = value;
    }
    
    public StatType TargetStatType
    {
        get => targetStatType;
        set => targetStatType = value;
    }

    [Header("所持・効果値")]
    public int ownedCount = 1;
    public float effectValuePerUnit = 1f;

    [Header("強化値")]
    public int patchLevel = 0;
    [Header("強化倍率")]
    public float levelMultiplier = 0.1f;

    public float GetEffectiveValue()
    {
        return effectValuePerUnit;
    }

    public string GetEffectDescription()
    {
        float value = effectValuePerUnit;
        float effectiveValue = value * (1f + patchLevel * levelMultiplier);
        
        if (isSpecialEffect)
        {
            switch (specialEffectType)
            {
                case SpecialEffectType.HPToAttack:
                    return $"HP -{10 + 2 * patchLevel}% → {affectedStatType} +{((10 + 2 * patchLevel) * 2)}";
                case SpecialEffectType.FixedStatsWithParry:
                    return "固定ステータス + パリィ時敵HP1.5%ダメージ";
                case SpecialEffectType.CounterAttack:
                    return $"防御中カウンター: 防御力の{10 + 2 * patchLevel}%ダメージ";
                case SpecialEffectType.AutoHeal:
                    return $"3秒おきに最大HPの{1 + 0.2f * patchLevel}%回復";
                case SpecialEffectType.ExpBonus:
                    return $"経験値獲得量 +{effectiveValue}%";
                case SpecialEffectType.EnemyHPReduce:
                    return $"敵の体力 -{effectiveValue}%";
                case SpecialEffectType.AFDropRate:
                    return "AFドロップ率 1.5倍";
                case SpecialEffectType.MaterialDropRate:
                    return "素材ドロップ量 2倍";
                default:
                    return "特殊効果なし";
            }
        }
        else
        {
            switch (effectType)
            {
                case PatchEffectType.Additive:
                    return $"{affectedStatType} +{effectiveValue:F1}";
                case PatchEffectType.Multiplicative:
                    return $"{affectedStatType} ×{effectiveValue:F2}";
                case PatchEffectType.Lifesteal:
                    return $"Lifesteal {effectiveValue * 100:F0}%";
                case PatchEffectType.ExtendReach:
                    return $"Reach +{effectiveValue:F1}";
                default:
                    return "効果なし";
            }
        }
    }

    public enum PatchRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [Header("レアリティ")]
    public PatchRarity rarity = PatchRarity.Common;
    [Header("最大所持数")]
    public int maxStack = 1; // 所持上限はすべて等しく1

    // IPlayerItem 実装
    public string ItemID => patchID;
    public string ItemName => patchName;
    public Sprite Icon => icon;
    public ItemType Type => ItemType.Patch;
    public string Description => description;
}
