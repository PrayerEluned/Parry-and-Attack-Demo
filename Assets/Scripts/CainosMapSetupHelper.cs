using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Cainosマップ作成をサポートするヘルパークラス
/// </summary>
public class CainosMapSetupHelper : MonoBehaviour
{
    [Header("Cainos タイルマップレイヤー")]
    [SerializeField] private Tilemap groundLayer;     // 地上レイヤー
    [SerializeField] private Tilemap layer1;          // レイヤー1
    [SerializeField] private Tilemap layer2;          // レイヤー2
    [SerializeField] private Tilemap layer3;          // レイヤー3
    
    [Header("コライダー設定")]
    [SerializeField] private bool enableColliders = true;
    
    [Header("プレファブ配置位置")]
    [SerializeField] private Transform propsParent;
    
    private void Start()
    {
        SetupTilemapLayers();
    }

    /// <summary>
    /// タイルマップレイヤーを適切に設定
    /// </summary>
    [ContextMenu("タイルマップレイヤーをセットアップ")]
    public void SetupTilemapLayers()
    {
        // 地上レイヤー設定
        if (groundLayer != null)
        {
            SetupSingleLayer(groundLayer, "Layer 1", "Layer 1", 0);
        }
        
        // レイヤー1設定
        if (layer1 != null)
        {
            SetupSingleLayer(layer1, "Layer 1", "Layer 1", 1);
        }
        
        // レイヤー2設定
        if (layer2 != null)
        {
            SetupSingleLayer(layer2, "Layer 2", "Layer 2", 2);
        }
        
        // レイヤー3設定
        if (layer3 != null)
        {
            SetupSingleLayer(layer3, "Layer 3", "Layer 3", 3);
        }
        
        Debug.Log("Cainos タイルマップレイヤーのセットアップが完了しました");
    }

    /// <summary>
    /// 単一レイヤーの設定
    /// </summary>
    private void SetupSingleLayer(Tilemap tilemap, string layerName, string sortingLayerName, int sortingOrder)
    {
        GameObject tilemapObject = tilemap.gameObject;
        
        // レイヤー設定
        int layerIndex = LayerMask.NameToLayer(layerName);
        if (layerIndex != -1)
        {
            tilemapObject.layer = layerIndex;
        }
        else
        {
            Debug.LogWarning($"レイヤー '{layerName}' が見つかりません。Cainos Setup Toolで設定してください。");
        }
        
        // TilemapRenderer設定
        TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;
        }
        
        // Collider設定
        TilemapCollider2D collider = tilemap.GetComponent<TilemapCollider2D>();
        if (enableColliders)
        {
            if (collider == null)
            {
                collider = tilemapObject.AddComponent<TilemapCollider2D>();
            }
            collider.usedByComposite = true;
            
            // CompositeCollider2Dも追加
            Rigidbody2D rb = tilemapObject.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = tilemapObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
            }
            
            CompositeCollider2D composite = tilemapObject.GetComponent<CompositeCollider2D>();
            if (composite == null)
            {
                composite = tilemapObject.AddComponent<CompositeCollider2D>();
            }
        }
        else if (collider != null)
        {
            DestroyImmediate(collider);
        }
        
        Debug.Log($"{tilemap.name} を {layerName} / {sortingLayerName} として設定しました");
    }

    /// <summary>
    /// 自動的にタイルマップレイヤーを作成
    /// </summary>
    [ContextMenu("自動レイヤー作成")]
    public void CreateCainosLayers()
    {
        Transform gridTransform = transform;
        
        // 各レイヤーを作成
        CreateTilemapLayer("Ground_Layer", "Layer 1", "Layer 1", 0, gridTransform);
        CreateTilemapLayer("Layer_1", "Layer 1", "Layer 1", 1, gridTransform);
        CreateTilemapLayer("Layer_2", "Layer 2", "Layer 2", 2, gridTransform);
        CreateTilemapLayer("Layer_3", "Layer 3", "Layer 3", 3, gridTransform);
        
        // プロップ配置用の親オブジェクトも作成
        if (propsParent == null)
        {
            GameObject propsObject = new GameObject("Props");
            propsObject.transform.SetParent(gridTransform);
            propsParent = propsObject.transform;
        }
        
        Debug.Log("Cainos レイヤー構造を自動作成しました");
    }

    /// <summary>
    /// 単一タイルマップレイヤーを作成
    /// </summary>
    private GameObject CreateTilemapLayer(string name, string layerName, string sortingLayerName, int sortingOrder, Transform parent)
    {
        // GameObject作成
        GameObject tilemapObject = new GameObject(name);
        tilemapObject.transform.SetParent(parent);
        
        // 必要なコンポーネントを追加
        Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
        
        // 設定を適用
        SetupSingleLayer(tilemap, layerName, sortingLayerName, sortingOrder);
        
        return tilemapObject;
    }

    /// <summary>
    /// Cainos プレファブを配置するヘルパー
    /// </summary>
    public void PlaceCainosPrefab(GameObject prefab, Vector3 position, string targetLayerName = null)
    {
        if (prefab == null)
        {
            Debug.LogError("プレファブが指定されていません");
            return;
        }
        
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        
        // プロップ親の下に配置
        if (propsParent != null)
        {
            instance.transform.SetParent(propsParent);
        }
        
        // レイヤー設定
        if (!string.IsNullOrEmpty(targetLayerName))
        {
            SetObjectLayer(instance, targetLayerName);
        }
        
        Debug.Log($"{prefab.name} を {position} に配置しました");
    }

    /// <summary>
    /// オブジェクトのレイヤーを設定
    /// </summary>
    private void SetObjectLayer(GameObject obj, string layerName)
    {
        int layerIndex = LayerMask.NameToLayer(layerName);
        if (layerIndex != -1)
        {
            obj.layer = layerIndex;
            
            // 子オブジェクトも同じレイヤーに設定
            foreach (Transform child in obj.transform)
            {
                SetObjectLayer(child.gameObject, layerName);
            }
        }
    }

    /// <summary>
    /// 現在のマップ構造を表示
    /// </summary>
    [ContextMenu("マップ構造を表示")]
    public void ShowMapStructure()
    {
        Debug.Log("=== Cainos マップ構造 ===");
        
        Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>();
        foreach (Tilemap tilemap in tilemaps)
        {
            TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
            string layerInfo = $"{tilemap.name}: Layer={LayerMask.LayerToName(tilemap.gameObject.layer)}, SortingLayer={renderer.sortingLayerName}, Order={renderer.sortingOrder}";
            Debug.Log(layerInfo);
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;

        // Ground Layer
        SetupSingleLayer(groundLayer, "Layer 1", "Layer 1", 0);
        
        // Layer 1
        SetupSingleLayer(layer1, "Layer 1", "Layer 1", 1);
        
        // Layer 2
        SetupSingleLayer(layer2, "Layer 2", "Layer 2", 2);
    }

    [ContextMenu("自動で3層構造を作成")]
    private void CreateHierarchy()
    {
        Transform gridTransform = transform;

        // 各レイヤーを作成
        CreateTilemapLayer("Ground_Layer", "Layer 1", "Layer 1", 0, gridTransform);
        CreateTilemapLayer("Layer_1", "Layer 1", "Layer 1", 1, gridTransform);
        CreateTilemapLayer("Layer_2", "Layer 2", "Layer 2", 2, gridTransform);
        CreateTilemapLayer("Layer_3", "Layer 3", "Layer 3", 3, gridTransform);
    }
} 