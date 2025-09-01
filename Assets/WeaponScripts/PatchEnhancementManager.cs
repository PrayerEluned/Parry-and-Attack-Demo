using UnityEngine;
using System.Collections.Generic;

public class PatchEnhancementManager : MonoBehaviour
{
    [Header("強化管理")]
    public List<EnhancePatch> availablePatches = new List<EnhancePatch>();
    
    // パッチの強化を実行
    public bool EnhancePatch(EnhancePatch patch)
    {
        if (patch == null) return false;
        
        // 強化可能かチェック
        if (!patch.canEnhance) return false;
        
        // 強化上限チェック
        if (patch.patchLevel >= patch.maxEnhanceLevel) return false;
        
        // 素材チェック
        if (!HasRequiredMaterials(patch)) return false;
        
        // 素材を消費
        ConsumeMaterials(patch);
        
        // 強化レベルを上げる
        patch.patchLevel++;
        
        // 特殊効果の適用
        if (patch.isSpecialEffect)
        {
            ApplySpecialEffect(patch);
        }
        
        Debug.Log($"パッチ {patch.patchName} を強化しました。レベル: {patch.patchLevel}");
        return true;
    }
    
    // 必要な素材を持っているかチェック
    private bool HasRequiredMaterials(EnhancePatch patch)
    {
        foreach (var requirement in patch.enhancementMaterials)
        {
            if (requirement.material == null) continue;
            
            // 実際のインベントリシステムに合わせて調整
            // ここでは仮の実装
            int ownedAmount = GetOwnedMaterialAmount(requirement.material);
            if (ownedAmount < requirement.requiredAmount)
            {
                return false;
            }
        }
        return true;
    }
    
    // 素材を消費
    private void ConsumeMaterials(EnhancePatch patch)
    {
        foreach (var requirement in patch.enhancementMaterials)
        {
            if (requirement.material == null) continue;
            
            // 実際のインベントリシステムに合わせて調整
            ConsumeMaterial(requirement.material, requirement.requiredAmount);
        }
    }
    
    // 特殊効果の適用
    private void ApplySpecialEffect(EnhancePatch patch)
    {
        // PlayerStatsを取得
        PlayerStats playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }
        
        if (playerStats == null)
        {
            Debug.LogError("PatchEnhancementManager: PlayerStatsが見つかりません！");
            return;
        }
        
        // SpecialPatchEffectコンポーネントをPlayerStatsから取得または作成
        SpecialPatchEffect specialEffect = playerStats.GetComponent<SpecialPatchEffect>();
        if (specialEffect == null)
        {
            specialEffect = playerStats.gameObject.AddComponent<SpecialPatchEffect>();
            Debug.Log($"PatchEnhancementManager: SpecialPatchEffectをPlayerに追加しました");
        }
        
        // 特殊効果の設定を更新
        specialEffect.effectType = patch.specialEffectType;
        specialEffect.patchLevel = patch.patchLevel;
        specialEffect.affectedStatType = patch.AffectedStatType;
        specialEffect.targetStatType = patch.TargetStatType;
        
        Debug.Log($"PatchEnhancementManager: 特殊効果を適用 - effectType={patch.specialEffectType}, patchLevel={patch.patchLevel}");
        
        // 特殊効果を適用
        specialEffect.ApplySpecialEffect();
    }
    
    // パッチの効果を取得
    public float GetPatchEffectValue(EnhancePatch patch, string statName)
    {
        if (patch == null) return 0f;
        
        if (patch.isSpecialEffect)
        {
            return GetSpecialEffectValue(patch, statName);
        }
        else
        {
            return GetNormalEffectValue(patch, statName);
        }
    }
    
    // 通常効果の値を取得
    private float GetNormalEffectValue(EnhancePatch patch, string statName)
    {
        float baseValue = 0f;
        
        switch (statName)
        {
            case "HP":
                baseValue = patch.enhanceHP;
                break;
            case "Attack":
                baseValue = patch.enhanceAttack;
                break;
            case "MagicAttack":
                baseValue = patch.enhanceMagicAttack;
                break;
            case "Defense":
                baseValue = patch.enhanceDefense;
                break;
            case "MagicDefense":
                baseValue = patch.enhanceMagicDefense;
                break;
            case "Fate":
                baseValue = patch.enhanceFate;
                break;
            case "Speed":
                baseValue = patch.enhanceSpeed;
                break;
        }
        
        return baseValue * patch.patchLevel;
    }
    
    // 特殊効果の値を取得
    private float GetSpecialEffectValue(EnhancePatch patch, string statName)
    {
        switch (patch.specialEffectType)
        {
            case SpecialEffectType.HPToAttack:
                if (statName == patch.AffectedStatType.ToString())
                {
                    float hpReduction = 10f + 2f * patch.patchLevel;
                    return hpReduction * 2f;
                }
                break;
            case SpecialEffectType.FixedStatsWithParry:
                if (statName == "Attack" || statName == "Defense" || 
                    statName == "MagicAttack" || statName == "MagicDefense")
                {
                    return 10f; // 固定値
                }
                break;
            case SpecialEffectType.CounterAttack:
                if (statName == "CounterDamage")
                {
                    return 10f + 2f * patch.patchLevel;
                }
                break;
            case SpecialEffectType.AutoHeal:
                if (statName == "HealAmount")
                {
                    return 1f + 0.2f * patch.patchLevel;
                }
                break;
            case SpecialEffectType.ExpBonus:
                if (statName == "ExpBonus")
                {
                    return patch.patchLevel;
                }
                break;
            case SpecialEffectType.EnemyHPReduce:
                if (statName == "EnemyHPReduce")
                {
                    return patch.patchLevel;
                }
                break;
            case SpecialEffectType.AFDropRate:
                if (statName == "AFDropRate")
                {
                    return 1.5f;
                }
                break;
            case SpecialEffectType.MaterialDropRate:
                if (statName == "MaterialDropRate")
                {
                    return 2f;
                }
                break;
        }
        
        return 0f;
    }
    
    // 仮の実装：素材の所持数を取得
    private int GetOwnedMaterialAmount(ConsumableItem material)
    {
        // 実際のインベントリシステムに合わせて調整
        return 10; // 仮の値
    }
    
    // 仮の実装：素材を消費
    private void ConsumeMaterial(ConsumableItem material, int amount)
    {
        // 実際のインベントリシステムに合わせて調整
        Debug.Log($"素材 {material.name} を {amount} 個消費しました。");
    }
    
    // パッチの強化可能状態をチェック
    public bool CanEnhancePatch(EnhancePatch patch)
    {
        if (patch == null) return false;
        
        if (!patch.canEnhance) return false;
        if (patch.patchLevel >= patch.maxEnhanceLevel) return false;
        if (!HasRequiredMaterials(patch)) return false;
        
        return true;
    }
    
    // パッチの強化コストを取得
    public string GetEnhancementCost(EnhancePatch patch)
    {
        if (patch == null) return "無効なパッチ";
        
        string cost = "必要素材:\n";
        foreach (var requirement in patch.enhancementMaterials)
        {
            if (requirement.material != null)
            {
                cost += $"{requirement.material.name} x{requirement.requiredAmount}\n";
            }
        }
        
        return cost;
    }
} 