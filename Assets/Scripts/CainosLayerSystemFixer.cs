using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Cainosレイヤーシステムの物理設定とプレファブ表示を修正
/// </summary>
public class CainosLayerSystemFixer : MonoBehaviour
{
    [Header("修正ツール")]
    [SerializeField] private bool fixPhysicsCollisions = false;
    [SerializeField] private bool fixPrefabVisibility = false;
    [SerializeField] private bool checkCurrentPhysics = false;
    [SerializeField] private bool analyzePrefabLayers = false;

    void Update()
    {
        if (fixPhysicsCollisions)
        {
            fixPhysicsCollisions = false;
#if UNITY_EDITOR
            FixPhysicsCollisionMatrix();
#else
            Debug.LogWarning("FixPhysicsCollisionMatrix はエディタ専用機能のためビルドでは無効です");
#endif
        }
        
        if (fixPrefabVisibility)
        {
            fixPrefabVisibility = false;
            FixPrefabVisibility();
        }
        
        if (checkCurrentPhysics)
        {
            checkCurrentPhysics = false;
            CheckCurrentPhysicsSettings();
        }
        
        if (analyzePrefabLayers)
        {
            analyzePrefabLayers = false;
            AnalyzePrefabLayers();
        }
    }

    #if UNITY_EDITOR
    /// <summary>
    /// Physics2Dのコリジョン設定を修正
    /// </summary>
    private void FixPhysicsCollisionMatrix()
    {
        Debug.Log("=== Physics2D コリジョン設定修正開始 ===");

        // Layer名からインデックスを取得
        int layer1Index = LayerMask.NameToLayer("Layer 1");
        int layer2Index = LayerMask.NameToLayer("Layer 2");
        int layer3Index = LayerMask.NameToLayer("Layer 3");
        int playerIndex = LayerMask.NameToLayer("Player");

        if (layer1Index == -1 || layer2Index == -1 || layer3Index == -1)
        {
            Debug.LogError("Layer 1, 2, 3が見つかりません。先にSetup Correct Layersを実行してください。");
            return;
        }

        // 現在の設定をレポート
        Debug.Log($"Layer 1 Index: {layer1Index}");
        Debug.Log($"Layer 2 Index: {layer2Index}");
        Debug.Log($"Layer 3 Index: {layer3Index}");
        Debug.Log($"Player Index: {playerIndex}");

        // Physics2D設定を取得
        var physics2DSettingsPath = "ProjectSettings/Physics2DSettings.asset";
        var physics2DSettings = AssetDatabase.LoadAllAssetsAtPath(physics2DSettingsPath)[0];
        SerializedObject serializedSettings = new SerializedObject(physics2DSettings);

        // Layer Collision Matrixを取得
        SerializedProperty collisionMatrix = serializedSettings.FindProperty("m_LayerCollisionMatrix");
        
        if (collisionMatrix != null)
        {
            // レイヤー間のコリジョンを無効化
            // Layer 1の壁とLayer 2のプレイヤーが衝突しないようにする
            SetLayerCollision(collisionMatrix, layer1Index, layer2Index, false);
            SetLayerCollision(collisionMatrix, layer1Index, layer3Index, false);
            SetLayerCollision(collisionMatrix, layer2Index, layer3Index, false);

            // 同じレイヤー内でのコリジョンは有効
            SetLayerCollision(collisionMatrix, layer1Index, layer1Index, true);
            SetLayerCollision(collisionMatrix, layer2Index, layer2Index, true);
            SetLayerCollision(collisionMatrix, layer3Index, layer3Index, true);

            // プレイヤーと各レイヤーの関係
            if (playerIndex != -1)
            {
                SetLayerCollision(collisionMatrix, playerIndex, layer1Index, true);
                SetLayerCollision(collisionMatrix, playerIndex, layer2Index, true);
                SetLayerCollision(collisionMatrix, playerIndex, layer3Index, true);
            }

            serializedSettings.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            Debug.Log("✅ Physics2D設定を修正しました:");
            Debug.Log("  - Layer 1 ⇔ Layer 2: コリジョン無効");
            Debug.Log("  - Layer 1 ⇔ Layer 3: コリジョン無効");
            Debug.Log("  - Layer 2 ⇔ Layer 3: コリジョン無効");
            Debug.Log("  - 同レイヤー内: コリジョン有効");
        }
        else
        {
            Debug.LogError("Physics2D設定が見つかりませんでした。");
        }

        Debug.Log("=== Physics2D設定修正完了 ===");
    }

    /// <summary>
    /// レイヤー間のコリジョン設定
    /// </summary>
    private void SetLayerCollision(SerializedProperty collisionMatrix, int layer1, int layer2, bool collide)
    {
        int index = layer1 * 32 + layer2;
        if (index < collisionMatrix.arraySize)
        {
            collisionMatrix.GetArrayElementAtIndex(index).boolValue = collide;
        }
        
        // 対称的に設定
        int reverseIndex = layer2 * 32 + layer1;
        if (reverseIndex < collisionMatrix.arraySize)
        {
            collisionMatrix.GetArrayElementAtIndex(reverseIndex).boolValue = collide;
        }
    }
    #endif

