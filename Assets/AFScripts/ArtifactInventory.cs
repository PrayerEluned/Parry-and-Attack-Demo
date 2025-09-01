using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Items;
using System;

public class ArtifactInventory : MonoBehaviour
{
    [Header("UIRg[[Q")]
    [SerializeField] private ArtifactUIController uiController;

    // --- 旧AF管理（今後削除予定）---
    // public List<Artifact> ownedArtifacts = new List<Artifact>();
    // public void AddArtifact(Artifact newArtifact, int amount = 1) { ... }

    // --- 新AF管理（IPlayerItem一元化）---
    public void AddArtifact(ArtifactItem artifact, int amount = 1)
    {
        AddItem(artifact, amount);
        var all = GetAllOwnedArtifacts();
        OnInventoryChanged?.Invoke();
    }

    public List<ArtifactItem> GetAllOwnedArtifacts()
    {
        var list = GetItemsByType(ItemType.Artifact).OfType<ArtifactItem>().ToList();
        return list;
    }

    private Dictionary<EnhancePatch, int> patchCounts = new Dictionary<EnhancePatch, int>();

    public EnhancePatch nonePatch; // Inspectorでセット

    // 一元管理リスト
    private List<IPlayerItem> allItems = new List<IPlayerItem>();
    // 種類ごと管理
    private Dictionary<ItemType, List<IPlayerItem>> itemsByType = new Dictionary<ItemType, List<IPlayerItem>>();

    // アイテム追加・削除時のイベント
    public event Action OnInventoryChanged;

    private Dictionary<ConsumableItem, int> consumableCounts = new Dictionary<ConsumableItem, int>();
    private Dictionary<ArtifactItem, int> artifactCounts = new Dictionary<ArtifactItem, int>();

