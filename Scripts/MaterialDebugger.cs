using UnityEngine;
using Game.Items;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MaterialDebugger : MonoBehaviour
{
    [Header("デバッグ情報")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // === キャッシュ化された参照（フリーズ対策） ===
    private static MaterialUIManager cachedMaterialUIManager;
    
    private void Start()
    {
        Debug.Log("MaterialDebugger: マテリアルシステム解禁テスト - 初期化を開始");
        
        try
        {
            // 基本的なマテリアルデバッグ初期化
            InitializeMaterialDebugger();
            Debug.Log("MaterialDebugger: 初期化完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MaterialDebugger: 初期化エラー - {e.Message}");
        }
    }
    
    private void InitializeMaterialDebugger()
    {
        // マテリアルデバッガーの基本初期化
        Debug.Log("MaterialDebugger: マテリアルデバッガー初期化");
        
        // 軽量な初期化処理
        if (transform != null)
        {
            Debug.Log("MaterialDebugger: Transform確認完了");
        }
    }
    
    public void DebugMaterialSystem()
    {
        if (!enableDebugLogs) return;
        
        var inventory = ArtifactInventory.Instance;
        if (inventory == null)
        {
            Debug.LogError("[MaterialDebugger] ArtifactInventory.Instanceが見つかりません");
            return;
        }
        
        // 現在の消費アイテム一覧を確認
        var consumables = inventory.GetAllOwnedConsumables();
        Debug.Log($"[MaterialDebugger] 現在の消費アイテム数: {consumables.Count}");
        
        if (consumables.Count == 0)
        {
            Debug.LogWarning("[MaterialDebugger] 消費アイテムが0個です。既存の素材アイテム（上級木材）を読み込みます...");
            
            // 上級木材を直接読み込み
            LoadAdvancedWoodMaterial(inventory);
            
            // 再度確認
            consumables = inventory.GetAllOwnedConsumables();
            Debug.Log($"[MaterialDebugger] 読み込み後の消費アイテム数: {consumables.Count}");
            
            if (consumables.Count == 0)
            {
                Debug.LogWarning("[MaterialDebugger] 上級木材が見つからないため、テスト用アイテムを作成します...");
                CreateTestMaterialItems();
            }
        }
        
        // 現在のアイテム一覧を表示
        foreach (var item in consumables)
        {
            int count = inventory.GetConsumableCount(item);
            Debug.Log($"[MaterialDebugger] - {item.itemName} ({item.itemID}): {count}個");
        }
        
        // MaterialUIManagerのリフレッシュを呼び出し
        RefreshMaterialUI();
    }
    
    private void LoadAdvancedWoodMaterial(ArtifactInventory inventory)
    {
        Debug.Log("[MaterialDebugger] 上級木材を読み込み中...");
        
#if UNITY_EDITOR
        try
        {
            // AssetDatabaseを使用して直接読み込み
            var advancedWood = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/ScriptableObjects/Materials/上級木材.asset");
            if (advancedWood != null)
            {
                Debug.Log($"[MaterialDebugger] 上級木材を発見: {advancedWood.name}");
                
                // 上級木材をConsumableItemとして作成
                var woodConsumable = ScriptableObject.CreateInstance<ConsumableItem>();
                woodConsumable.itemID = "advanced_wood";
                woodConsumable.itemName = "いい木材";
                woodConsumable.description = "良質な木材。加工されているのはなぜだろうか？";
                woodConsumable.maxStack = 500;
                
                // 木のアイコンを設定
                var woodIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI picture/Items/wood.png");
                if (woodIcon == null)
                {
                    // 別のパスを試行
                    var allWoodSprites = AssetDatabase.FindAssets("wood t:Sprite");
                    if (allWoodSprites.Length > 0)
                    {
                        string spritePath = AssetDatabase.GUIDToAssetPath(allWoodSprites[0]);
                        woodIcon = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                        Debug.Log($"[MaterialDebugger] 木のアイコンを発見: {spritePath}");
                    }
                }
                
                if (woodIcon != null)
                {
                    woodConsumable.icon = woodIcon;
                    Debug.Log("[MaterialDebugger] 木のアイコンを設定しました");
                }
                
                inventory.AddConsumable(woodConsumable, 30);
                Debug.Log("[MaterialDebugger] いい木材を30個追加しました");
            }
            else
            {
                Debug.LogWarning("[MaterialDebugger] AssetDatabaseでも上級木材が見つかりませんでした");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MaterialDebugger] AssetDatabase読み込みエラー: {e.Message}");
        }
#endif
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
            cachedMaterialUIManager.RefreshMaterialList();
            Debug.Log("[MaterialDebugger] MaterialUIManagerをリフレッシュしました");
        }
        else
        {
            Debug.LogWarning("[MaterialDebugger] MaterialUIManagerが見つかりません");
        }
    }
    
    private bool TryAddExistingMaterials()
    {
        Debug.Log("[MaterialDebugger] 既存の素材アイテムを検索中...");
        
        var inventory = ArtifactInventory.Instance;
        var allConsumables = Resources.FindObjectsOfTypeAll<ConsumableItem>();
        
        string[] materialKeywords = { "wood", "iron", "stone", "木", "鉄", "石", "材料", "素材", "grass", "草" };
        bool foundAny = false;
        
        foreach (var consumable in allConsumables)
        {
            if (consumable == null) continue;
            
            string itemName = consumable.itemName.ToLower();
            string itemId = consumable.itemID?.ToLower() ?? "";
            
            // キーワードマッチング
            foreach (var keyword in materialKeywords)
            {
                if (itemName.Contains(keyword.ToLower()) || itemId.Contains(keyword.ToLower()))
                {
                    int amount = Random.Range(15, 60);
                    inventory.AddConsumable(consumable, amount);
                    Debug.Log($"[MaterialDebugger] 既存素材発見・追加: {consumable.itemName} ({amount}個)");
                    foundAny = true;
                    break;
                }
            }
        }
        
        return foundAny;
    }
    
    private void CreateTestMaterialItems()
    {
        Debug.Log("[MaterialDebugger] テスト用素材アイテムを作成中...");
        
        var inventory = ArtifactInventory.Instance;
        
        // 動的にConsumableItemを作成してテスト
        var testMaterial1 = ScriptableObject.CreateInstance<ConsumableItem>();
        testMaterial1.itemID = "material_iron_fragment";
        testMaterial1.itemName = "鉄の欠片";
        testMaterial1.description = "武器の強化に使用する基本素材";
        testMaterial1.maxStack = 99;
        
        var testMaterial2 = ScriptableObject.CreateInstance<ConsumableItem>();
        testMaterial2.itemID = "material_magic_powder";
        testMaterial2.itemName = "魔法の粉";
        testMaterial2.description = "エンチャントに使用する魔法素材";
        testMaterial2.maxStack = 50;
        
        var testMaterial3 = ScriptableObject.CreateInstance<ConsumableItem>();
        testMaterial3.itemID = "material_dragon_scale";
        testMaterial3.itemName = "竜の鱗";
        testMaterial3.description = "レア装備の製作に必要な貴重な素材";
        testMaterial3.maxStack = 10;
        
        // インベントリに追加
        inventory.AddConsumable(testMaterial1, 25);
        inventory.AddConsumable(testMaterial2, 15);
        inventory.AddConsumable(testMaterial3, 3);
        
        Debug.Log("[MaterialDebugger] テスト用素材アイテムを追加しました");
        
        // 追加後に再度確認
        Invoke(nameof(DebugMaterialSystem), 1f);
    }
    
    [ContextMenu("手動デバッグ実行")]
    public void ManualDebug()
    {
        DebugMaterialSystem();
    }
    
    [ContextMenu("テスト素材追加")]
    public void ManualAddTestMaterials()
    {
        CreateTestMaterialItems();
    }
} 