    /// <summary>
    /// プレファブの表示設定を修正
    /// </summary>
    private void FixPrefabVisibility()
    {
        Debug.Log("=== プレファブ表示設定修正開始 ===");

        // シーン内の全SpriteRendererを取得
        SpriteRenderer[] allRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        int fixedCount = 0;

        foreach (SpriteRenderer renderer in allRenderers)
        {
            GameObject obj = renderer.gameObject;
            
            // Cainosプレファブかチェック
            if (obj.name.Contains("PF Struct") || obj.name.Contains("Cainos") || 
                obj.transform.parent?.name.Contains("Props") == true)
            {
                // オブジェクトのレイヤーを確認
                string layerName = LayerMask.LayerToName(obj.layer);
                
                // レイヤーに応じて適切なソーティングレイヤーを設定
                string targetSortingLayer = GetAppropriateeSortingLayer(layerName);
                
                if (renderer.sortingLayerName != targetSortingLayer)
                {
                    Debug.Log($"修正: {obj.name} - Layer:{layerName} → SortingLayer:{targetSortingLayer}");
                    renderer.sortingLayerName = targetSortingLayer;
                    fixedCount++;
                }

                // ソーティングオーダーを調整
                if (obj.name.Contains("Stairs"))
                {
                    renderer.sortingOrder = 1; // 階段は少し手前
                }
                else if (obj.name.Contains("Wall"))
                {
                    renderer.sortingOrder = 0; // 壁は基本位置
                }
                else
                {
                    renderer.sortingOrder = 0; // その他も基本位置
                }
            }
        }

        Debug.Log($"プレファブ表示修正完了: {fixedCount}個のオブジェクトを修正");
    }

    /// <summary>
    /// レイヤーに応じた適切なソーティングレイヤーを取得
    /// </summary>
    private string GetAppropriateeSortingLayer(string objectLayer)
    {
        switch (objectLayer)
        {
            case "Layer 1":
                return "Layer 1";
            case "Layer 2":
                return "Layer 2";
            case "Layer 3":
                return "Layer 3";
            default:
                return "Layer 1"; // デフォルト
        }
    }

    /// <summary>
    /// 現在のPhysics2D設定をチェック
    /// </summary>
    private void CheckCurrentPhysicsSettings()
    {
        Debug.Log("=== 現在のPhysics2D設定 ===");

        int layer1 = LayerMask.NameToLayer("Layer 1");
        int layer2 = LayerMask.NameToLayer("Layer 2");
        int layer3 = LayerMask.NameToLayer("Layer 3");
        int player = LayerMask.NameToLayer("Player");

        if (layer1 != -1 && layer2 != -1)
        {
            bool collision12 = !Physics2D.GetIgnoreLayerCollision(layer1, layer2);
            Debug.Log($"Layer 1 ⇔ Layer 2 コリジョン: {(collision12 ? "有効" : "無効")}");
        }

        if (layer1 != -1 && layer3 != -1)
        {
            bool collision13 = !Physics2D.GetIgnoreLayerCollision(layer1, layer3);
            Debug.Log($"Layer 1 ⇔ Layer 3 コリジョン: {(collision13 ? "有効" : "無効")}");
        }

        if (layer2 != -1 && layer3 != -1)
        {
            bool collision23 = !Physics2D.GetIgnoreLayerCollision(layer2, layer3);
            Debug.Log($"Layer 2 ⇔ Layer 3 コリジョン: {(collision23 ? "有効" : "無効")}");
        }

        if (player != -1 && layer1 != -1)
        {
            bool playerCollision = !Physics2D.GetIgnoreLayerCollision(player, layer1);
            Debug.Log($"Player ⇔ Layer 1 コリジョン: {(playerCollision ? "有効" : "無効")}");
        }

        Debug.Log("========================");
    }

    /// <summary>
    /// プレファブのレイヤー設定を分析
    /// </summary>
    private void AnalyzePrefabLayers()
    {
        Debug.Log("=== プレファブレイヤー分析 ===");

        SpriteRenderer[] allRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        
        foreach (SpriteRenderer renderer in allRenderers)
        {
            GameObject obj = renderer.gameObject;
            
            if (obj.name.Contains("PF Struct") || obj.name.Contains("Stairs"))
            {
                string layerName = LayerMask.LayerToName(obj.layer);
                Debug.Log($"プレファブ: {obj.name}");
                Debug.Log($"  Unity Layer: {layerName} ({obj.layer})");
                Debug.Log($"  Sorting Layer: {renderer.sortingLayerName}");
                Debug.Log($"  Sorting Order: {renderer.sortingOrder}");
                Debug.Log($"  Active: {obj.activeInHierarchy}");
                Debug.Log("---");
            }
        }

        Debug.Log("===================");
    }

    /// <summary>
    /// 使用方法ガイド
    /// </summary>
    [ContextMenu("使用方法ガイド")]
    public void ShowUsageGuide()
    {
        Debug.Log("================================");
        Debug.Log("🔧 Cainosレイヤーシステム修正ツール");
        Debug.Log("================================");
        Debug.Log("【壁の影響を受ける問題】");
        Debug.Log("1. 'Fix Physics Collisions' - レイヤー間コリジョン修正");
        Debug.Log("");
        Debug.Log("【プレファブが見えない問題】");
        Debug.Log("2. 'Fix Prefab Visibility' - プレファブ表示修正");
        Debug.Log("");
        Debug.Log("【分析・確認】");
        Debug.Log("3. 'Check Current Physics' - 現在の物理設定確認");
        Debug.Log("4. 'Analyze Prefab Layers' - プレファブレイヤー分析");
        Debug.Log("");
        Debug.Log("【修正内容】");
        Debug.Log("- Layer 1, 2, 3間のコリジョンを無効化");
        Debug.Log("- プレファブのソーティングレイヤーを適切に設定");
        Debug.Log("- レイヤーに応じた表示順序の調整");
        Debug.Log("================================");
    }
} 