    /// <summary>
    ///A[eBt@Ngǉ
    /// </summary>
    public void AddItem(IPlayerItem item, int amount = 1)
    {
        // ConsumableItem
        if (item is ConsumableItem consumable)
        {
            if (consumableCounts.ContainsKey(consumable))
            {
                consumableCounts[consumable] = Mathf.Min(consumableCounts[consumable] + amount, consumable.maxStack);
            }
            else
            {
                consumableCounts[consumable] = Mathf.Min(amount, consumable.maxStack);
                allItems.Add(consumable);
                if (!itemsByType.ContainsKey(item.Type))
                    itemsByType[item.Type] = new List<IPlayerItem>();
                itemsByType[item.Type].Add(consumable);
            }
            OnInventoryChanged?.Invoke();
            return;
        }
        // EnhancePatch
        if (item is EnhancePatch patch)
        {
            AddPatch(patch, amount);
            return;
        }
        // ArtifactItem
        if (item is ArtifactItem artifact)
        {
            if (artifactCounts.ContainsKey(artifact))
            {
                artifactCounts[artifact] = Mathf.Min(artifactCounts[artifact] + amount, artifact.maxStackCount);
            }
            else
            {
                artifactCounts[artifact] = Mathf.Min(amount, artifact.maxStackCount);
                if (!allItems.Contains(artifact))
                    allItems.Add(artifact);
                if (!itemsByType.ContainsKey(item.Type))
                    itemsByType[item.Type] = new List<IPlayerItem>();
                if (!itemsByType[item.Type].Contains(artifact))
                    itemsByType[item.Type].Add(artifact);
            }
            OnInventoryChanged?.Invoke();
            return;
        }
        // その他（武器・スキル等）
        allItems.Add(item);
        if (!itemsByType.ContainsKey(item.Type))
            itemsByType[item.Type] = new List<IPlayerItem>();
        itemsByType[item.Type].Add(item);
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Z^i+j̃Xe[^XʍvvZ
    /// </summary>
    public float GetTotalAdditive(StatType statType)
    {
        float total = 0f;
        foreach (var af in GetAllOwnedArtifacts())
        {
            if (!af.isMultiplier && af.affectedStat == statType)
            {
                int count = GetArtifactCount(af);
                total += af.effectValue * count;
            }
        }
        return total;
    }

    /// <summary>
    /// Z^i~j̃Xe[^XʍvvZ
    /// </summary>
    public float GetTotalMultiplier(StatType statType)
    {
        if (statType == StatType.ExpGain)
        {
            float total = 0f;
            foreach (var af in GetAllOwnedArtifacts())
            {
                if (af.isMultiplier && af.affectedStat == statType)
                {
                    int count = GetArtifactCount(af);
                    total += Mathf.Pow(af.effectValue, count);
                }
            }
            return total; // 経験値倍率は加算のみ
        }
        else
        {
            float total = 1f;
            foreach (var af in GetAllOwnedArtifacts())
            {
                if (af.isMultiplier && af.affectedStat == statType)
                {
                    int count = GetArtifactCount(af);
                    total *= Mathf.Pow(af.effectValue, count);
                }
            }
            return total; // ステータスは乗算
        }
    }

    public int GetPatchSlotExpandCount()
    {
        int count = 0;
        foreach (var af in GetAllOwnedArtifacts())
        {
            if (af != null && af.isPatchSlotExpandArtifact)
            {
                count += GetArtifactCount(af);
            }
        }
        return count;
    }
    public static ArtifactInventory Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        if (nonePatch != null)
        {
            AddItem(nonePatch);
        }
    }

    private void Start()
    {
        // 緊急復旧: Phase 1に戻す（UI連携が原因と判明）
        Debug.Log("ArtifactInventory: 緊急復旧 - Phase 1に戻します（UI連携無効）");
        
        try
        {
            // Step 1: 基本的な初期化のみ（UI連携なし）
            InitializeBasicSystem();
            
            Debug.Log("ArtifactInventory: Phase 1復活完了 - 基本機能のみ動作中（UI連携無効）");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ArtifactInventory: Phase 1復活エラー - {e.Message}");
            // エラーが発生した場合は完全無効化
            Debug.Log("ArtifactInventory: エラーのため完全無効化に戻します");
            return;
        }
    }
    
    private void InitializeBasicSystem()
    {
        // 基本的なデータ構造の初期化
        if (allItems == null)
            allItems = new List<IPlayerItem>();
        
        if (itemsByType == null)
            itemsByType = new Dictionary<ItemType, List<IPlayerItem>>();
        
        if (consumableCounts == null)
            consumableCounts = new Dictionary<ConsumableItem, int>();
        
        if (artifactCounts == null)
            artifactCounts = new Dictionary<ArtifactItem, int>();
        
        if (patchCounts == null)
            patchCounts = new Dictionary<EnhancePatch, int>();
        
        Debug.Log("ArtifactInventory: 基本データ構造初期化完了");
        
        // デフォルトパッチの追加（安全チェック付き）
        if (nonePatch != null)
        {
            AddItem(nonePatch);
            Debug.Log("ArtifactInventory: デフォルトパッチ追加完了");
        }
    }
    
    private void InitializeUIConnections()
    {
        Debug.Log("ArtifactInventory: UI連携初期化開始");
        
        try
        {
            // UIとの連携を安全に初期化
            if (uiController != null)
            {
                OnInventoryChanged += uiController.RefreshArtifactList;
                Debug.Log("ArtifactInventory: UI連携初期化完了");
            }
            else
            {
                Debug.LogWarning("ArtifactInventory: UIControllerが設定されていません");
            }
            
            // 初期化完了を通知
            OnInventoryChanged?.Invoke();
            Debug.Log("ArtifactInventory: 全初期化完了 - UI連携成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ArtifactInventory: UI連携初期化エラー - {e.Message}");
            // UI連携でエラーが発生しても、基本機能は維持
            Debug.Log("ArtifactInventory: UI連携エラーのため、基本機能のみで継続");
        }
    }

    private void OnDestroy()
    {
    }

    public void AddPatch(EnhancePatch patch, int amount = 1)
    {
        if (patchCounts.ContainsKey(patch))
        {
            patchCounts[patch] = Mathf.Min(patchCounts[patch] + amount, patch.maxStack);
        }
        else
        {
            patchCounts[patch] = Mathf.Min(amount, patch.maxStack);
            allItems.Add(patch);
            if (!itemsByType.ContainsKey(patch.Type))
                itemsByType[patch.Type] = new List<IPlayerItem>();
            itemsByType[patch.Type].Add(patch);
        }
        OnInventoryChanged?.Invoke();
    }
    public List<EnhancePatch> GetAllOwnedPatches()
    {
        return patchCounts.Keys.ToList();
    }

    public int GetPatchCount(EnhancePatch patch)
    {
        return patchCounts.ContainsKey(patch) ? patchCounts[patch] : 0;
    }

    // アイテム消費・削除
    public void RemoveItem(IPlayerItem item, int amount = 1)
    {
        // ConsumableItemの場合は個数を減らす
        if (item is ConsumableItem consumable)
        {
            if (consumableCounts.ContainsKey(consumable))
            {
                int currentCount = consumableCounts[consumable];
                int newCount = Mathf.Max(0, currentCount - amount);
                
                if (newCount == 0)
                {
                    // 個数が0になった場合はリストからも削除
                    consumableCounts.Remove(consumable);
                    allItems.Remove(consumable);
                    if (itemsByType.ContainsKey(item.Type))
                        itemsByType[item.Type].Remove(consumable);
                    Debug.Log($"消費アイテム {consumable.ItemName} を完全に削除しました");
                }
                else
                {
                    // 個数を減らすだけ
                    consumableCounts[consumable] = newCount;
                    Debug.Log($"消費アイテム {consumable.ItemName}: {currentCount} → {newCount}個");
                }
                
                OnInventoryChanged?.Invoke();
                return;
            }
            else
            {
                Debug.LogWarning($"消費アイテム {consumable.ItemName} は所持していません");
                return;
            }
        }
        
        // ArtifactItemの場合も個数を減らす
        if (item is ArtifactItem artifact)
        {
            if (artifactCounts.ContainsKey(artifact))
            {
                int currentCount = artifactCounts[artifact];
                int newCount = Mathf.Max(0, currentCount - amount);
                
                if (newCount == 0)
                {
                    artifactCounts.Remove(artifact);
                    allItems.Remove(artifact);
                    if (itemsByType.ContainsKey(item.Type))
                        itemsByType[item.Type].Remove(artifact);
                    Debug.Log($"アーティファクト {artifact.ItemName} を完全に削除しました");
                }
                else
                {
                    artifactCounts[artifact] = newCount;
                    Debug.Log($"アーティファクト {artifact.ItemName}: {currentCount} → {newCount}個");
                }
                
                OnInventoryChanged?.Invoke();
                return;
            }
            else
            {
                Debug.LogWarning($"アーティファクト {artifact.ItemName} は所持していません");
                return;
            }
        }
        
        // その他のアイテム（武器、スキルなど）は従来通り削除
        allItems.Remove(item);
        if (itemsByType.ContainsKey(item.Type))
            itemsByType[item.Type].Remove(item);
        OnInventoryChanged?.Invoke();
    }

    // 種類ごとに取得
    public List<IPlayerItem> GetItemsByType(ItemType type)
    {
        return itemsByType.ContainsKey(type) ? itemsByType[type] : new List<IPlayerItem>();
    }

    // 所持判定
    public bool HasItem(IPlayerItem item)
    {
        return allItems.Contains(item);
    }

    // --- 武器管理（IPlayerItem一元化）---
    public void AddWeapon(WeaponItem weapon, int amount = 1)
    {
        AddItem(weapon, amount);
    }
    public List<WeaponItem> GetAllOwnedWeapons()
    {
        return GetItemsByType(ItemType.Weapon).OfType<WeaponItem>().ToList();
    }

    // --- スキル管理（IPlayerItem一元化）---
    public void AddSkill(SkillData skill, int amount = 1)
    {
        AddItem(skill, amount);
    }
    public List<SkillData> GetAllOwnedSkills()
    {
        return GetItemsByType(ItemType.Skill).OfType<SkillData>().ToList();
    }

    // --- 消費アイテム管理（IPlayerItem一元化）---
    public void AddConsumable(ConsumableItem item, int amount = 1)
    {
        AddItem(item, amount);
        var all = GetAllOwnedConsumables();
        OnInventoryChanged?.Invoke();
    }
    public List<ConsumableItem> GetAllOwnedConsumables()
    {
        var list = GetItemsByType(ItemType.Consumable).OfType<ConsumableItem>().ToList();
        return list;
    }

    public int GetConsumableCount(ConsumableItem consumable)
    {
        return consumableCounts.ContainsKey(consumable) ? consumableCounts[consumable] : 0;
    }
    public int GetArtifactCount(ArtifactItem artifact)
    {
        return artifactCounts.ContainsKey(artifact) ? artifactCounts[artifact] : 0;
    }

    // すべての所持アイテム（AF, Consumable, Weapon, Patch）を返す
    public List<IPlayerItem> GetAllOwnedItems()
    {
        return allItems;
    }

    // 武器の所持数を返す（現状は1本ずつ管理想定、今後拡張可）
    public int GetWeaponCount(WeaponItem weapon)
    {
        // allItemsに含まれていれば1、なければ0（今後スタック対応なら辞書管理に拡張）
        return allItems.Contains(weapon) ? 1 : 0;
    }

    // StatTypeごとに加算値を計算（リストを使い回す最適化版）
    public float GetTotalAdditiveFromList(StatType statType, List<ArtifactItem> artifacts)
    {
        float total = 0f;
        foreach (var af in artifacts)
        {
            if (!af.isMultiplier && af.affectedStat == statType)
            {
                int count = GetArtifactCount(af);
                total += af.effectValue * count;
            }
        }
        return total;
    }

    // StatTypeごとに乗算値を計算（リストを使い回す最適化版）
    public float GetTotalMultiplierFromList(StatType statType, List<ArtifactItem> artifacts)
    {
        if (statType == StatType.ExpGain)
        {
            float total = 0f;
            foreach (var af in artifacts)
            {
                if (af.isMultiplier && af.affectedStat == statType)
                {
                    int count = GetArtifactCount(af);
                    total += Mathf.Pow(af.effectValue, count);
                }
            }
            return total; // 経験値倍率は加算のみ
        }
        else
        {
            float total = 1f;
            foreach (var af in artifacts)
            {
                if (af.isMultiplier && af.affectedStat == statType)
                {
                    int count = GetArtifactCount(af);
                    total *= Mathf.Pow(af.effectValue, count);
                }
            }
            return total; // ステータスは乗算
        }
    }
}
