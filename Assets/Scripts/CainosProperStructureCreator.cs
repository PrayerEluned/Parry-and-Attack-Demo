using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Cainos SC Demo構造に基づいた正しいWorldとTownマップ作成
/// 3層システム（LAYER 1, 2, 3）での実装
/// </summary>
public class CainosProperStructureCreator : MonoBehaviour
{
    [Header("マップ作成設定")]
    [SerializeField] private bool createWorldMap = true;
    [SerializeField] private bool createTownMap = true;
    [SerializeField] private bool cleanupOldStructure = true;
    [SerializeField] private bool setupPlayer = true;
    
    [Header("配置設定")]
    [SerializeField] private Vector3 worldPosition = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 townPosition = new Vector3(100, 0, 0);
    
    /// <summary>
    /// Cainos正式構造でWorldとTownを作成
    /// </summary>
    [ContextMenu("Cainos正式構造でマップ作成")]
    public void CreateCainosProperMaps()
    {
        Debug.Log("=== Cainos正式構造マップ作成開始 ===");
        
        try
        {
            // 古い構造削除
            if (cleanupOldStructure) CleanupOldStructure();
            
            // RENDERING作成
            CreateRenderingSystem();
            
            // SCENE作成
            GameObject sceneRoot = CreateSceneRoot();
            
            // World用 3層構造作成
            if (createWorldMap) CreateWorldLayers(sceneRoot);
            
            // Town用 3層構造作成  
            if (createTownMap) CreateTownLayers(sceneRoot);
            
            // プレイヤー設定
            if (setupPlayer) SetupPlayerForCainos();
            
            Debug.Log("=== Cainos正式構造マップ作成完了 ===");
            Debug.Log("✅ 3層システム（LAYER 1, 2, 3）準備完了");
            Debug.Log("✅ 階段システム対応完了");
            Debug.Log("✅ Cainos公式構造準拠");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"マップ作成エラー: {e.Message}");
        }
    }
    
    /// <summary>
    /// 古い間違った構造を削除
    /// </summary>
    private void CleanupOldStructure()
    {
        string[] oldObjects = {
            "Grid", "Props_Container", "LAYER 1 - World", "LAYER 1 - Town",
            "Area_FirstGame", "Area_NewTown", "Area_Forest"
        };
        
        foreach (string objName in oldObjects)
        {
            GameObject oldObj = GameObject.Find(objName);
            if (oldObj != null)
            {
                DestroyImmediate(oldObj);
                Debug.Log($"✅ 削除: {objName}");
            }
        }
        
        // 古いワープシステム削除
        CleanupOldWarpSystem();
    }
    
    /// <summary>
    /// 古いワープシステムを一括削除
    /// </summary>
    [ContextMenu("古いワープシステムを削除")]
    public void CleanupOldWarpSystem()
    {
        Debug.Log("=== 古いワープシステム削除開始 ===");
        
        // SafeAreaPortalコンポーネント削除
        var portals = FindObjectsByType<SafeAreaPortal>(FindObjectsSortMode.None);
        foreach (var portal in portals)
        {
            Debug.Log($"✅ SafeAreaPortal削除: {portal.gameObject.name}");
            DestroyImmediate(portal.gameObject);
        }
        
        // SafeAreaManagerオブジェクト削除
        var areaManagers = FindObjectsByType<SafeAreaManager>(FindObjectsSortMode.None);
        foreach (var manager in areaManagers)
        {
            Debug.Log($"✅ SafeAreaManager削除: {manager.gameObject.name}");
            DestroyImmediate(manager.gameObject);
        }
        
        // CainosWarpSetupHelperオブジェクト削除
        var warpHelpers = FindObjectsByType<CainosWarpSetupHelper>(FindObjectsSortMode.None);
        foreach (var helper in warpHelpers)
        {
            Debug.Log($"✅ CainosWarpSetupHelper削除: {helper.gameObject.name}");
            DestroyImmediate(helper.gameObject);
        }
        
        // 古いAreaPortal削除
        var oldPortals = FindObjectsByType<AreaPortal>(FindObjectsSortMode.None);
        foreach (var oldPortal in oldPortals)
        {
            Debug.Log($"✅ AreaPortal削除: {oldPortal.gameObject.name}");
            DestroyImmediate(oldPortal.gameObject);
        }
        
        Debug.Log("=== 古いワープシステム削除完了 ===");
        Debug.Log("✅ エラーの原因となっていた古いシステムを削除しました");
        Debug.Log("✅ 'Cainos正式構造でマップ作成' を実行してください");
    }
    
    /// <summary>
    /// RENDERING システム作成
    /// </summary>
    private void CreateRenderingSystem()
    {
        GameObject rendering = GameObject.Find("RENDERING");
        if (rendering == null)
        {
            rendering = new GameObject("RENDERING");
            Debug.Log("✅ RENDERING作成");
        }
        
        // Main Camera確認
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.transform.parent != rendering.transform)
        {
            mainCam.transform.SetParent(rendering.transform);
            Debug.Log("✅ Main CameraをRENDERINGに移動");
        }
    }
    
    /// <summary>
    /// SCENE ルート作成
    /// </summary>
    private GameObject CreateSceneRoot()
    {
        GameObject scene = GameObject.Find("SCENE");
        if (scene == null)
        {
            scene = new GameObject("SCENE");
            Debug.Log("✅ SCENE作成");
        }
        return scene;
    }
    
    /// <summary>
    /// World用3層構造作成
    /// </summary>
    private void CreateWorldLayers(GameObject sceneRoot)
    {
        Debug.Log("World用3層構造作成中...");
        
        // World LAYER 3
        GameObject worldLayer3 = CreateLayerStructure(sceneRoot, "LAYER 3 - World", worldPosition, "Layer 3", false);
        CreateMapContent(worldLayer3, "World", "Layer 3");
        
        // World LAYER 2  
        GameObject worldLayer2 = CreateLayerStructure(sceneRoot, "LAYER 2 - World", worldPosition, "Layer 2", false);
        CreateMapContent(worldLayer2, "World", "Layer 2");
        
        // World LAYER 1（最詳細）
        GameObject worldLayer1 = CreateLayerStructure(sceneRoot, "LAYER 1 - World", worldPosition, "Layer 1", true);
        CreateMapContent(worldLayer1, "World", "Layer 1");
        
        Debug.Log("✅ World 3層構造作成完了");
    }
    
    /// <summary>
    /// Town用3層構造作成
    /// </summary>
    private void CreateTownLayers(GameObject sceneRoot)
    {
        Debug.Log("Town用3層構造作成中...");
        
        // Town LAYER 3
        GameObject townLayer3 = CreateLayerStructure(sceneRoot, "LAYER 3 - Town", townPosition, "Layer 3", false);
        CreateMapContent(townLayer3, "Town", "Layer 3");
        
        // Town LAYER 2
        GameObject townLayer2 = CreateLayerStructure(sceneRoot, "LAYER 2 - Town", townPosition, "Layer 2", false);
        CreateMapContent(townLayer2, "Town", "Layer 2");
        
        // Town LAYER 1（最詳細）
        GameObject townLayer1 = CreateLayerStructure(sceneRoot, "LAYER 1 - Town", townPosition, "Layer 1", true);
        CreateMapContent(townLayer1, "Town", "Layer 1");
        
        Debug.Log("✅ Town 3層構造作成完了");
    }
    
    /// <summary>
    /// 各レイヤー構造作成（LAYER 1, 2, 3）
    /// </summary>
    private GameObject CreateLayerStructure(GameObject parent, string layerName, Vector3 position, string unityLayer, bool isDetailLayer)
    {
        GameObject layer = new GameObject(layerName);
        layer.transform.SetParent(parent.transform);
        layer.transform.position = position;
        
        // レイヤー設定
        int layerIndex = LayerMask.NameToLayer(unityLayer);
        if (layerIndex != -1)
        {
            layer.layer = layerIndex;
        }
        
        // Grid コンポーネント（必須）
        var grid = layer.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);
        
        if (isDetailLayer)
        {
            // LAYER 1用：詳細構造
            CreateDetailedLayerContent(layer, layerName, unityLayer);
        }
        else
        {
            // LAYER 2, 3用：基本構造
            CreateBasicLayerContent(layer, layerName, unityLayer);
        }
        
        Debug.Log($"✅ {layerName} 作成完了");
        return layer;
    }
    
    /// <summary>
    /// LAYER 1用詳細構造作成
    /// </summary>
    private void CreateDetailedLayerContent(GameObject parent, string layerName, string unityLayer)
    {
        string mapType = layerName.Contains("World") ? "World" : "Town";
        
        // Props
        CreateChildObject(parent, "Props", unityLayer);
        
        // Grass
        CreateTilemapObject(parent, "Grass", unityLayer, "Layer 1", 0);
        
        // Tilemap
        CreateTilemapObject(parent, "Tilemap", unityLayer, "Layer 1", 1);
        
        // Layer 1 - Wall
        CreateTilemapObject(parent, $"Layer 1 - Wall - {mapType}", "Default", "Layer 1", 2);
        
        // Layer 1 - Wall Shadow
        CreateTilemapObject(parent, $"Layer 1 - Wall Shadow - {mapType}", "Default", "Layer 1", 1);
        
        // Layer 1 - Stone Ground
        CreateTilemapObject(parent, $"Layer 1 - Stone Ground - {mapType}", unityLayer, "Layer 1", 0);
        
        // Layer 1 - Grass
        CreateTilemapObject(parent, $"Layer 1 - Grass - {mapType}", unityLayer, "Layer 1", 0);
    }
    
    /// <summary>
    /// LAYER 2, 3用基本構造作成
    /// </summary>
    private void CreateBasicLayerContent(GameObject parent, string layerName, string unityLayer)
    {
        string sortingLayer = unityLayer; // Layer 2 or Layer 3
        
        // Props
        CreateChildObject(parent, "Props", unityLayer);
        
        // Tilemap
        CreateTilemapObject(parent, "Tilemap", unityLayer, sortingLayer, 1);
        
        // Grass
        CreateTilemapObject(parent, "Grass", unityLayer, sortingLayer, 0);
    }
    
    /// <summary>
    /// Tilemapオブジェクト作成
    /// </summary>
    private void CreateTilemapObject(GameObject parent, string objName, string unityLayer, string sortingLayer, int orderInLayer)
    {
        GameObject tilemapObj = new GameObject(objName);
        tilemapObj.transform.SetParent(parent.transform);
        tilemapObj.transform.localPosition = Vector3.zero;
        
        // Unity Layer設定
        int layerIndex = LayerMask.NameToLayer(unityLayer);
        if (layerIndex != -1)
        {
            tilemapObj.layer = layerIndex;
        }
        
        // Tilemap コンポーネント
        tilemapObj.AddComponent<Tilemap>();
        
        // TilemapRenderer
        var renderer = tilemapObj.AddComponent<TilemapRenderer>();
        
        // ソーティングレイヤー設定（安全）
        if (IsSortingLayerValid(sortingLayer))
        {
            renderer.sortingLayerName = sortingLayer;
        }
        else
        {
            renderer.sortingLayerName = "Default";
            Debug.LogWarning($"ソーティングレイヤー '{sortingLayer}' 未設定。Defaultを使用");
        }
        renderer.sortingOrder = orderInLayer;
        
        // Cainosマテリアル自動設定
        SetCainosMaterial(renderer, objName);
        
        // Grassレイヤーにはコライダー追加
        if (objName.Contains("Grass"))
        {
            var collider = tilemapObj.AddComponent<TilemapCollider2D>();
            collider.compositeOperation = Collider2D.CompositeOperation.Merge;
            
            var composite = tilemapObj.AddComponent<CompositeCollider2D>();
            composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
            
            var rb = tilemapObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }
    }
    
    /// <summary>
    /// Cainosマテリアル自動設定
    /// </summary>
    private void SetCainosMaterial(TilemapRenderer renderer, string objName)
    {
        string materialPath = "";
        
        // オブジェクト名に基づいてマテリアル選択
        if (objName.Contains("Wall Shadow"))
        {
            materialPath = "Assets/Cainos/Pixel Art Top Down - Basic/Material/MT Shadow";
        }
        else if (objName.Contains("Tilemap") || objName.Contains("Wall") || 
                 objName.Contains("Stone Ground") || objName.Contains("Grass"))
        {
            materialPath = "Assets/Cainos/Pixel Art Top Down - Basic/Material/MT Tileset";
        }
        
        // マテリアル読み込み・設定
        if (!string.IsNullOrEmpty(materialPath))
        {
#if UNITY_EDITOR
            var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(materialPath + ".mat");
            if (material != null)
            {
                renderer.material = material;
                Debug.Log($"✅ {objName} に {material.name} マテリアル設定");
            }
            else
            {
                Debug.LogWarning($"⚠️ マテリアルが見つかりません: {materialPath}.mat");
            }
#endif
        }
    }
    
    /// <summary>
    /// 子オブジェクト作成（Props用）
    /// </summary>
    private void CreateChildObject(GameObject parent, string objName, string unityLayer)
    {
        GameObject child = new GameObject(objName);
        child.transform.SetParent(parent.transform);
        child.transform.localPosition = Vector3.zero;
        
        // Unity Layer設定
        int layerIndex = LayerMask.NameToLayer(unityLayer);
        if (layerIndex != -1)
        {
            child.layer = layerIndex;
        }
        
        // Props用子カテゴリー作成
        if (objName == "Props")
        {
            CreateChildObject(child, "Buildings", unityLayer);
            CreateChildObject(child, "Stairs", unityLayer);
            CreateChildObject(child, "Nature", unityLayer);
            CreateChildObject(child, "Interactive", unityLayer);
        }
    }
    
    /// <summary>
    /// プレイヤーをCainos対応設定
    /// </summary>
    private void SetupPlayerForCainos()
    {
        var player = FindFirstObjectByType<PlayerStats>();
        if (player == null)
        {
            Debug.LogWarning("PlayerStatsが見つかりません");
            return;
        }
        
        // プレイヤーをWorldマップに配置
        player.transform.position = worldPosition + new Vector3(10, 10, 0);
        
        // 初期レイヤー設定（LAYER 1）
        player.gameObject.layer = LayerMask.NameToLayer("Layer 1");
        
        // SpriteRenderer設定
        var renderer = player.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            if (IsSortingLayerValid("Layer 1"))
            {
                renderer.sortingLayerName = "Layer 1";
            }
            renderer.sortingOrder = 10;
        }
        
        // CainosPlayerAdapter追加
        if (player.GetComponent<CainosPlayerAdapter>() == null)
        {
            player.gameObject.AddComponent<CainosPlayerAdapter>();
        }
        
        Debug.Log("✅ プレイヤーCainos対応設定完了");
    }
    
    /// <summary>
    /// ソーティングレイヤー存在チェック
    /// </summary>
    private bool IsSortingLayerValid(string layerName)
    {
        try
        {
            var layers = UnityEngine.SortingLayer.layers;
            foreach (var layer in layers)
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
    /// 使用方法ガイド表示
    /// </summary>
    [ContextMenu("Cainos正式構造使用ガイド")]
    public void ShowUsageGuide()
    {
        Debug.Log("=== Cainos正式構造使用ガイド ===");
        Debug.Log("");
        Debug.Log("【作成される構造】");
        Debug.Log("RENDERING");
        Debug.Log("├── Main Camera");
        Debug.Log("SCENE");
        Debug.Log("├── LAYER 3 - World/Town (最上階)");
        Debug.Log("│   ├── Props, Tilemap, Grass");
        Debug.Log("├── LAYER 2 - World/Town (2階)");
        Debug.Log("│   ├── Props, Tilemap, Grass");
        Debug.Log("└── LAYER 1 - World/Town (地上階)");
        Debug.Log("    ├── Props, Grass, Tilemap");
        Debug.Log("    ├── Layer 1 - Wall");
        Debug.Log("    ├── Layer 1 - Wall Shadow");
        Debug.Log("    ├── Layer 1 - Stone Ground");
        Debug.Log("    └── Layer 1 - Grass");
        Debug.Log("");
        Debug.Log("【タイル配置方法】");
        Debug.Log("├── 地面: Layer 1 - Grass");
        Debug.Log("├── 石床: Layer 1 - Stone Ground");
        Debug.Log("├── 壁: Layer 1 - Wall");
        Debug.Log("├── 影: Layer 1 - Wall Shadow");
        Debug.Log("└── 2階以上: LAYER 2/3のTilemap");
        Debug.Log("");
        Debug.Log("【階段プレファブ配置】");
        Debug.Log("階段プレファブをProps/Stairsに配置すると");
        Debug.Log("StairsLayerTriggerで自動的にレイヤー切り替え！");
        Debug.Log("=============================");
    }

    private void CreateMapContent(GameObject parent, string mapType, string unityLayer)
    {
        // タイルマップを作成
        CreateTilemapObject(parent, "Grass", unityLayer, "Layer 1", 0);
        CreateTilemapObject(parent, "Tilemap", unityLayer, "Layer 1", 1);
        CreateTilemapObject(parent, $"Layer 1 - Wall - {mapType}", "Default", "Layer 1", 2);
        CreateTilemapObject(parent, $"Layer 1 - Wall Shadow - {mapType}", "Default", "Layer 1", 1);
        CreateTilemapObject(parent, $"Layer 1 - Stone Ground - {mapType}", unityLayer, "Layer 1", 0);
        CreateTilemapObject(parent, $"Layer 1 - Grass - {mapType}", unityLayer, "Layer 1", 0);

        if (unityLayer == "Layer 2" || unityLayer == "Layer 3")
        {
            string sortingLayer = unityLayer; // Layer 2 or Layer 3
            CreateTilemapObject(parent, $"Layer 2/3 - Wall - {mapType}", "Default", sortingLayer, 2);
            CreateTilemapObject(parent, $"Layer 2/3 - Wall Shadow - {mapType}", "Default", sortingLayer, 1);
            CreateTilemapObject(parent, $"Layer 2/3 - Ground - {mapType}", unityLayer, sortingLayer, 0);
        }
    }

    private void SetupPlayer(GameObject sceneRoot)
    {
        // 既存のプレイヤーを探す
        PlayerStats player = FindObjectOfType<PlayerStats>();
        if (player == null)
        {
            // プレイヤーがいない場合は警告
            Debug.LogWarning("シーンにPlayerが見つかりません。Playerプレハブを配置してください。");
            return;
        }

        // プレイヤーをSceneRootの子にする
        player.transform.SetParent(sceneRoot.transform, true);

        // レイヤーとソーティングレイヤーを設定
        player.gameObject.layer = LayerMask.NameToLayer("Layer 1");

        SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            if (IsSortingLayerValid("Layer 1"))
            {
                renderer.sortingLayerName = "Layer 1";
                renderer.sortingOrder = 5; // 他のオブジェクトより手前に表示
            }
            else
            {
                Debug.LogWarning("Sorting Layer 'Layer 1' が見つかりません。");
            }
        }
    }
} 