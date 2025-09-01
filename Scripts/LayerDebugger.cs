using UnityEngine;

/// <summary>
/// レイヤー設定をリアルタイムでデバッグ
/// </summary>
public class LayerDebugger : MonoBehaviour
{
    [Header("デバッグ設定")]
    [SerializeField] private bool showPlayerInfo = true;
    [SerializeField] private bool showWallInfo = true;
    [SerializeField] private bool showCollisionInfo = true;
    [SerializeField] private float updateInterval = 1.0f;

    private float lastUpdateTime;

    void Update()
    {
        if (Time.time - lastUpdateTime > updateInterval)
        {
            lastUpdateTime = Time.time;
            DebugCurrentState();
        }
    }

    void DebugCurrentState()
    {
        if (showPlayerInfo)
        {
            CheckPlayerLayer();
        }

        if (showWallInfo)
        {
            CheckWallLayers();
        }

        if (showCollisionInfo)
        {
            CheckCollisionSettings();
        }
    }

    /// <summary>
    /// プレイヤーのレイヤー情報を確認
    /// </summary>
    void CheckPlayerLayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            string layerName = LayerMask.LayerToName(player.layer);
            Debug.Log($"🎮 プレイヤーレイヤー: {layerName} ({player.layer})");

            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Debug.Log($"🎮 プレイヤーソーティングレイヤー: {sr.sortingLayerName}");
            }
        }
        else
        {
            Debug.LogWarning("Playerタグのオブジェクトが見つかりません");
        }
    }

    /// <summary>
    /// 壁のレイヤー情報を確認
    /// </summary>
    void CheckWallLayers()
    {
        // Layer 1の壁をチェック
        CheckSpecificLayerObjects("Layer 1", "Wall");
        CheckSpecificLayerObjects("Layer 2", "Wall");
        CheckSpecificLayerObjects("Layer 3", "Wall");
    }

    void CheckSpecificLayerObjects(string layerName, string objectType)
    {
        int layerIndex = LayerMask.NameToLayer(layerName);
        if (layerIndex == -1) return;

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == layerIndex && obj.name.Contains(objectType))
            {
                count++;
                if (count <= 3) // 最初の3個だけ表示
                {
                    Debug.Log($"🧱 {layerName} {objectType}: {obj.name}");
                }
            }
        }

        if (count > 3)
        {
            Debug.Log($"🧱 {layerName} {objectType}: 他{count - 3}個...");
        }
    }

    /// <summary>
    /// コリジョン設定を確認
    /// </summary>
    void CheckCollisionSettings()
    {
        int layer1 = LayerMask.NameToLayer("Layer 1");
        int layer2 = LayerMask.NameToLayer("Layer 2");
        int layer3 = LayerMask.NameToLayer("Layer 3");

        if (layer1 != -1 && layer2 != -1)
        {
            bool collision12 = !Physics2D.GetIgnoreLayerCollision(layer1, layer2);
            Debug.Log($"⚙️ Layer 1 ⇔ Layer 2: {(collision12 ? "衝突する" : "衝突しない")}");
        }

        if (layer1 != -1 && layer3 != -1)
        {
            bool collision13 = !Physics2D.GetIgnoreLayerCollision(layer1, layer3);
            Debug.Log($"⚙️ Layer 1 ⇔ Layer 3: {(collision13 ? "衝突する" : "衝突しない")}");
        }

        if (layer2 != -1 && layer3 != -1)
        {
            bool collision23 = !Physics2D.GetIgnoreLayerCollision(layer2, layer3);
            Debug.Log($"⚙️ Layer 2 ⇔ Layer 3: {(collision23 ? "衝突する" : "衝突しない")}");
        }

        // 同レイヤー内のコリジョン
        if (layer1 != -1)
        {
            bool collision11 = !Physics2D.GetIgnoreLayerCollision(layer1, layer1);
            Debug.Log($"⚙️ Layer 1 ⇔ Layer 1: {(collision11 ? "衝突する" : "衝突しない")}");
        }
    }

    /// <summary>
    /// 手動でチェック実行
    /// </summary>
    [ContextMenu("今すぐデバッグ")]
    public void DebugNow()
    {
        Debug.Log("=== レイヤーデバッグ開始 ===");
        DebugCurrentState();
        Debug.Log("=== レイヤーデバッグ終了 ===");
    }

    /// <summary>
    /// プレイヤー周辺の壁を確認
    /// </summary>
    [ContextMenu("プレイヤー周辺の壁をチェック")]
    public void CheckWallsAroundPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("プレイヤーが見つかりません");
            return;
        }

        Debug.Log("=== プレイヤー周辺の壁チェック ===");
        Debug.Log($"プレイヤー位置: {player.transform.position}");
        Debug.Log($"プレイヤーレイヤー: {LayerMask.LayerToName(player.layer)}");

        // プレイヤー周辺のコライダーを取得
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(player.transform.position, 5f);
        
        foreach (Collider2D col in nearbyColliders)
        {
            if (col.gameObject != player && col.name.Contains("Wall"))
            {
                string layerName = LayerMask.LayerToName(col.gameObject.layer);
                float distance = Vector2.Distance(player.transform.position, col.transform.position);
                Debug.Log($"近くの壁: {col.name} - Layer: {layerName} - 距離: {distance:F2}");
            }
        }
    }
} 