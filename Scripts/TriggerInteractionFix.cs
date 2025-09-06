using UnityEngine;

/// <summary>
/// プレファブの光るアニメーションなどのトリガー検出を修正
/// レイヤー衝突制御でトリガーが機能しなくなった問題を解決
/// </summary>
public class TriggerInteractionFix : MonoBehaviour
{
    [Header("トリガー修正設定")]
    [SerializeField] private bool enableFix = true;
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("除外レイヤー")]
    [SerializeField] private string[] excludeLayerNames = new string[] 
    { 
        "UI", 
        "IgnoreRaycast", 
        "TransparentFX", 
        "Water",
        "Default"  // Defaultレイヤーもトリガー用として除外
    };
    
    private void Start()
    {
        if (enableFix)
        {
            FixTriggerInteractions();
        }
    }
    
    /// <summary>
    /// トリガー検出を修正
    /// </summary>
    private void FixTriggerInteractions()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== TriggerInteractionFix: トリガー検出修正開始 ===");
        }
        
        // 1. プレイヤーレイヤーとDefaultレイヤーの衝突を有効化（トリガー用）
        int playerLayer = LayerMask.NameToLayer("Player");
        int defaultLayer = 0; // Default layer
        
        if (playerLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, defaultLayer, false);
            if (showDebugInfo)
            {
                Debug.Log($"プレイヤー ⇔ Default: 衝突有効化（トリガー用）");
            }
        }
        
        // 2. すべてのトリガーコライダーをチェックして修正
        FixAllTriggerColliders();
        
        // 3. レイヤー衝突マトリックスの表示
        if (showDebugInfo)
        {
            ShowCollisionMatrix();
        }
        
        Debug.Log("TriggerInteractionFix: 修正完了");
    }
    
    /// <summary>
    /// すべてのトリガーコライダーを修正
    /// </summary>
    private void FixAllTriggerColliders()
    {
        // シーン内のすべてのトリガーコライダーを検索
        Collider2D[] allColliders = Object.FindObjectsOfType<Collider2D>();
        int triggerCount = 0;
        int fixedCount = 0;
        
        foreach (Collider2D col in allColliders)
        {
            if (col.isTrigger)
            {
                triggerCount++;
                
                // プレファブのトリガーと思われるオブジェクトを特定
                if (IsPrefabTrigger(col))
                {
                    // Defaultレイヤーに設定（トリガー検出用）
                    if (col.gameObject.layer != 0)
                    {
                        col.gameObject.layer = 0; // Default layer
                        fixedCount++;
                        
                        if (showDebugInfo)
                        {
                            Debug.Log($"🔧 トリガー修正: {col.name} → Defaultレイヤー");
                        }
                    }
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"トリガーコライダー: {triggerCount}個発見, {fixedCount}個修正");
        }
    }
    
    /// <summary>
    /// プレファブのトリガーかどうかを判定
    /// </summary>
    private bool IsPrefabTrigger(Collider2D collider)
    {
        string objName = collider.name.ToLower();
        
        // 光るアニメーションなどのプレファブを特定するキーワード
        string[] triggerKeywords = {
            "artifact", "item", "chest", "portal", "trigger", 
            "interaction", "glow", "light", "pickup", "collectible",
            "prefab", "prop", "decoration"
        };
        
        foreach (string keyword in triggerKeywords)
        {
            if (objName.Contains(keyword))
            {
                return true;
            }
        }
        
        // コンポーネントで判定
        if (collider.GetComponent<Animator>() != null)
        {
            return true; // アニメーターがあるものはプレファブの可能性が高い
        }
        
        return false;
    }
    
    /// <summary>
    /// レイヤー衝突マトリックスを表示
    /// </summary>
    private void ShowCollisionMatrix()
    {
        Debug.Log("=== レイヤー衝突マトリックス ===");
        
        int playerLayer = LayerMask.NameToLayer("Player");
        int layer1 = LayerMask.NameToLayer("Layer 1");
        int layer2 = LayerMask.NameToLayer("Layer 2");
        int layer3 = LayerMask.NameToLayer("Layer 3");
        int defaultLayer = 0;
        
        if (playerLayer != -1)
        {
            Debug.Log($"プレイヤー ⇔ Default: {!Physics2D.GetIgnoreLayerCollision(playerLayer, defaultLayer)}");
            if (layer1 != -1) Debug.Log($"プレイヤー ⇔ Layer 1: {!Physics2D.GetIgnoreLayerCollision(playerLayer, layer1)}");
            if (layer2 != -1) Debug.Log($"プレイヤー ⇔ Layer 2: {!Physics2D.GetIgnoreLayerCollision(playerLayer, layer2)}");
            if (layer3 != -1) Debug.Log($"プレイヤー ⇔ Layer 3: {!Physics2D.GetIgnoreLayerCollision(playerLayer, layer3)}");
        }
    }
    
    /// <summary>
    /// トリガー検出テスト
    /// </summary>
    [ContextMenu("トリガー検出テスト")]
    public void TestTriggerDetection()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("プレイヤーが見つかりません");
            return;
        }
        
        Debug.Log("=== トリガー検出テスト ===");
        Debug.Log($"プレイヤー位置: {player.transform.position}");
        Debug.Log($"プレイヤーレイヤー: {LayerMask.LayerToName(player.layer)} ({player.layer})");
        
        // 周辺のトリガーを検索
        Collider2D[] triggers = Physics2D.OverlapCircleAll(player.transform.position, 10f);
        int triggerCount = 0;
        
        foreach (Collider2D col in triggers)
        {
            if (col.isTrigger && col.gameObject != player)
            {
                triggerCount++;
                float distance = Vector2.Distance(player.transform.position, col.transform.position);
                string layerName = LayerMask.LayerToName(col.gameObject.layer);
                
                Debug.Log($"🎯 トリガー発見: {col.name} - Layer: {layerName} - 距離: {distance:F2}");
                
                // アニメーターがあるかチェック
                Animator animator = col.GetComponent<Animator>();
                if (animator != null)
                {
                    Debug.Log($"  アニメーター: 有り - 状態数: {animator.runtimeAnimatorController?.animationClips?.Length ?? 0}");
                }
            }
        }
        
        Debug.Log($"近くのトリガー数: {triggerCount}");
    }
    
    /// <summary>
    /// 衝突設定を強制更新
    /// </summary>
    [ContextMenu("衝突設定を強制更新")]
    public void ForceUpdateCollisions()
    {
        Debug.Log("衝突設定を強制更新します");
        
        // すべてのレイヤー衝突を一旦リセット
        for (int i = 0; i < 32; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                Physics2D.IgnoreLayerCollision(i, j, false);
            }
        }
        
        Debug.Log("衝突設定の強制更新が完了しました");
    }
    
    /// <summary>
    /// 緊急修復：すべてのトリガー検出を強制修正
    /// </summary>
    [ContextMenu("緊急修復")]
    public void EmergencyFix()
    {
        Debug.Log("=== 緊急修復開始 ===");
        
        // 1. プレイヤーをPlayerレイヤーに設定
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                player.layer = playerLayer;
                Debug.Log($"🔧 プレイヤーをPlayerレイヤーに設定");
            }
        }
        
        // 2. すべてのレイヤー衝突を一旦リセット
        for (int i = 0; i < 32; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                Physics2D.IgnoreLayerCollision(i, j, false);
            }
        }
        Debug.Log("🔧 すべてのレイヤー衝突をリセット");
        
        // 3. 基本的な衝突設定のみ適用
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        int layer1Index = LayerMask.NameToLayer("Layer 1");
        int layer2Index = LayerMask.NameToLayer("Layer 2");
        int layer3Index = LayerMask.NameToLayer("Layer 3");
        
        // 階層レイヤー間の衝突のみ無効化
        if (layer1Index != -1 && layer2Index != -1)
            Physics2D.IgnoreLayerCollision(layer1Index, layer2Index, true);
        if (layer1Index != -1 && layer3Index != -1)
            Physics2D.IgnoreLayerCollision(layer1Index, layer3Index, true);
        if (layer2Index != -1 && layer3Index != -1)
            Physics2D.IgnoreLayerCollision(layer2Index, layer3Index, true);
        
        Debug.Log("🔧 基本的な衝突設定を適用");
        
        // 4. トリガー修正
        FixTriggerInteractions();
        
        Debug.Log("=== 緊急修復完了 ===");
    }
} 