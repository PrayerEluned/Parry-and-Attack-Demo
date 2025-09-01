using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 現在のヒエラルキー構造を修正して完璧にするツール
/// </summary>
public class QuickMapFix : MonoBehaviour
{
    [Header("修正設定")]
    [SerializeField] private bool createMissingGrid = true;
    [SerializeField] private bool createMissingPropsContainer = true;
    [SerializeField] private bool renameAreaWorld = true;
    [SerializeField] private bool setupLayersAndTags = true;
    
    /// <summary>
    /// 現在の構造を完璧に修正
    /// </summary>
    [ContextMenu("現在の構造を修正")]
    public void FixCurrentStructure()
    {
        Debug.Log("QuickMapFix: 構造修正開始");
        
        if (createMissingGrid) CreateMissingGrid();
        if (createMissingPropsContainer) CreatePropsContainer();
        if (renameAreaWorld) RenameAreaWorld();
        if (setupLayersAndTags) SetupLayersAndTags();
        
        Debug.Log("QuickMapFix: 構造修正完了！これで Cainos タイルが配置できます");
    }
    
    /// <summary>
    /// 不足しているGridシステムを作成
    /// </summary>
    private void CreateMissingGrid()
    {
        // 既存のGridをチェック
        Grid existingGrid = FindFirstObjectByType<Grid>();
        GameObject gridObj;
        
        if (existingGrid == null)
        {
            gridObj = new GameObject("Grid");
            gridObj.AddComponent<Grid>();
            Debug.Log("✅ Grid オブジェクト作成完了");
        }
        else
        {
            gridObj = existingGrid.gameObject;
            Debug.Log("既存の Grid を使用");
        }
        
        // 必要なタイルマップレイヤーを作成
        CreateTilemapLayer(gridObj, "Layer_1", 20, "Layer 1", 1, false);
        CreateTilemapLayer(gridObj, "Layer_2", 21, "Layer 2", 2, false);
        CreateTilemapLayer(gridObj, "Layer_3", 22, "Layer 3", 3, false);
        CreateTilemapLayer(gridObj, "Collision_Layer", 0, "Default", -1, true);
        
        Debug.Log("✅ タイルマップレイヤー作成完了");
    }
    
    /// <summary>
    /// タイルマップレイヤーの作成
    /// </summary>
    private void CreateTilemapLayer(GameObject parent, string layerName, int layer, string sortingLayer, int orderInLayer, bool addCollider)
    {
        // 既存チェック
        Transform existing = parent.transform.Find(layerName);
        if (existing != null)
        {
            Debug.Log($"既存の {layerName} をスキップ");
            return;
        }
        
        GameObject tilemapObj = new GameObject(layerName);
        tilemapObj.transform.SetParent(parent.transform);
        
        // レイヤー設定
        if (layer > 0)
        {
            tilemapObj.layer = layer;
        }
        
        // 基本コンポーネント
        tilemapObj.AddComponent<Tilemap>();
        var renderer = tilemapObj.AddComponent<TilemapRenderer>();
        
        // レンダラー設定
        renderer.sortingLayerName = sortingLayer;
        renderer.sortingOrder = orderInLayer;
        
        // コライダー追加（地面とコリジョンレイヤーのみ）
        if (addCollider)
        {
            var collider = tilemapObj.AddComponent<TilemapCollider2D>();
            collider.compositeOperation = Collider2D.CompositeOperation.Merge;
            
            var composite = tilemapObj.AddComponent<CompositeCollider2D>();
            composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
            
            var rb = tilemapObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }
        
        Debug.Log($"✅ {layerName} 作成完了");
    }
    
    /// <summary>
    /// Props Containerを作成
    /// </summary>
    private void CreatePropsContainer()
    {
        // 既存チェック
        GameObject existing = GameObject.Find("Props_Container");
        if (existing != null)
        {
            Debug.Log("既存の Props_Container をスキップ");
            return;
        }
        
        GameObject propsContainer = new GameObject("Props_Container");
        
        // 子カテゴリー作成
        CreateChildObject(propsContainer, "Buildings");
        CreateChildObject(propsContainer, "Nature_Objects");
        CreateChildObject(propsContainer, "Stairs_Objects");
        CreateChildObject(propsContainer, "Interactive_Objects");
        
        Debug.Log("✅ Props_Container 作成完了");
    }
    
