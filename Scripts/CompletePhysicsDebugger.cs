using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Physics2D設定とプレイヤー設定を完全に診断・修正するツール
/// </summary>
public class CompletePhysicsDebugger : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool autoFixOnStart = true;

    /// <summary>
    /// 完全な物理診断・修正
    /// </summary>
    [ContextMenu("完全な物理診断・修正")]
    public void CompletePhysicsDiagnosis()
    {
        Debug.Log("🔬 === 完全な物理診断開始 ===");

        // 1. Project Settingsの物理設定を確認
        CheckPhysics2DSettings();

        // 2. レイヤーマトリクスを確認・修正
        CheckAndFixLayerMatrix();

        // 3. プレイヤーの詳細設定を確認・修正
        CheckAndFixPlayerSettings();

        // 4. 壁オブジェクトの詳細確認
        CheckWallObjectsDetailed();

        // 5. 実際の衝突テスト（詳細版）
        PerformDetailedCollisionTest();

        // 6. 必要に応じて修正適用
        ApplyPhysicsFixes();

        Debug.Log("✅ 完全な物理診断完了");
    }

    /// <summary>
    /// Project SettingsのPhysics2D設定を確認
    /// </summary>
    [ContextMenu("1. Physics2D設定を確認")]
    public void CheckPhysics2DSettings()
    {
        Debug.Log("--- Physics2D Project Settings確認 ---");

        // 重力設定
        Debug.Log($"🌍 重力: {Physics2D.gravity}");

        // 速度しきい値
        Debug.Log($"⚡ 速度しきい値: {Physics2D.bounceThreshold}");

        // バウンスしきい値
        Debug.Log($"🏀 バウンスしきい値: {Physics2D.bounceThreshold}");

        // デフォルトマテリアル（Unity2022以降では利用不可）
        Debug.Log($"📦 デフォルトマテリアル: Unity2022以降では利用不可");

        // レイヤー衝突マトリクス（同じレイヤー同士を含む）
        Debug.Log("=== レイヤー衝突マトリクス ===");
        CheckLayerCollisionMatrix();
    }

    /// <summary>
    /// レイヤー衝突マトリクスを確認
    /// </summary>
    private void CheckLayerCollisionMatrix()
    {
        int layer1 = LayerMask.NameToLayer("Layer 1");
        int layer2 = LayerMask.NameToLayer("Layer 2");
        int layer3 = LayerMask.NameToLayer("Layer 3");

        Debug.Log("🔍 レイヤー衝突マトリクス:");

        // 同じレイヤー同士の衝突（重要！）
        if (layer1 != -1)
        {
            bool layer1SelfCollision = !Physics2D.GetIgnoreLayerCollision(layer1, layer1);
            Debug.Log($"Layer 1 ⇔ Layer 1: {(layer1SelfCollision ? "✅ 有効" : "❌ 無効")}");
        }

        if (layer2 != -1)
        {
            bool layer2SelfCollision = !Physics2D.GetIgnoreLayerCollision(layer2, layer2);
            Debug.Log($"Layer 2 ⇔ Layer 2: {(layer2SelfCollision ? "✅ 有効" : "❌ 無効")}");
        }

        if (layer3 != -1)
        {
            bool layer3SelfCollision = !Physics2D.GetIgnoreLayerCollision(layer3, layer3);
            Debug.Log($"Layer 3 ⇔ Layer 3: {(layer3SelfCollision ? "✅ 有効" : "❌ 無効")}");
        }

        // レイヤー間の衝突
        if (layer1 != -1 && layer2 != -1)
        {
            bool collision = !Physics2D.GetIgnoreLayerCollision(layer1, layer2);
            Debug.Log($"Layer 1 ⇔ Layer 2: {(collision ? "✅ 有効" : "❌ 無効")}");
        }

        if (layer1 != -1 && layer3 != -1)
        {
            bool collision = !Physics2D.GetIgnoreLayerCollision(layer1, layer3);
            Debug.Log($"Layer 1 ⇔ Layer 3: {(collision ? "✅ 有効" : "❌ 無効")}");
        }

        if (layer2 != -1 && layer3 != -1)
        {
            bool collision = !Physics2D.GetIgnoreLayerCollision(layer2, layer3);
            Debug.Log($"Layer 2 ⇔ Layer 3: {(collision ? "✅ 有効" : "❌ 無効")}");
        }
    }

    /// <summary>
    /// レイヤーマトリクスを修正
    /// </summary>
    [ContextMenu("2. レイヤーマトリクスを修正")]
    public void CheckAndFixLayerMatrix()
    {
        Debug.Log("--- レイヤーマトリクス修正 ---");

        int layer1 = LayerMask.NameToLayer("Layer 1");
        int layer2 = LayerMask.NameToLayer("Layer 2");
        int layer3 = LayerMask.NameToLayer("Layer 3");

        // 同じレイヤー同士の衝突を有効化（最重要！）
        if (layer1 != -1)
        {
            Physics2D.IgnoreLayerCollision(layer1, layer1, false);
            Debug.Log("🔧 Layer 1 ⇔ Layer 1: 衝突有効化");
        }

        if (layer2 != -1)
        {
            Physics2D.IgnoreLayerCollision(layer2, layer2, false);
            Debug.Log("🔧 Layer 2 ⇔ Layer 2: 衝突有効化");
        }

        if (layer3 != -1)
        {
            Physics2D.IgnoreLayerCollision(layer3, layer3, false);
            Debug.Log("🔧 Layer 3 ⇔ Layer 3: 衝突有効化");
        }

        // 全てのレイヤー間の衝突を有効化
        if (layer1 != -1 && layer2 != -1)
        {
            Physics2D.IgnoreLayerCollision(layer1, layer2, false);
            Debug.Log("🔧 Layer 1 ⇔ Layer 2: 衝突有効化");
        }

        if (layer1 != -1 && layer3 != -1)
        {
            Physics2D.IgnoreLayerCollision(layer1, layer3, false);
            Debug.Log("🔧 Layer 1 ⇔ Layer 3: 衝突有効化");
        }

        if (layer2 != -1 && layer3 != -1)
        {
            Physics2D.IgnoreLayerCollision(layer2, layer3, false);
            Debug.Log("🔧 Layer 2 ⇔ Layer 3: 衝突有効化");
        }

        // 修正後の確認
        CheckLayerCollisionMatrix();
    }

    /// <summary>
    /// プレイヤーの詳細設定を確認・修正
    /// </summary>
    [ContextMenu("3. プレイヤーの詳細設定を確認・修正")]
    public void CheckAndFixPlayerSettings()
    {
        Debug.Log("--- プレイヤー詳細設定確認 ---");

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ プレイヤーが見つかりません");
            return;
        }

        Debug.Log($"🎮 プレイヤー: {player.name}");
        Debug.Log($"🎮 位置: {player.transform.position}");
        Debug.Log($"🎮 レイヤー: {LayerMask.LayerToName(player.layer)} ({player.layer})");

        // Rigidbody2D詳細確認
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Debug.Log($"🎮 Rigidbody2D詳細:");
            Debug.Log($"  - BodyType: {rb.bodyType}");
            Debug.Log($"  - Mass: {rb.mass}");
            Debug.Log($"  - Drag: {rb.linearDamping}");
            Debug.Log($"  - Angular Drag: {rb.angularDamping}");
            Debug.Log($"  - Gravity Scale: {rb.gravityScale}");
            Debug.Log($"  - Collision Detection: {rb.collisionDetectionMode}");
            Debug.Log($"  - Sleeping Mode: {rb.sleepMode}");
            Debug.Log($"  - Interpolate: {rb.interpolation}");
            Debug.Log($"  - Constraints: {rb.constraints}");

            // 必要に応じて修正
            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                Debug.Log("🔧 BodyTypeをDynamicに修正");
            }

            if (rb.gravityScale != 0f)
            {
                rb.gravityScale = 0f;
                Debug.Log("🔧 Gravity Scaleを0に修正");
            }

            if (rb.collisionDetectionMode != CollisionDetectionMode2D.Continuous)
            {
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                Debug.Log("🔧 Collision DetectionをContinuousに修正");
            }
        }

        // Collider2D詳細確認
        Collider2D[] colliders = player.GetComponents<Collider2D>();
        Debug.Log($"🎮 コライダー詳細 ({colliders.Length}個):");

        foreach (Collider2D col in colliders)
        {
            Debug.Log($"  - {col.GetType().Name}:");
            Debug.Log($"    isTrigger: {col.isTrigger}");
            Debug.Log($"    enabled: {col.enabled}");
            Debug.Log($"    usedByEffector: {col.usedByEffector}");
            Debug.Log($"    usedByComposite: {col.usedByComposite}");

            if (col is CircleCollider2D circle)
            {
                Debug.Log($"    radius: {circle.radius}");
                Debug.Log($"    offset: {circle.offset}");
            }
            else if (col is BoxCollider2D box)
            {
                Debug.Log($"    size: {box.size}");
                Debug.Log($"    offset: {box.offset}");
            }

            // 物理マテリアル確認
            if (col.sharedMaterial != null)
            {
                Debug.Log($"    Material: {col.sharedMaterial.name}");
                Debug.Log($"    Friction: {col.sharedMaterial.friction}");
                Debug.Log($"    Bounciness: {col.sharedMaterial.bounciness}");
            }
            else
            {
                Debug.Log($"    Material: None");
            }

            // Triggerが有効になっている場合は修正
            if (col.isTrigger)
            {
                col.isTrigger = false;
                Debug.Log("🔧 isTriggerを無効化");
            }
        }
    }

    /// <summary>
    /// 壁オブジェクトの詳細確認
    /// </summary>
    [ContextMenu("4. 壁オブジェクトの詳細確認")]
    public void CheckWallObjectsDetailed()
    {
        Debug.Log("--- 壁オブジェクト詳細確認 ---");

        string[] layerNames = { "Layer 1", "Layer 2", "Layer 3" };

        foreach (string layerName in layerNames)
        {
            int layerIndex = LayerMask.NameToLayer(layerName);
            if (layerIndex == -1) continue;

            Debug.Log($"=== {layerName} ({layerIndex}) ===");

            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var layerObjects = System.Array.FindAll(allObjects, obj => obj.layer == layerIndex);

            Debug.Log($"🧱 レイヤー内総オブジェクト数: {layerObjects.Length}");

            var wallObjects = System.Array.FindAll(layerObjects, obj => obj.name.Contains("Wall"));
            Debug.Log($"🧱 壁オブジェクト数: {wallObjects.Length}");

            // 最初の5個の壁オブジェクトを詳細表示
            for (int i = 0; i < Mathf.Min(wallObjects.Length, 5); i++)
            {
                GameObject wall = wallObjects[i];
                Debug.Log($"  壁 #{i + 1}: {wall.name}");
                Debug.Log($"    位置: {wall.transform.position}");
                Debug.Log($"    スケール: {wall.transform.localScale}");
                Debug.Log($"    アクティブ: {wall.activeInHierarchy}");

                Collider2D[] wallColliders = wall.GetComponents<Collider2D>();
                Debug.Log($"    コライダー数: {wallColliders.Length}");

                foreach (Collider2D col in wallColliders)
                {
                    Debug.Log($"      - {col.GetType().Name}: trigger={col.isTrigger}, enabled={col.enabled}");

                    if (col is TilemapCollider2D tilemapCol)
                    {
                        Debug.Log($"        usedByComposite: {tilemapCol.usedByComposite}");
                    }
                    else if (col is CompositeCollider2D compositeCol)
                    {
                        Debug.Log($"        pathCount: {compositeCol.pathCount}");
                        Debug.Log($"        pointCount: {compositeCol.pointCount}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 詳細な衝突テスト
    /// </summary>
    [ContextMenu("5. 詳細な衝突テスト")]
    public void PerformDetailedCollisionTest()
    {
        Debug.Log("--- 詳細衝突テスト ---");

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        Vector2 playerPos = player.transform.position;
        Debug.Log($"🎮 プレイヤー位置: {playerPos}");

        // 複数の距離でテスト
        float[] testDistances = { 0.5f, 1f, 2f, 3f };
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        string[] dirNames = { "上", "下", "左", "右" };

        foreach (float distance in testDistances)
        {
            Debug.Log($"=== 距離 {distance}m での衝突テスト ===");

            for (int i = 0; i < directions.Length; i++)
            {
                // Physics2D.RaycastAllで全ての衝突を取得
                RaycastHit2D[] hits = Physics2D.RaycastAll(playerPos, directions[i], distance);

                Debug.Log($"{dirNames[i]}方向 ({hits.Length}個の衝突):");

                if (hits.Length == 0)
                {
                    Debug.Log($"  ❌ 衝突なし");
                }
                else
                {
                    foreach (RaycastHit2D hit in hits)
                    {
                        string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
                        Debug.Log($"  ✅ {hit.collider.name} ({layerName}) 距離:{hit.distance:F2}m");
                    }
                }
            }
        }

        // OverlapCircleテスト
        Debug.Log("=== 重複テスト ===");
        float[] radii = { 0.3f, 0.5f, 1f };

        foreach (float radius in radii)
        {
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(playerPos, radius);
            Debug.Log($"半径 {radius}m: {overlaps.Length}個のオブジェクトと重複");

            foreach (Collider2D overlap in overlaps)
            {
                if (overlap.gameObject != player)
                {
                    string layerName = LayerMask.LayerToName(overlap.gameObject.layer);
                    Debug.Log($"  🔄 {overlap.name} ({layerName})");
                }
            }
        }
    }

    /// <summary>
    /// 物理修正を適用
    /// </summary>
    [ContextMenu("6. 物理修正を適用")]
    public void ApplyPhysicsFixes()
    {
        Debug.Log("--- 物理修正適用 ---");

        // レイヤーマトリクス修正
        CheckAndFixLayerMatrix();

        // プレイヤー設定修正
        CheckAndFixPlayerSettings();

        // Layer 2の壁にコライダー追加
        FixLayer2Walls();

        Debug.Log("✅ 物理修正適用完了");
    }

    /// <summary>
    /// Layer 2の壁を修正（コライダー追加）
    /// </summary>
    private void FixLayer2Walls()
    {
        int layer2 = LayerMask.NameToLayer("Layer 2");
        if (layer2 == -1) return;

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int fixedWalls = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == layer2 && obj.name.Contains("Wall"))
            {
                Collider2D[] colliders = obj.GetComponents<Collider2D>();

                if (colliders.Length == 0)
                {
                    // BoxCollider2Dを追加
                    BoxCollider2D newCollider = obj.AddComponent<BoxCollider2D>();
                    newCollider.isTrigger = false;
                    fixedWalls++;
                    Debug.Log($"🔧 {obj.name}: BoxCollider2D追加");
                }
            }
        }

        Debug.Log($"✅ Layer 2の壁 {fixedWalls}個を修正");
    }

    /// <summary>
    /// プレイヤーを原点に移動（テスト用）
    /// </summary>
    [ContextMenu("プレイヤーを原点に移動")]
    public void MovePlayerToOrigin()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = Vector3.zero;
            Debug.Log("🚚 プレイヤーを原点(0,0,0)に移動");
        }
    }

    private void Start()
    {
        if (autoFixOnStart)
        {
            CompletePhysicsDiagnosis();
        }
    }
} 