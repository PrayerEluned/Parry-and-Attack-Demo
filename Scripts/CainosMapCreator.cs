using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Cainosアセットの正しい構造でWorldとTownマップを作成
/// 公式サンプル SC All Props.unity の構造を基準
/// </summary>
public class CainosMapCreator : MonoBehaviour
{
    [Header("作成設定")]
    [SerializeField] private bool createWorldMap = true;
    [SerializeField] private bool createTownMap = true;
    [SerializeField] private bool cleanupOldStructure = true;
    [SerializeField] private bool setupWarpPortals = true;
    
    [Header("マップサイズ設定")]
    [SerializeField] private Vector2Int worldMapSize = new Vector2Int(50, 50);
    [SerializeField] private Vector2Int townMapSize = new Vector2Int(30, 30);
    
    [Header("配置設定")]
    [SerializeField] private Vector3 worldPosition = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 townPosition = new Vector3(100, 0, 0);
    
    /// <summary>
    /// WorldとTownマップを作成
    /// </summary>
    [ContextMenu("WorldとTownマップを作成")]
    public void CreateWorldAndTownMaps()
    {
        Debug.Log("=== Cainosマップ作成開始 ===");
        
        try
        {
            // Step 1: 古い構造をクリーンアップ
            if (cleanupOldStructure) CleanupOldStructure();
            
            // Step 2: Worldマップ作成
            if (createWorldMap) CreateWorldMap();
            
            // Step 3: Townマップ作成  
            if (createTownMap) CreateTownMap();
            
            // Step 4: ワープポータル設定
            if (setupWarpPortals) SetupWarpPortals();
            
            // Step 5: プレイヤー設定
            SetupPlayer();
            
            Debug.Log("=== Cainosマップ作成完了 ===");
            Debug.Log("✅ Worldマップ: タイル配置可能");
            Debug.Log("✅ Townマップ: タイル配置可能"); 
            Debug.Log("✅ ワープポータル: World ⇔ Town");
            Debug.Log("Window > 2D > Tile Palette でタイルを配置してください！");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"マップ作成中にエラーが発生: {e.Message}");
        }
    }
    
    /// <summary>
    /// 古いGrid構造を削除
    /// </summary>
    private void CleanupOldStructure()
    {
        Debug.Log("古い構造をクリーンアップ中...");
        
        // 問題のあったGrid構造を削除
        GameObject oldGrid = GameObject.Find("Grid");
        if (oldGrid != null)
        {
            DestroyImmediate(oldGrid);
            Debug.Log("✅ 古いGrid構造を削除");
        }
        
        // Props_Container等も整理
        GameObject oldProps = GameObject.Find("Props_Container");
        if (oldProps != null)
        {
            DestroyImmediate(oldProps);
            Debug.Log("✅ 古いProps_Container削除");
        }
    }
    
    /// <summary>
    /// Worldマップ作成（Cainos公式構造）
    /// </summary>
    private void CreateWorldMap()
    {
        Debug.Log("Worldマップ作成中...");
        
        // LAYER 1 - World (Grid役割)
        GameObject worldLayer = new GameObject("LAYER 1 - World");
        worldLayer.transform.position = worldPosition;
        
        // Grid コンポーネント追加
        var worldGrid = worldLayer.AddComponent<Grid>();
        worldGrid.cellSize = new Vector3(1, 1, 0); // Cainos標準
        
        // World用Tilemap作成
        CreateTilemapStructure(worldLayer, "World");
        
        // Worldプロップス用コンテナ
        GameObject worldProps = new GameObject("Props - World");
        worldProps.transform.SetParent(worldLayer.transform);
        CreatePropsStructure(worldProps);
        
        Debug.Log("✅ Worldマップ作成完了");
    }
    
    /// <summary>
    /// Townマップ作成（Cainos公式構造）
    /// </summary>
    private void CreateTownMap()
    {
        Debug.Log("Townマップ作成中...");
        
        // LAYER 1 - Town (Grid役割)
        GameObject townLayer = new GameObject("LAYER 1 - Town");
        townLayer.transform.position = townPosition;
        
        // Grid コンポーネント追加
        var townGrid = townLayer.AddComponent<Grid>();
        townGrid.cellSize = new Vector3(1, 1, 0); // Cainos標準
        
        // Town用Tilemap作成
        CreateTilemapStructure(townLayer, "Town");
        
        // Townプロップス用コンテナ
        GameObject townProps = new GameObject("Props - Town");
        townProps.transform.SetParent(townLayer.transform);
        CreatePropsStructure(townProps);
        
        Debug.Log("✅ Townマップ作成完了");
    }
    
    /// <summary>
    /// Tilemap構造作成（Cainos公式構造準拠）
    /// </summary>
    private void CreateTilemapStructure(GameObject parent, string mapName)
    {
        // Tilemap（基本レイヤー）
        CreateTilemapLayer(parent, "Tilemap", "CainosLayer1", "CainosLayer1", 0);
        
        // Wall レイヤー（建物・壁用）
        CreateTilemapLayer(parent, $"Layer 1 - Wall - {mapName}", "Default", "CainosLayer1", 1);
        
        // Grass レイヤー（地面用）
        CreateTilemapLayer(parent, $"Layer 1 - Grass - {mapName}", "CainosLayer1", "CainosLayer1", 0);
        
        // 上位レイヤー（2階建て建物用）
        CreateTilemapLayer(parent, $"Layer 2 - Upper - {mapName}", "CainosLayer2", "CainosLayer2", 1);
        CreateTilemapLayer(parent, $"Layer 3 - Roof - {mapName}", "CainosLayer3", "CainosLayer3", 2);
    }
    
    /// <summary>
    /// 個別Tilemapレイヤー作成
    /// </summary>
    private void CreateTilemapLayer(GameObject parent, string layerName, string unityLayer, string sortingLayer, int orderInLayer)
    {
        GameObject tilemapObj = new GameObject(layerName);
        tilemapObj.transform.SetParent(parent.transform);
        tilemapObj.transform.localPosition = Vector3.zero;
        
        // Unity Layer設定
        if (LayerMask.NameToLayer(unityLayer) != -1)
        {
            tilemapObj.layer = LayerMask.NameToLayer(unityLayer);
        }
        else
        {
            Debug.LogWarning($"レイヤー '{unityLayer}' が見つかりません。Defaultを使用");
        }
        
        // Tilemap コンポーネント
        tilemapObj.AddComponent<Tilemap>();
        
        // TilemapRenderer コンポーネント
        var renderer = tilemapObj.AddComponent<TilemapRenderer>();
        
        // ソーティングレイヤー設定（安全チェック付き）
        if (IsSortingLayerValid(sortingLayer))
        {
            renderer.sortingLayerName = sortingLayer;
        }
        else
        {
            Debug.LogWarning($"ソーティングレイヤー '{sortingLayer}' が見つかりません。Defaultを使用");
            renderer.sortingLayerName = "Default";
        }
        renderer.sortingOrder = orderInLayer;
        
        // コリジョン（Grassレイヤーのみ）
        if (layerName.Contains("Grass"))
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
    /// Props構造作成
    /// </summary>
    private void CreatePropsStructure(GameObject parent)
    {
        string[] propsCategories = {
            "Buildings",
            "Stairs", 
            "Nature",
            "Interactive",
            "Decorations"
        };
        
        foreach (string category in propsCategories)
        {
            GameObject categoryObj = new GameObject(category);
            categoryObj.transform.SetParent(parent.transform);
            categoryObj.transform.localPosition = Vector3.zero;
        }
    }
    
    /// <summary>
    /// ワープポータル設定
    /// </summary>
    private void SetupWarpPortals()
    {
        Debug.Log("ワープポータル設定中...");
        
        // SafeAreaManagerがあるか確認
        var areaManager = FindFirstObjectByType<SafeAreaManager>();
        if (areaManager == null)
        {
            GameObject managerObj = new GameObject("SafeAreaManager");
            managerObj.AddComponent<SafeAreaManager>();
            Debug.Log("✅ SafeAreaManager作成");
        }
        
        // World → Town ポータル
        CreateWarpPortal("Portal_World_to_Town", worldPosition + new Vector3(20, 0, 0), "Town");
        
        // Town → World ポータル  
        CreateWarpPortal("Portal_Town_to_World", townPosition + new Vector3(-10, 0, 0), "World");
        
        Debug.Log("✅ ワープポータル設定完了");
    }
    
    /// <summary>
    /// ワープポータル作成
    /// </summary>
    private void CreateWarpPortal(string portalName, Vector3 position, string targetArea)
    {
        GameObject portal = new GameObject(portalName);
        portal.transform.position = position;
        
        // Collider2D
        var collider = portal.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(2f, 2f);
        
        // SafeAreaPortal（あれば）
        var areaPortal = portal.GetComponent<SafeAreaPortal>();
        if (areaPortal == null)
        {
            areaPortal = portal.AddComponent<SafeAreaPortal>();
        }
        
        // 設定
        areaPortal.SetTargetArea(targetArea);
        areaPortal.SetRequireInteraction(true);
        
        Debug.Log($"✅ ワープポータル作成: {portalName} → {targetArea}");
    }
    
    /// <summary>
    /// プレイヤー設定
    /// </summary>
    private void SetupPlayer()
    {
        var player = FindFirstObjectByType<PlayerStats>();
        if (player == null) return;
        
        // プレイヤーをWorldマップ中央に配置
        player.transform.position = worldPosition + new Vector3(10, 10, 0);
        
        // レイヤー設定
        if (LayerMask.NameToLayer("CainosLayer1") != -1)
        {
            player.gameObject.layer = LayerMask.NameToLayer("CainosLayer1");
        }
        
        // CainosPlayerAdapter
        if (player.GetComponent<CainosPlayerAdapter>() == null)
        {
            player.gameObject.AddComponent<CainosPlayerAdapter>();
        }
        
        Debug.Log("✅ プレイヤー設定完了");
    }
    
    /// <summary>
    /// ソーティングレイヤーチェック
    /// </summary>
    private bool IsSortingLayerValid(string layerName)
    {
        try
        {
            var sortingLayers = UnityEngine.SortingLayer.layers;
            foreach (var layer in sortingLayers)
            {
                if (layer.name == layerName) return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// タイル配置ガイド表示
    /// </summary>
    [ContextMenu("タイル配置ガイドを表示")]
    public void ShowTilePlacementGuide()
    {
        Debug.Log("=== Cainosタイル配置ガイド ===");
        Debug.Log("1. Window > 2D > Tile Palette を開く");
        Debug.Log("2. Create New Palette で新しいパレット作成");
        Debug.Log("3. Cainosタイルを追加:");
        Debug.Log("   Assets/Cainos/Pixel Art Top Down - Basic/Tile Palette/");
        Debug.Log("");
        Debug.Log("【Worldマップ配置先】");
        Debug.Log("├── 地面: Layer 1 - Grass - World");
        Debug.Log("├── 壁・建物: Layer 1 - Wall - World");  
        Debug.Log("├── 基本: Tilemap");
        Debug.Log("└── 上層: Layer 2 - Upper - World");
        Debug.Log("");
        Debug.Log("【Townマップ配置先】");
        Debug.Log("├── 地面: Layer 1 - Grass - Town");
        Debug.Log("├── 壁・建物: Layer 1 - Wall - Town");
        Debug.Log("├── 基本: Tilemap"); 
        Debug.Log("└── 上層: Layer 2 - Upper - Town");
        Debug.Log("");
        Debug.Log("【プレファブ配置先】");
        Debug.Log("├── Worldの建物: Props - World/Buildings/");
        Debug.Log("├── Worldの階段: Props - World/Stairs/");
        Debug.Log("├── Townの建物: Props - Town/Buildings/");
        Debug.Log("└── Townの階段: Props - Town/Stairs/");
        Debug.Log("=============================");
    }
} 