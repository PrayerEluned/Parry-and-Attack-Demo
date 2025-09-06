using UnityEngine;
using Game.Items;

public class MaterialItemAdder : MonoBehaviour
{
    [Header("テスト用素材アイテム追加")]
    [SerializeField] private ConsumableItem[] materialItems;
    
    private void Start()
    {
        Debug.Log("MaterialItemAdder: マテリアルシステム解禁テスト - 初期化を開始");
        
        try
        {
            // 基本的なマテリアルアイテム初期化
            InitializeMaterialAdder();
            Debug.Log("MaterialItemAdder: 初期化完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MaterialItemAdder: 初期化エラー - {e.Message}");
        }
    }
    
    private void InitializeMaterialAdder()
    {
        // マテリアルアイテムアダーの基本初期化
        Debug.Log("MaterialItemAdder: マテリアルアイテムアダー初期化");
        
        // 軽量な初期化処理
        if (transform != null)
        {
            Debug.Log("MaterialItemAdder: Transform確認完了");
        }
    }
    
    [ContextMenu("素材アイテムを手動追加")]
    public void AddMaterialsManually()
    {
        Start();
    }
} 