using UnityEngine;
using Game.Items;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExistingMaterialAdder : MonoBehaviour
{
    [Header("既存素材アイテムをConsumableItemとして手動設定")]
    [SerializeField] private ConsumableItem[] existingMaterialItems;
    
    [Header("自動検索設定")]
    [SerializeField] private bool autoSearchMaterials = true;
    [SerializeField] private string[] materialKeywords = { "wood", "iron", "stone", "木", "鉄", "石", "材料", "素材" };
    
    private void Start()
    {
        Debug.Log("ExistingMaterialAdder: マテリアルシステム解禁テスト - 初期化を開始");
        
        try
        {
            // 基本的な既存マテリアル初期化
            InitializeExistingMaterialAdder();
            Debug.Log("ExistingMaterialAdder: 初期化完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ExistingMaterialAdder: 初期化エラー - {e.Message}");
        }
    }
    
    private void InitializeExistingMaterialAdder()
    {
        // 既存マテリアルアダーの基本初期化
        Debug.Log("ExistingMaterialAdder: 既存マテリアルアダー初期化");
        
        // 軽量な初期化処理
        if (transform != null)
        {
            Debug.Log("ExistingMaterialAdder: Transform確認完了");
        }
    }
    
    private void SearchAndAddExistingMaterials()
    {
        Debug.Log("[ExistingMaterialAdder] 既存の素材アイテムを自動検索中...");
        
        // ConsumableItemタイプのアセットをすべて検索
        var allConsumables = Resources.FindObjectsOfTypeAll<ConsumableItem>();
        var inventory = ArtifactInventory.Instance;
        
        if (inventory == null)
        {
            Debug.LogError("[ExistingMaterialAdder] ArtifactInventory.Instanceが見つかりません");
            return;
        }
        
        List<ConsumableItem> foundMaterials = new List<ConsumableItem>();
        
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
                    foundMaterials.Add(consumable);
                    Debug.Log($"[ExistingMaterialAdder] 素材アイテム発見: {consumable.itemName} (ID: {consumable.itemID})");
                    break;
                }
            }
        }
        
        // 見つかった素材アイテムをインベントリに追加
        foreach (var material in foundMaterials)
        {
            // ランダムに10-50個追加
            int amount = Random.Range(10, 51);
            inventory.AddConsumable(material, amount);
            Debug.Log($"[ExistingMaterialAdder] {material.itemName}を{amount}個追加しました");
        }
        
        if (foundMaterials.Count == 0)
        {
            Debug.LogWarning("[ExistingMaterialAdder] 既存の素材アイテムが見つかりませんでした。手動で設定してください。");
        }
    }
    
    private void AddExistingMaterialItems()
    {
        Debug.Log("[ExistingMaterialAdder] 手動設定された素材アイテムを追加中...");
        
        var inventory = ArtifactInventory.Instance;
        if (inventory == null)
        {
            Debug.LogError("[ExistingMaterialAdder] ArtifactInventory.Instanceが見つかりません");
            return;
        }
        
        foreach (var material in existingMaterialItems)
        {
            if (material != null)
            {
                int amount = Random.Range(20, 100);
                inventory.AddConsumable(material, amount);
                Debug.Log($"[ExistingMaterialAdder] 手動設定: {material.itemName}を{amount}個追加しました");
            }
        }
    }
    
    [ContextMenu("既存素材を手動検索・追加")]
    public void ManualSearchAndAdd()
    {
        SearchAndAddExistingMaterials();
    }
    
    [ContextMenu("設定済み素材を追加")]
    public void ManualAddExisting()
    {
        AddExistingMaterialItems();
    }
    
    [ContextMenu("すべての ConsumableItem を表示")]
    public void ShowAllConsumableItems()
    {
        Debug.Log("[ExistingMaterialAdder] プロジェクト内のすべてのConsumableItemを検索中...");
        
        var allConsumables = Resources.FindObjectsOfTypeAll<ConsumableItem>();
        Debug.Log($"[ExistingMaterialAdder] 見つかったConsumableItem: {allConsumables.Length}個");
        
        foreach (var item in allConsumables)
        {
            if (item != null)
            {
#if UNITY_EDITOR
                string assetPath = AssetDatabase.GetAssetPath(item);
                Debug.Log($"  - 名前: {item.itemName}, ID: {item.itemID}, パス: {assetPath}");
#else
                Debug.Log($"  - 名前: {item.itemName}, ID: {item.itemID}");
#endif
            }
        }
    }
} 