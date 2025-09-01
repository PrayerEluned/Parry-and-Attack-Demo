using UnityEngine;
using Game.Items;

public class DirectMaterialAdder : MonoBehaviour
{
    [Header("直接素材追加")]
    [SerializeField] private bool addOnStart = true;
    
    // === キャッシュ化された参照（フリーズ対策） ===
    private static MaterialUIManager cachedMaterialUIManager;
    
    private void Start()
    {
        Debug.Log("DirectMaterialAdder: マテリアルシステム解禁テスト - 初期化を開始");
        
        try
        {
            // 基本的なダイレクトマテリアル初期化
            InitializeDirectMaterialAdder();
            Debug.Log("DirectMaterialAdder: 初期化完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DirectMaterialAdder: 初期化エラー - {e.Message}");
        }
    }
    
    private void InitializeDirectMaterialAdder()
    {
        // ダイレクトマテリアルアダーの基本初期化
        Debug.Log("DirectMaterialAdder: ダイレクトマテリアルアダー初期化");
        
        // 軽量な初期化処理
        if (transform != null)
        {
            Debug.Log("DirectMaterialAdder: Transform確認完了");
        }
    }
    
    public void AddMaterialItems()
    {
        // フリーズ対策：Singletonを直接使用
        ArtifactInventory inventory = ArtifactInventory.Instance;
        
        if (inventory == null)
        {
            Debug.LogError("[DirectMaterialAdder] ArtifactInventory.Instanceが見つかりません");
            return;
        }
        
        Debug.Log($"[DirectMaterialAdder] 使用するArtifactInventoryインスタンスID: {inventory.GetInstanceID()}");
        
        Debug.Log("[DirectMaterialAdder] 素材アイテムを作成中...");
        
        // いい木材を作成
        var woodItem = CreateMaterialItem(
            "wood_001",
            "いい木材", 
            "良質な木材。加工されているのはなぜだろうか？",
            500
        );
        
        if (woodItem != null)
        {
            inventory.AddConsumable(woodItem, 50);
            Debug.Log("[DirectMaterialAdder] いい木材を50個追加しました");
        }
        
        // 鉄の欠片を作成
        var ironItem = CreateMaterialItem(
            "iron_001",
            "鉄の欠片",
            "武器の強化に使用する基本素材",
            200
        );
        
        if (ironItem != null)
        {
            inventory.AddConsumable(ironItem, 30);
            Debug.Log("[DirectMaterialAdder] 鉄の欠片を30個追加しました");
        }
        
        // 魔法の粉を作成
        var magicPowder = CreateMaterialItem(
            "magic_001",
            "魔法の粉",
            "エンチャントに使用する魔法素材",
            100
        );
        
        if (magicPowder != null)
        {
            inventory.AddConsumable(magicPowder, 20);
            Debug.Log("[DirectMaterialAdder] 魔法の粉を20個追加しました");
        }
        
        // 追加結果を確認
        var allConsumables = inventory.GetAllOwnedConsumables();
        Debug.Log($"[DirectMaterialAdder] 追加完了！現在の消費アイテム総数: {allConsumables.Count}");
        
        foreach (var item in allConsumables)
        {
            int count = inventory.GetConsumableCount(item);
            Debug.Log($"[DirectMaterialAdder] - {item.itemName}: {count}個");
        }
        
        // MaterialUIManagerをリフレッシュ
        RefreshMaterialUI();
    }
    
    private ConsumableItem CreateMaterialItem(string itemId, string itemName, string description, int maxStack)
    {
        var item = ScriptableObject.CreateInstance<ConsumableItem>();
        item.itemID = itemId;
        item.itemName = itemName;
        item.description = description;
        item.maxStack = maxStack;
        
        // デフォルトアイコンを設定（木材アイコンを使用）
        try
        {
            var icon = Resources.Load<Sprite>("UI picture/Items/wood_0");
            if (icon != null)
            {
                item.icon = icon;
                Debug.Log($"[DirectMaterialAdder] {itemName}にアイコンを設定しました");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[DirectMaterialAdder] {itemName}のアイコン設定でエラー: {e.Message}");
        }
        
        Debug.Log($"[DirectMaterialAdder] {itemName}を作成しました (ID: {itemId})");
        return item;
    }
    
    private void RefreshMaterialUI()
    {
        // フリーズ対策：キャッシュを使用
        if (cachedMaterialUIManager == null)
        {
            cachedMaterialUIManager = Object.FindFirstObjectByType<MaterialUIManager>();
        }
        
        if (cachedMaterialUIManager != null)
        {
            // Singletonインスタンスを使用
            ArtifactInventory correctInventory = ArtifactInventory.Instance;
            
            if (correctInventory != null)
            {
                // リフレクションでpublicフィールドを設定
                var field = typeof(MaterialUIManager).GetField("artifactInventory", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(cachedMaterialUIManager, correctInventory);
                    Debug.Log($"[DirectMaterialAdder] MaterialUIManagerのArtifactInventory参照を正しいインスタンス（ID={correctInventory.GetInstanceID()}）に設定しました");
                }
            }
            
            cachedMaterialUIManager.RefreshMaterialList();
            Debug.Log("[DirectMaterialAdder] MaterialUIManagerをリフレッシュしました");
        }
        else
        {
            Debug.LogWarning("[DirectMaterialAdder] MaterialUIManagerが見つかりません");
        }
    }
    
    [ContextMenu("手動で素材追加")]
    public void ManualAddMaterials()
    {
        Debug.Log("[DirectMaterialAdder] 手動で素材追加を実行");
        AddMaterialItems();
    }
    
    [ContextMenu("現在の消費アイテム確認")]
    public void CheckConsumableItems()
    {
        var inventory = ArtifactInventory.Instance;
        if (inventory == null)
        {
            Debug.LogError("[DirectMaterialAdder] ArtifactInventory.Instanceが見つかりません");
            return;
        }
        
        var allConsumables = inventory.GetAllOwnedConsumables();
        Debug.Log($"[DirectMaterialAdder] 現在の消費アイテム数: {allConsumables.Count}");
        
        if (allConsumables.Count == 0)
        {
            Debug.Log("[DirectMaterialAdder] 消費アイテムが0個です");
        }
        else
        {
            foreach (var item in allConsumables)
            {
                int count = inventory.GetConsumableCount(item);
                Debug.Log($"[DirectMaterialAdder] - {item.itemName} ({item.itemID}): {count}個");
            }
        }
    }
} 