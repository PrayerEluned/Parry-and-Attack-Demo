using UnityEngine;

/// <summary>
/// プレイヤーと壁の落下問題を一括修正するツール
/// 2Dトップダウンゲーム用の物理設定を適用
/// </summary>
public class PhysicsFixTool : MonoBehaviour
{
    [Header("修正対象")]
    [SerializeField] private bool fixPlayer = true;
    [SerializeField] private bool fixWalls = true;
    [SerializeField] private bool fixTilemaps = true;
    [SerializeField] private bool fixProjectSettings = true;
    
    [Header("設定値")]
    [SerializeField] private float playerGravityScale = 0f;
    [SerializeField] private RigidbodyType2D wallBodyType = RigidbodyType2D.Static;
    [SerializeField] private float wallGravityScale = 0f;
    
    /// <summary>
    /// 落下問題を即座に修正
    /// </summary>
    [ContextMenu("落下問題を即座修正")]
    public void FixFallingObjects()
    {
        Debug.Log("=== 落下問題修正開始 ===");
        
        if (fixPlayer) FixPlayerPhysics();
        if (fixWalls) FixWallPhysics();
        if (fixTilemaps) FixTilemapPhysics();
        if (fixProjectSettings) FixProjectPhysics();
        
        Debug.Log("=== 落下問題修正完了 ===");
        Debug.Log("✅ プレイヤーと壁の落下が停止しました");
    }
    
    /// <summary>
    /// プレイヤーの物理設定修正
    /// </summary>
    private void FixPlayerPhysics()
    {
        Debug.Log("プレイヤー物理設定修正中...");
        
        var players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 重力を無効化
                rb.gravityScale = playerGravityScale;
                // 回転を固定
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                // 連続衝突検出
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                // BodyTypeをDynamicに（移動可能）
                rb.bodyType = RigidbodyType2D.Dynamic;
                
                Debug.Log($"✅ プレイヤー修正: {player.name} (GravityScale: {rb.gravityScale})");
            }
        }
        
        // CharacterMovementがあるオブジェクトも修正
        var movements = FindObjectsByType<CharacterMovement>(FindObjectsSortMode.None);
        foreach (var movement in movements)
        {
            var rb = movement.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = playerGravityScale;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.bodyType = RigidbodyType2D.Dynamic;
                
                Debug.Log($"✅ CharacterMovement修正: {movement.name}");
            }
        }
        
        Debug.Log("プレイヤー物理設定修正完了");
    }
    
    /// <summary>
    /// 壁・障害物の物理設定修正
    /// </summary>
    private void FixWallPhysics()
    {
        Debug.Log("壁物理設定修正中...");
        
        var rigidbodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        foreach (var rb in rigidbodies)
        {
            // プレイヤー以外のRigidbody2Dを修正
            bool isPlayer = rb.GetComponent<PlayerStats>() != null || 
                           rb.GetComponent<CharacterMovement>() != null;
            
            if (!isPlayer)
            {
                // 壁・障害物は静的に設定
                rb.bodyType = wallBodyType;
                rb.gravityScale = wallGravityScale;
                
                Debug.Log($"✅ 壁修正: {rb.name} (BodyType: {rb.bodyType})");
            }
        }
        
        Debug.Log("壁物理設定修正完了");
    }
    
    /// <summary>
    /// タイルマップの物理設定修正
    /// </summary>
    private void FixTilemapPhysics()
    {
        Debug.Log("タイルマップ物理設定修正中...");
        
        var tilemapColliders = FindObjectsByType<UnityEngine.Tilemaps.TilemapCollider2D>(FindObjectsSortMode.None);
        foreach (var collider in tilemapColliders)
        {
            var rb = collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // タイルマップは静的に設定
                rb.bodyType = RigidbodyType2D.Static;
                rb.gravityScale = 0f;
                
                Debug.Log($"✅ タイルマップ修正: {rb.name}");
            }
        }
        
        var compositeColliders = FindObjectsByType<CompositeCollider2D>(FindObjectsSortMode.None);
        foreach (var collider in compositeColliders)
        {
            var rb = collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // コンポジットコライダーも静的に設定
                rb.bodyType = RigidbodyType2D.Static;
                rb.gravityScale = 0f;
                
                Debug.Log($"✅ コンポジット修正: {rb.name}");
            }
        }
        
        Debug.Log("タイルマップ物理設定修正完了");
    }
    
    /// <summary>
    /// プロジェクト全体の物理設定確認
    /// </summary>
    private void FixProjectPhysics()
    {
        Debug.Log("プロジェクト物理設定確認中...");
        
        // Physics2D設定の確認（読み取り専用）
        Vector2 gravity = Physics2D.gravity;
        Debug.Log($"現在の重力設定: {gravity}");
        
        if (gravity.y < -5f)
        {
            Debug.LogWarning("⚠️ 重力が強すぎる可能性があります");
            Debug.LogWarning("Edit > Project Settings > Physics 2D で重力を調整することを推奨");
            Debug.LogWarning("推奨設定: Gravity Y = 0 (トップダウンゲームの場合)");
        }
        
        Debug.Log("プロジェクト物理設定確認完了");
    }
    
    /// <summary>
    /// 現在の物理状態をレポート
    /// </summary>
    [ContextMenu("物理状態をレポート")]
    public void ReportPhysicsState()
    {
        Debug.Log("=== 物理状態レポート ===");
        
        // プレイヤー状態
        var players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Debug.Log($"プレイヤー {player.name}: BodyType={rb.bodyType}, GravityScale={rb.gravityScale}");
            }
        }
        
        // Rigidbody2D全体の状態
        var rigidbodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        int dynamicCount = 0;
        int staticCount = 0;
        int kinematicCount = 0;
        int fallingCount = 0;
        
        foreach (var rb in rigidbodies)
        {
            switch (rb.bodyType)
            {
                case RigidbodyType2D.Dynamic: dynamicCount++; break;
                case RigidbodyType2D.Static: staticCount++; break;
                case RigidbodyType2D.Kinematic: kinematicCount++; break;
            }
            
            if (rb.gravityScale > 0.1f) fallingCount++;
        }
        
        Debug.Log($"Rigidbody2D統計:");
        Debug.Log($"  Dynamic: {dynamicCount}, Static: {staticCount}, Kinematic: {kinematicCount}");
        Debug.Log($"  重力の影響を受ける: {fallingCount}");
        Debug.Log($"  プロジェクト重力: {Physics2D.gravity}");
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 特定オブジェクトの物理設定表示
    /// </summary>
    public void DebugRigidbody(Rigidbody2D rb)
    {
        if (rb == null) return;
        
        Debug.Log($"=== {rb.name} 物理設定 ===");
        Debug.Log($"BodyType: {rb.bodyType}");
        Debug.Log($"GravityScale: {rb.gravityScale}");
        Debug.Log($"Mass: {rb.mass}");
        Debug.Log($"Constraints: {rb.constraints}");
        Debug.Log($"CollisionDetection: {rb.collisionDetectionMode}");
        Debug.Log("==========================");
    }
} 