using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Cainosマップ用ヒエラルキー自動作成ツール
/// 推奨構造を一発で作成できます
/// </summary>
public class HierarchySetupTool : MonoBehaviour
{
    [Header("作成設定")]
    [SerializeField] private bool createSystemManagers = true;
    [SerializeField] private bool createWorldMap = true;
    [SerializeField] private bool createGameAreas = true;
    [SerializeField] private bool createPlayerCamera = true;
    [SerializeField] private bool createUISystem = true;
    
    [Header("エリア設定")]
    [SerializeField] private string[] areaNames = {"FirstGame", "NewTown", "Forest"};
    
    [Header("レイヤー設定")] 
    [SerializeField] private string[] sortingLayers = {"Default", "Layer 1", "Layer 2", "Layer 3"};

    /// <summary>
    /// 完全なヒエラルキー構造を作成
    /// </summary>
    [ContextMenu("完全なヒエラルキー構造を作成")]
    public void CreateCompleteHierarchy()
    {
        Debug.Log("HierarchySetupTool: 完全なヒエラルキー構造作成開始");

        try
        {
            // 各フェーズを順番に実行（エラーが起きても継続）
            if (createSystemManagers)
            {
                try { CreateSystemManagers(); }
                catch (System.Exception e) { Debug.LogError($"システムマネージャー作成エラー: {e.Message}"); }
            }
            
            if (createWorldMap)
            {
                try { CreateWorldMap(); }
                catch (System.Exception e) { Debug.LogError($"ワールドマップ作成エラー: {e.Message}"); }
            }
            
            if (createGameAreas)
            {
                try { CreateGameAreas(); }
                catch (System.Exception e) { Debug.LogError($"ゲームエリア作成エラー: {e.Message}"); }
            }
            
            if (createPlayerCamera)
            {
                try { SetupPlayerAndCamera(); }
                catch (System.Exception e) { Debug.LogError($"プレイヤー・カメラ設定エラー: {e.Message}"); }
            }
            
            if (createUISystem)
            {
                try { CreateUISystem(); }
                catch (System.Exception e) { Debug.LogError($"UIシステム作成エラー: {e.Message}"); }
            }
            
            Debug.Log("HierarchySetupTool: ヒエラルキー構造作成完了！");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"HierarchySetupTool: 全体エラー - {e.Message}");
        }
    }
    
    /// <summary>
    /// システムマネージャーの作成
    /// </summary>
    [ContextMenu("システムマネージャーを作成")]
    public void CreateSystemManagers()
    {
        Debug.Log("システムマネージャー作成中...");
        
        // SafeAreaManager
        if (FindFirstObjectByType<SafeAreaManager>() == null)
        {
            GameObject areaManager = new GameObject("SafeAreaManager");
            areaManager.AddComponent<SafeAreaManager>();
            Debug.Log("✅ SafeAreaManager作成完了");
        }
        
        // CainosWarpSetupHelper
        if (FindFirstObjectByType<CainosWarpSetupHelper>() == null)
        {
            GameObject warpHelper = new GameObject("CainosWarpSetupHelper");
            warpHelper.AddComponent<CainosWarpSetupHelper>();
            Debug.Log("✅ CainosWarpSetupHelper作成完了");
        }
        
        Debug.Log("システムマネージャー作成完了");
    }
    
    /// <summary>
    /// ワールドマップ（Tilemap）の作成
    /// </summary>
    [ContextMenu("ワールドマップを作成")]
    public void CreateWorldMap()
    {
        Debug.Log("ワールドマップ作成中...");
        
        // 既存のGridがあるかチェック
        Grid existingGrid = FindFirstObjectByType<Grid>();
        GameObject gridObj;
        
        if (existingGrid == null)
        {
            // 新規Grid作成
            gridObj = new GameObject("Grid");
            gridObj.AddComponent<Grid>();
        }
        else
        {
            gridObj = existingGrid.gameObject;
            Debug.Log("既存のGridを使用します");
        }
        
        // タイルマップレイヤーの作成
        CreateTilemapLayer(gridObj, "Ground_Layer", 0, "Default", 0);
        CreateTilemapLayer(gridObj, "Layer_1", 20, "Layer 1", 1);
        CreateTilemapLayer(gridObj, "Layer_2", 21, "Layer 2", 2);
        CreateTilemapLayer(gridObj, "Layer_3", 22, "Layer 3", 3);
        CreateTilemapLayer(gridObj, "Collision_Layer", 0, "Default", -1);
        
        // Props Container作成
        CreatePropsContainer(gridObj);
        
        Debug.Log("ワールドマップ作成完了");
    }
    
    /// <summary>
    /// タイルマップレイヤーの作成
    /// </summary>
    private void CreateTilemapLayer(GameObject parent, string layerName, int layer, string sortingLayer, int orderInLayer)
    {
        try
        {
            // 既存チェック
            Transform existing = parent.transform.Find(layerName);
            GameObject tilemapObj;
            
            if (existing != null)
            {
                Debug.Log($"既存の{layerName}を更新します");
                tilemapObj = existing.gameObject;
            }
            else
            {
                tilemapObj = new GameObject(layerName);
                tilemapObj.transform.SetParent(parent.transform);
            }
            
            // レイヤー設定
            if (layer > 0)
            {
                tilemapObj.layer = layer;
            }
            
            // Tilemapコンポーネント（既存チェック）
            var tilemap = tilemapObj.GetComponent<Tilemap>();
            if (tilemap == null)
            {
                tilemap = tilemapObj.AddComponent<Tilemap>();
            }
            
            var renderer = tilemapObj.GetComponent<TilemapRenderer>();
            if (renderer == null)
            {
                renderer = tilemapObj.AddComponent<TilemapRenderer>();
            }
            
            // Renderer設定
            if (!string.IsNullOrEmpty(sortingLayer))
            {
                renderer.sortingLayerName = sortingLayer;
                renderer.sortingOrder = orderInLayer;
            }
            
            // 地面とコリジョンレイヤーにはColliderを追加（既存チェック）
            if (layerName.Contains("Ground") || layerName.Contains("Collision"))
            {
                var collider = tilemapObj.GetComponent<TilemapCollider2D>();
                if (collider == null)
                {
                    collider = tilemapObj.AddComponent<TilemapCollider2D>();
                    collider.compositeOperation = Collider2D.CompositeOperation.Merge;
                }
                
                var composite = tilemapObj.GetComponent<CompositeCollider2D>();
                if (composite == null)
                {
                    composite = tilemapObj.AddComponent<CompositeCollider2D>();
                    composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
                }
                
                // Rigidbody2Dも追加（Static設定）
                var rb = tilemapObj.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = tilemapObj.AddComponent<Rigidbody2D>();
                    rb.bodyType = RigidbodyType2D.Static;
                }
            }
            
            Debug.Log($"✅ {layerName} 作成/更新完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ {layerName} 作成エラー: {e.Message}");
        }
    }
    
    /// <summary>
    /// Props Containerの作成
    /// </summary>
    private void CreatePropsContainer(GameObject parent)
    {
        GameObject propsContainer = CreateOrGetChildObject(parent, "Props_Container");
        
        CreateOrGetChildObject(propsContainer, "Buildings");
        CreateOrGetChildObject(propsContainer, "Nature_Objects");
        CreateOrGetChildObject(propsContainer, "Stairs_Objects");
        CreateOrGetChildObject(propsContainer, "Interactive_Objects");
        
        Debug.Log("✅ Props Container作成完了");
    }
    
    /// <summary>
    /// ゲームエリアの作成
    /// </summary>
    [ContextMenu("ゲームエリアを作成")]
    public void CreateGameAreas()
    {
        Debug.Log("ゲームエリア作成中...");
        
        // エリアコンテナ作成
        GameObject areasContainer = CreateOrGetRootObject("=== GAME AREAS ===");
        
        foreach (string areaName in areaNames)
        {
            CreateGameArea(areasContainer, areaName);
        }
        
        Debug.Log("ゲームエリア作成完了");
    }
    
    /// <summary>
    /// 個別ゲームエリアの作成
    /// </summary>
    private void CreateGameArea(GameObject parent, string areaName)
    {
        GameObject areaObj = CreateOrGetChildObject(parent, $"Area_{areaName}");
        
        // エリア内構造作成
        CreateOrGetChildObject(areaObj, "Ground_Objects");
        CreateOrGetChildObject(areaObj, "Buildings");
        CreateOrGetChildObject(areaObj, "NPCs");
        CreateOrGetChildObject(areaObj, "Enemies");
        CreateOrGetChildObject(areaObj, "WarpPortals");
        CreateOrGetChildObject(areaObj, "Cainos_Stairs");
        CreateOrGetChildObject(areaObj, "Interactive_Objects");
        
        Debug.Log($"✅ Area_{areaName} 作成完了");
    }
    
    /// <summary>
    /// プレイヤーとカメラの設定
    /// </summary>
    [ContextMenu("プレイヤーとカメラを設定")]
    public void SetupPlayerAndCamera()
    {
        Debug.Log("プレイヤーとカメラ設定中...");
        
        // プレイヤー設定
        var playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            // CainosPlayerAdapterの追加
            if (playerStats.GetComponent<CainosPlayerAdapter>() == null)
            {
                playerStats.gameObject.AddComponent<CainosPlayerAdapter>();
            }
            
            // レイヤー設定
            playerStats.gameObject.layer = LayerMask.NameToLayer("Layer 1");
            
            // SpriteRenderer設定
            var renderer = playerStats.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = "Layer 1";
                renderer.sortingOrder = 10;
            }
            
            Debug.Log("✅ プレイヤー設定完了");
        }
        else
        {
            Debug.LogWarning("PlayerStatsが見つかりません");
        }
        
        // カメラ設定
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 3.5f;  // 3.5に変更
            mainCamera.transform.position = new Vector3(0, 0, -10);
            
            Debug.Log("✅ カメラ設定完了");
        }
        
        Debug.Log("プレイヤーとカメラ設定完了");
    }
    
    /// <summary>
    /// UIシステムの作成
    /// </summary>
    [ContextMenu("UIシステムを作成")]
    public void CreateUISystem()
    {
        Debug.Log("UIシステム作成中...");
        
        // メインCanvas確認
        Canvas mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas_Main");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        
        // EventSystem確認
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        Debug.Log("✅ UIシステム作成完了");
    }
    
    /// <summary>
    /// ルートオブジェクトの作成または取得
    /// </summary>
    private GameObject CreateOrGetRootObject(string name)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null) return existing;
        
        return new GameObject(name);
    }
    
    /// <summary>
    /// 子オブジェクトの作成または取得
    /// </summary>
    private GameObject CreateOrGetChildObject(GameObject parent, string name)
    {
        Transform existing = parent.transform.Find(name);
        if (existing != null) return existing.gameObject;
        
        GameObject newObj = new GameObject(name);
        newObj.transform.SetParent(parent.transform);
        return newObj;
    }
    
    /// <summary>
    /// クリーンアップ：空のオブジェクトを削除
    /// </summary>
    [ContextMenu("空のオブジェクトをクリーンアップ")]
    public void CleanupEmptyObjects()
    {
        Debug.Log("空のオブジェクトクリーンアップ中...");
        
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int cleanedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // 子がなく、重要なコンポーネントもない空のオブジェクト
            if (obj.transform.childCount == 0 && 
                obj.GetComponents<Component>().Length <= 1 && // Transform以外のコンポーネントなし
                !obj.name.Contains("Area_") && 
                !obj.name.Contains("Manager") &&
                !obj.name.Contains("Camera") &&
                !obj.name.Contains("Player"))
            {
                if (Application.isPlaying)
                {
                    Destroy(obj);
                }
                else
                {
                    DestroyImmediate(obj);
                }
                cleanedCount++;
            }
        }
        
        Debug.Log($"✅ {cleanedCount}個の空オブジェクトを削除しました");
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 2f);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, "Hierarchy Setup Tool");
        #endif
    }
} 