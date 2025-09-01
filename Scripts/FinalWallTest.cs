using UnityEngine;

/// <summary>
/// 修正後の壁抜けテストを行う最終確認ツール
/// </summary>
public class FinalWallTest : MonoBehaviour
{
    [Header("テスト設定")]
    [SerializeField] private bool showDebugRays = true;
    [SerializeField] private float testDistance = 3f;
    [SerializeField] private float moveTestDistance = 0.5f;

    /// <summary>
    /// 完全な壁抜けテスト
    /// </summary>
    [ContextMenu("完全な壁抜けテスト")]
    public void CompleteWallPenetrationTest()
    {
        Debug.Log("🎯 === 完全な壁抜けテスト開始 ===");

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ プレイヤーが見つかりません");
            return;
        }

        // 1. 現在位置での詳細な衝突テスト
        TestDetailedCollisions(player);

        // 2. 移動シミュレーションテスト
        TestMovementSimulation(player);

        // 3. レイヤー別の衝突テスト
        TestLayerSpecificCollisions(player);

        // 4. 最終評価
        EvaluateFinalResult();

        Debug.Log("✅ 完全な壁抜けテスト完了");
    }

    /// <summary>
    /// 詳細な衝突テスト
    /// </summary>
    private void TestDetailedCollisions(GameObject player)
    {
        Debug.Log("--- 詳細衝突テスト ---");

        Vector2 playerPos = player.transform.position;
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        string[] dirNames = { "上", "下", "左", "右" };

        for (int i = 0; i < directions.Length; i++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(playerPos, directions[i], testDistance);
            
            Debug.Log($"🔍 {dirNames[i]}方向 ({hits.Length}個の衝突):");
            
            bool hasWallCollision = false;
            
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject != player)
                {
                    string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
                    string objectName = hit.collider.name;
                    
                    if (objectName.Contains("Wall"))
                    {
                        hasWallCollision = true;
                        Debug.Log($"  🧱 壁検出: {objectName} ({layerName}) 距離:{hit.distance:F2}m");
                    }
                    else
                    {
                        Debug.Log($"  📦 オブジェクト: {objectName} ({layerName}) 距離:{hit.distance:F2}m");
                    }
                }
            }

            if (!hasWallCollision)
            {
                Debug.Log($"  ⚠️ {dirNames[i]}方向に壁が検出されていません");
            }
        }
    }

    /// <summary>
    /// 移動シミュレーションテスト
    /// </summary>
    private void TestMovementSimulation(GameObject player)
    {
        Debug.Log("--- 移動シミュレーションテスト ---");

        Vector2 playerPos = player.transform.position;
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        string[] dirNames = { "上", "下", "左", "右" };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2 targetPos = playerPos + directions[i] * moveTestDistance;
            
            // 移動経路上の衝突をチェック
            RaycastHit2D hit = Physics2D.Raycast(playerPos, directions[i], moveTestDistance);
            
            if (hit.collider != null && hit.collider.gameObject != player)
            {
                string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
                Debug.Log($"🚫 {dirNames[i]}移動阻止: {hit.collider.name} ({layerName}) 距離:{hit.distance:F2}m");
            }
            else
            {
                Debug.Log($"✅ {dirNames[i]}移動可能");
            }

            // 実際の移動先での重複チェック
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(targetPos, 0.3f);
            int wallOverlaps = 0;
            
            foreach (Collider2D overlap in overlaps)
            {
                if (overlap.gameObject != player && overlap.name.Contains("Wall"))
                {
                    wallOverlaps++;
                    string layerName = LayerMask.LayerToName(overlap.gameObject.layer);
                    Debug.Log($"  🚨 移動先で壁と重複: {overlap.name} ({layerName})");
                }
            }

            if (wallOverlaps > 0)
            {
                Debug.Log($"  ❌ {dirNames[i]}移動すると壁抜けが発生");
            }
        }
    }

    /// <summary>
    /// レイヤー別の衝突テスト
    /// </summary>
    private void TestLayerSpecificCollisions(GameObject player)
    {
        Debug.Log("--- レイヤー別衝突テスト ---");

        Vector2 playerPos = player.transform.position;
        int playerLayer = player.layer;

        // 各レイヤーとの衝突テスト
        string[] layerNames = { "Layer 1", "Layer 2", "Layer 3" };
        
        foreach (string layerName in layerNames)
        {
            int layerIndex = LayerMask.NameToLayer(layerName);
            if (layerIndex == -1) continue;

            // レイヤー間の衝突設定確認
            bool shouldCollide = !Physics2D.GetIgnoreLayerCollision(playerLayer, layerIndex);
            Debug.Log($"🔍 プレイヤー ⇔ {layerName}: {(shouldCollide ? "衝突有効" : "衝突無効")}");

            // 実際の衝突テスト
            int layerMask = 1 << layerIndex;
            RaycastHit2D hit = Physics2D.Raycast(playerPos, Vector2.up, testDistance, layerMask);
            
            if (hit.collider != null)
            {
                Debug.Log($"  ✅ 実際の衝突: {hit.collider.name} 距離:{hit.distance:F2}m");
            }
            else
            {
                Debug.Log($"  ❌ 実際の衝突なし");
            }
        }
    }

    /// <summary>
    /// 最終評価
    /// </summary>
    private void EvaluateFinalResult()
    {
        Debug.Log("--- 最終評価 ---");

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        Vector2 playerPos = player.transform.position;
        int totalWallCollisions = 0;
        int totalDirections = 4;

        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        
        for (int i = 0; i < directions.Length; i++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(playerPos, directions[i], testDistance);
            
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject != player && hit.collider.name.Contains("Wall"))
                {
                    totalWallCollisions++;
                    break; // 1方向につき1回カウント
                }
            }
        }

        Debug.Log($"📊 壁衝突検出率: {totalWallCollisions}/{totalDirections} ({(float)totalWallCollisions/totalDirections*100:F1}%)");

        if (totalWallCollisions >= 3)
        {
            Debug.Log("🎉 壁抜け修正成功！衝突判定が正常に機能しています");
        }
        else if (totalWallCollisions >= 2)
        {
            Debug.Log("⚠️ 部分的に修正済み。一部の方向で壁抜けが発生する可能性があります");
        }
        else
        {
            Debug.Log("❌ 壁抜け修正失敗。追加の対策が必要です");
        }
    }

    /// <summary>
    /// プレイヤーを安全な位置に移動
    /// </summary>
    [ContextMenu("プレイヤーを安全な位置に移動")]
    public void MovePlayerToSafePosition()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        // 安全な位置を探す
        Vector2[] testPositions = {
            Vector2.zero,
            new Vector2(1, 1),
            new Vector2(-1, 1),
            new Vector2(1, -1),
            new Vector2(-1, -1)
        };

        foreach (Vector2 testPos in testPositions)
        {
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(testPos, 0.3f);
            bool isSafe = true;

            foreach (Collider2D overlap in overlaps)
            {
                if (overlap.name.Contains("Wall"))
                {
                    isSafe = false;
                    break;
                }
            }

            if (isSafe)
            {
                player.transform.position = new Vector3(testPos.x, testPos.y, 0);
                Debug.Log($"🚚 プレイヤーを安全な位置に移動: {testPos}");
                return;
            }
        }

        Debug.LogWarning("⚠️ 安全な位置が見つかりませんでした");
    }
} 