    /// <summary>
    /// Area_World を Area_FirstGame に名前変更
    /// </summary>
    private void RenameAreaWorld()
    {
        GameObject areaWorld = GameObject.Find("Area_World");
        if (areaWorld != null)
        {
            areaWorld.name = "Area_FirstGame";
            Debug.Log("✅ Area_World → Area_FirstGame に名前変更完了");
        }
        else
        {
            Debug.Log("Area_World が見つかりません");
        }
    }
    
    /// <summary>
    /// レイヤーとタグの設定確認
    /// </summary>
    private void SetupLayersAndTags()
    {
        Debug.Log("レイヤー設定の確認:");
        
        // レイヤー確認
        string[] requiredLayers = { "Layer 1", "Layer 2", "Layer 3" };
        int[] layerNumbers = {20, 21, 22};
        
        for (int i = 0; i < requiredLayers.Length; i++)
        {
            int layerNum = LayerMask.NameToLayer(requiredLayers[i]);
            if (layerNum == -1)
            {
                Debug.LogWarning($"❌ レイヤー '{requiredLayers[i]}' が設定されていません");
                Debug.LogWarning($"Edit > Project Settings > Tags and Layers で User Layer {layerNumbers[i]} に '{requiredLayers[i]}' を設定してください");
            }
            else
            {
                Debug.Log($"✅ レイヤー '{requiredLayers[i]}' 設定済み");
            }
        }
        
        // ソーティングレイヤー確認
        Debug.Log("ソーティングレイヤーも Edit > Project Settings > Tags and Layers で設定してください：");
        Debug.Log("- Layer 1 (Order: 1)");
        Debug.Log("- Layer 2 (Order: 2)");
        Debug.Log("- Layer 3 (Order: 3)");

        // ソーティングレイヤーの推奨設定を表示（エディタのみ）
#if UNITY_EDITOR
        EditorGUILayout.HelpBox(
            "推奨されるソーティングレイヤー設定:\n" +
            "- Layer 1 (Order: 1)\n" +
            "- Layer 2 (Order: 2)\n" +
            "- Layer 3 (Order: 3)",
            MessageType.Info
        );
#endif
    }
    
    /// <summary>
    /// 子オブジェクト作成
    /// </summary>
    private void CreateChildObject(GameObject parent, string name)
    {
        Transform existing = parent.transform.Find(name);
        if (existing != null) return;
        
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform);
    }
    
    /// <summary>
    /// Cainosタイルパレット設定ガイド表示
    /// </summary>
    [ContextMenu("Cainosタイル配置ガイドを表示")]
    public void ShowTilePlacementGuide()
    {
        Debug.Log("=== Cainosタイル配置ガイド ===");
        Debug.Log("1. Window > 2D > Tile Palette を開く");
        Debug.Log("2. Create New Palette で新しいパレット作成");
        Debug.Log("3. Cainos アセットからタイルをドラッグ：");
        Debug.Log("   - Assets/Cainos/Pixel Art Top Down - Basic/Tile Palette/");
        Debug.Log("   - TP Grass.prefab");
        Debug.Log("   - TP Stone Ground.prefab");
        Debug.Log("   - TP Wall.prefab");
        Debug.Log("4. タイル配置先：");
        Debug.Log("   - 地面: Layer_1");
        Debug.Log("   - 基本構造: Layer_2");
        Debug.Log("   - 2階部分: Layer_3");
        Debug.Log("5. Cainosプレファブ配置先：");
        Debug.Log("   - 建物: Props_Container/Buildings/");
        Debug.Log("   - 階段: Props_Container/Stairs_Objects/");
        Debug.Log("   - 自然物: Props_Container/Nature_Objects/");
        Debug.Log("=============================");
    }
} 