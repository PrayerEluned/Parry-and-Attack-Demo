using UnityEngine;
using Game.Items;
using System.Collections.Generic;

[System.Serializable]
public class DropItem
{
    [Header("ドロップアイテム設定")]
    public ConsumableItem item;
    [Range(0f, 100f)]
    public float dropChance = 50f;
    [Header("ドロップ数設定")]
    public int minCount = 1;
    public int maxCount = 3;
    
    /// <summary>
    /// ドロップするかどうかの判定
    /// </summary>
    public bool ShouldDrop()
    {
        return Random.Range(0f, 100f) <= dropChance;
    }
    
    /// <summary>
    /// ドロップ数を計算
    /// </summary>
    public int GetDropCount()
    {
        return Random.Range(minCount, maxCount + 1);
    }
}

[System.Serializable]
public class ExperienceDrop
{
    [Header("経験値ドロップ設定")]
    [Range(0f, 100f)]
    public float dropChance = 100f; // デフォルトで100%（必ず経験値をドロップ）
    [Header("経験値量設定")]
    public int minExperience = 10;
    public int maxExperience = 20;
    
    /// <summary>
    /// 経験値をドロップするかどうかの判定
    /// </summary>
    public bool ShouldDrop()
    {
        return Random.Range(0f, 100f) <= dropChance;
    }
    
    /// <summary>
    /// ドロップする経験値を計算
    /// </summary>
    public int GetExperienceAmount()
    {
        return Random.Range(minExperience, maxExperience + 1);
    }
}

public class EnemyItemDrop : MonoBehaviour
{
    [Header("ドロップテーブル")]
    [SerializeField] private List<DropItem> dropTable = new List<DropItem>();
    
    [Header("経験値ドロップ設定")]
    [SerializeField] private ExperienceDrop experienceDrop = new ExperienceDrop();
    
    [Header("ドロップ設定")]
    [Range(0f, 100f)]
    [SerializeField] private float globalDropChance = 80f;
    [SerializeField] private bool dropMultipleItems = false;
    
    /// <summary>
    /// アイテムドロップを試行
    /// </summary>
    public void TryDropItems()
    {
        var inventory = ArtifactInventory.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("EnemyItemDrop: ArtifactInventoryが見つかりません。ドロップをスキップします。");
            return;
        }
        
        // グローバルドロップ判定
        if (Random.Range(0f, 100f) > globalDropChance)
        {
            Debug.Log("EnemyItemDrop: グローバルドロップ判定で失敗しました。");
            return;
        }
        
        bool hasDropped = false;
        
        // 経験値ドロップ判定
        if (experienceDrop.ShouldDrop())
        {
            int experienceAmount = experienceDrop.GetExperienceAmount();
            var playerStats = PlayerStats.Instance;
            if (playerStats != null)
            {
                playerStats.GainEXP(experienceAmount);
                Debug.Log($"EnemyItemDrop: 経験値 {experienceAmount} をドロップしました！");
                hasDropped = true;
            }
            else
            {
                Debug.LogWarning("EnemyItemDrop: PlayerStats.Instanceが見つからないため経験値を与えられません");
            }
        }
        
        // ドロップテーブルが空の場合
        if (dropTable == null || dropTable.Count == 0)
        {
            if (!hasDropped)
            {
                Debug.LogWarning("EnemyItemDrop: ドロップテーブルが設定されていません。");
            }
            return;
        }
        
        // 各アイテムのドロップ判定
        foreach (var dropItem in dropTable)
        {
            if (dropItem.item == null) continue;
            
            if (dropItem.ShouldDrop())
            {
                int dropCount = dropItem.GetDropCount();
                
                // 特殊効果パッチによる素材ドロップ量増加を適用
                var playerStats = PlayerStats.Instance;
                if (playerStats != null)
                {
                    var specialEffect = playerStats.GetComponent<SpecialPatchEffect>();
                    if (specialEffect != null)
                    {
                        float dropAmountBonus = specialEffect.GetMaterialDropRateBonus();
                        if (dropAmountBonus > 1f)
                        {
                            dropCount = Mathf.RoundToInt(dropCount * dropAmountBonus);
                            Debug.Log($"特殊効果パッチ: 素材ドロップ量 {dropItem.GetDropCount()} → {dropCount} (倍率: {dropAmountBonus})");
                        }
                    }
                }
                
                inventory.AddConsumable(dropItem.item, dropCount);
                
                Debug.Log($"EnemyItemDrop: {dropItem.item.ItemName} x{dropCount} をドロップしました！");
                
                hasDropped = true;
                
                // 複数アイテムドロップが無効の場合、最初の1つで終了
                if (!dropMultipleItems)
                {
                    break;
                }
            }
        }
        
        if (!hasDropped)
        {
            Debug.Log("EnemyItemDrop: ドロップ判定で何もドロップしませんでした。");
        }
    }
    
    /// <summary>
    /// 特定のアイテムを強制ドロップ（デバッグ用）
    /// </summary>
    public void ForceDropItem(ConsumableItem item, int count = 1)
    {
        var inventory = ArtifactInventory.Instance;
        if (inventory != null && item != null)
        {
            inventory.AddConsumable(item, count);
            Debug.Log($"EnemyItemDrop: {item.ItemName} x{count} を強制ドロップしました！");
        }
    }
    
    /// <summary>
    /// インスペクターでドロップテストするメソッド
    /// </summary>
    [ContextMenu("ドロップテスト")]
    private void TestDrop()
    {
        TryDropItems();
    }
    
    /// <summary>
    /// ドロップテーブルにアイテムを追加（スクリプトから）
    /// </summary>
    public void AddDropItem(ConsumableItem item, float chance, int minCount, int maxCount)
    {
        var newDrop = new DropItem
        {
            item = item,
            dropChance = chance,
            minCount = minCount,
            maxCount = maxCount
        };
        dropTable.Add(newDrop);
    }
    
    /// <summary>
    /// 経験値ドロップ設定を変更（スクリプトから）
    /// </summary>
    public void SetExperienceDrop(float chance, int minExp, int maxExp)
    {
        experienceDrop.dropChance = chance;
        experienceDrop.minExperience = minExp;
        experienceDrop.maxExperience = maxExp;
    }
    
    /// <summary>
    /// 経験値ドロップを無効化
    /// </summary>
    public void DisableExperienceDrop()
    {
        experienceDrop.dropChance = 0f;
    }
    
    /// <summary>
    /// 経験値ドロップを有効化
    /// </summary>
    public void EnableExperienceDrop()
    {
        experienceDrop.dropChance = 100f;
    }
}