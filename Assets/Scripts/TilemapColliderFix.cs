using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// TilemapCollider2DとCompositeCollider2Dの統合問題を修正するツール
/// </summary>
public class TilemapColliderFix : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool autoFixOnStart = true;

    /// <summary>
    /// Tilemap Collider問題を修正
    /// </summary>
    [ContextMenu("Tilemap Collider問題を修正")]
    public void FixTilemapColliderIssues()
    {
        Debug.Log("🔧 === Tilemap Collider修正開始 ===");

        // 1. 全てのTilemapCollider2Dを検索
        TilemapCollider2D[] tilemapColliders = FindObjectsByType<TilemapCollider2D>(FindObjectsSortMode.None);
        Debug.Log($"🔍 TilemapCollider2D発見: {tilemapColliders.Length}個");

        foreach (TilemapCollider2D tilemapCol in tilemapColliders)
        {
            FixTilemapCollider(tilemapCol);
        }

        // 2. 全てのCompositeCollider2Dを検索
        CompositeCollider2D[] compositeColliders = FindObjectsByType<CompositeCollider2D>(FindObjectsSortMode.None);
        Debug.Log($"🔍 CompositeCollider2D発見: {compositeColliders.Length}個");

        foreach (CompositeCollider2D compositeCol in compositeColliders)
        {
            FixCompositeCollider(compositeCol);
        }

        Debug.Log("✅ Tilemap Collider修正完了");
    }

    /// <summary>
    /// TilemapCollider2Dを修正
    /// </summary>
    private void FixTilemapCollider(TilemapCollider2D tilemapCol)
    {
        GameObject obj = tilemapCol.gameObject;
        string layerName = LayerMask.LayerToName(obj.layer);

        Debug.Log($"🔧 修正中: {obj.name} ({layerName})");

        // CompositeCollider2Dがあるか確認
        CompositeCollider2D compositeCol = obj.GetComponent<CompositeCollider2D>();

        if (compositeCol != null)
        {
            Debug.Log($"  - CompositeCollider2D検出");

            // TilemapCollider2DをCompositeに統合
            if (!tilemapCol.usedByComposite)
            {
                tilemapCol.usedByComposite = true;
                Debug.Log($"  ✅ TilemapCollider2D.usedByComposite = true");
            }

            // Rigidbody2Dが必要
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = obj.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                rb.gravityScale = 0f;
                Debug.Log($"  ✅ Rigidbody2D追加 (Static)");
            }
            else
            {
                // 既存のRigidbody2Dを適切に設定
                if (rb.bodyType != RigidbodyType2D.Static)
                {
                    rb.bodyType = RigidbodyType2D.Static;
                    Debug.Log($"  🔧 Rigidbody2D.bodyType = Static");
                }
            }

            // CompositeCollider2Dの設定を確認
            CheckCompositeColliderSettings(compositeCol);
        }
        else
        {
            Debug.Log($"  - CompositeCollider2D未検出");

            // usedByCompositeが有効になっているが、CompositeCollider2Dがない場合
            if (tilemapCol.usedByComposite)
            {
                Debug.Log($"  ⚠️ usedByComposite=trueだが、CompositeCollider2Dが存在しない");
                
                // CompositeCollider2Dを追加
                CompositeCollider2D newComposite = obj.AddComponent<CompositeCollider2D>();
                Debug.Log($"  ✅ CompositeCollider2D追加");

                // Rigidbody2Dも追加
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = obj.AddComponent<Rigidbody2D>();
                    rb.bodyType = RigidbodyType2D.Static;
                    rb.gravityScale = 0f;
                    Debug.Log($"  ✅ Rigidbody2D追加 (Static)");
                }

                CheckCompositeColliderSettings(newComposite);
            }
        }

        // TilemapCollider2Dの基本設定
        tilemapCol.isTrigger = false;
        if (!tilemapCol.enabled)
        {
            tilemapCol.enabled = true;
            Debug.Log($"  🔧 TilemapCollider2D.enabled = true");
        }
    }

    /// <summary>
    /// CompositeCollider2Dを修正
    /// </summary>
    private void FixCompositeCollider(CompositeCollider2D compositeCol)
    {
        GameObject obj = compositeCol.gameObject;
        string layerName = LayerMask.LayerToName(obj.layer);

        Debug.Log($"🔧 CompositeCollider2D修正: {obj.name} ({layerName})");

        // 現在の状態をログ出力
        Debug.Log($"  - pathCount: {compositeCol.pathCount}");
        Debug.Log($"  - pointCount: {compositeCol.pointCount}");

        CheckCompositeColliderSettings(compositeCol);

        // 統合後の状態をログ出力
        Debug.Log($"  - 修正後 pathCount: {compositeCol.pathCount}");
        Debug.Log($"  - 修正後 pointCount: {compositeCol.pointCount}");
    }

    /// <summary>
    /// CompositeCollider2Dの設定を確認・修正
    /// </summary>
    private void CheckCompositeColliderSettings(CompositeCollider2D compositeCol)
    {
        // 基本設定
        compositeCol.isTrigger = false;
        
        if (!compositeCol.enabled)
        {
            compositeCol.enabled = true;
            Debug.Log($"  🔧 CompositeCollider2D.enabled = true");
        }

        // 形状の再生成を強制
        compositeCol.GenerateGeometry();
        Debug.Log($"  🔧 CompositeCollider2D.GenerateGeometry()実行");

        // Rigidbody2Dの確認
        Rigidbody2D rb = compositeCol.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = compositeCol.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            rb.gravityScale = 0f;
            Debug.Log($"  ✅ Rigidbody2D追加 (Static)");
        }
    }

    /// <summary>
    /// 全てのTilemapCollider2Dの詳細情報を表示
    /// </summary>
    [ContextMenu("TilemapCollider2D詳細情報")]
    public void ShowTilemapColliderDetails()
    {
        Debug.Log("🔍 === TilemapCollider2D詳細情報 ===");

        TilemapCollider2D[] tilemapColliders = FindObjectsByType<TilemapCollider2D>(FindObjectsSortMode.None);

        foreach (TilemapCollider2D tilemapCol in tilemapColliders)
        {
            GameObject obj = tilemapCol.gameObject;
            string layerName = LayerMask.LayerToName(obj.layer);

            Debug.Log($"📋 {obj.name} ({layerName}):");
            Debug.Log($"  - enabled: {tilemapCol.enabled}");
            Debug.Log($"  - isTrigger: {tilemapCol.isTrigger}");
            Debug.Log($"  - usedByComposite: {tilemapCol.usedByComposite}");
            Debug.Log($"  - usedByEffector: {tilemapCol.usedByEffector}");

            // Tilemap確認
            Tilemap tilemap = obj.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                Debug.Log($"  - Tilemap存在: true");
            }
            else
            {
                Debug.Log($"  - Tilemap存在: false");
            }

            // CompositeCollider2D確認
            CompositeCollider2D compositeCol = obj.GetComponent<CompositeCollider2D>();
            if (compositeCol != null)
            {
                Debug.Log($"  - CompositeCollider2D存在: true");
                Debug.Log($"  - CompositeCollider2D pathCount: {compositeCol.pathCount}");
                Debug.Log($"  - CompositeCollider2D pointCount: {compositeCol.pointCount}");
            }
            else
            {
                Debug.Log($"  - CompositeCollider2D存在: false");
            }

            // Rigidbody2D確認
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Debug.Log($"  - Rigidbody2D存在: true (BodyType: {rb.bodyType})");
            }
            else
            {
                Debug.Log($"  - Rigidbody2D存在: false");
            }

            Debug.Log($"");
        }
    }

    /// <summary>
    /// 衝突テスト（修正後）
    /// </summary>
    [ContextMenu("修正後の衝突テスト")]
    public void TestCollisionAfterFix()
    {
        Debug.Log("🎯 === 修正後の衝突テスト ===");

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ プレイヤーが見つかりません");
            return;
        }

        Vector2 playerPos = player.transform.position;
        Debug.Log($"🎮 プレイヤー位置: {playerPos}");

        // 4方向のRaycastテスト
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        string[] dirNames = { "上", "下", "左", "右" };
        float testDistance = 2f;

        for (int i = 0; i < directions.Length; i++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(playerPos, directions[i], testDistance);
            
            Debug.Log($"{dirNames[i]}方向 ({hits.Length}個の衝突):");
            
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject != player)
                {
                    string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
                    Debug.Log($"  ✅ {hit.collider.name} ({layerName}) 距離:{hit.distance:F2}m");
                }
            }
        }
    }

    private void Start()
    {
        if (autoFixOnStart)
        {
            FixTilemapColliderIssues();
        }
    }
} 