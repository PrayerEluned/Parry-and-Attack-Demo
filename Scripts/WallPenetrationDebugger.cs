using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

/// <summary>
/// 壁抜け問題をリアルタイムで診断・修正するツール
/// CainosのStairsLayerTriggerシステムに基づく正しいレイヤー制御
/// CharacterMovementの壁抜け問題も修正
/// 
/// 注意：統合版CharacterMovementが存在するため無効化されています
/// </summary>
public class WallPenetrationDebugger : MonoBehaviour
{
    [Header("診断設定")]
    [SerializeField] private bool enableRealtimeDebug = true;
    [SerializeField] private float debugUpdateInterval = 0.5f;
    [SerializeField] private bool showDetailedInfo = true;
    
    [Header("修正設定")]
    [SerializeField] private bool autoFixIssues = true;
    [SerializeField] private float maxAllowedSpeed = 4f;
    [SerializeField] private bool enableMovementFix = true;
    [SerializeField] private float maxCollisionCheckDistance = 0.6f;
    
    [Header("デバッグ用設定")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private bool enableDebugKeys = true;
    
    [Header("レイヤー設定")]
    [SerializeField] private string cainosLayer1 = "Layer 1";
    [SerializeField] private string cainosLayer2 = "Layer 2";
    [SerializeField] private string cainosLayer3 = "Layer 3";
    [SerializeField] private string sortingLayer1 = "Layer 1";
    [SerializeField] private string sortingLayer2 = "Layer 2";
    [SerializeField] private string sortingLayer3 = "Layer 3";
    
    [Header("テスト設定")]
    [SerializeField] private bool enableCollisionTest = true;
    [SerializeField] private float testRayDistance = 1f;
    
    private CharacterMovement characterMovement;
    private Rigidbody2D playerRb;
    private Collider2D playerCollider;
    private SpriteRenderer playerSpriteRenderer;
    private float lastDebugUpdate;
    private bool isOriginalMovementDisabled = false;
    
    private void Start()
    {
        // 統合版CharacterMovementが存在するため無効化
        Debug.Log("WallPenetrationDebugger: 統合版CharacterMovementが壁抜け対策を内蔵しているため無効化されています");
        return;
        
        // プレイヤーコンポーネントを取得
        characterMovement = GetComponent<CharacterMovement>();
        playerRb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        
        if (showDetailedInfo)
        {
            Debug.Log("=== Cainos壁抜け診断ツール開始 ===");
            DiagnoseInitialSetup();
            DiagnoseCharacterMovement();
        }
    }
    
    private void Update()
    {
        // 統合版CharacterMovementが存在するため無効化
        return;
        
        if (enableRealtimeDebug && Time.time - lastDebugUpdate > debugUpdateInterval)
        {
            PerformRealtimeDebug();
            lastDebugUpdate = Time.time;
        }
        
        // 修正版の移動処理
        if (enableMovementFix && characterMovement != null)
        {
            HandleSafeMovement();
        }
    }
    
    private void FixedUpdate()
    {
        // 統合版CharacterMovementが存在するため無効化
        return;
        
        if (enableCollisionTest && playerRb != null)
        {
            TestCollisionDetection();
        }
    }
    
    /// <summary>
    /// CharacterMovementの問題を診断
    /// </summary>
    private void DiagnoseCharacterMovement()
    {
        Debug.Log("--- CharacterMovement診断 ---");
        
        if (characterMovement == null)
        {
            Debug.LogError("⚠️ CharacterMovementが見つかりません");
            return;
        }
        
        // リフレクションで基本移動速度を取得
        var speedField = characterMovement.GetType().GetField("baseMoveSpeed", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (speedField != null)
        {
            float currentSpeed = (float)speedField.GetValue(characterMovement);
            Debug.Log($"基本移動速度: {currentSpeed}");
            
            // PlayerStatsによる最大速度倍率を計算
            PlayerStats playerStats = GetComponent<PlayerStats>();
            if (playerStats != null && playerStats.stats != null)
            {
                float multiplier = Mathf.Clamp(playerStats.stats.TotalSpeed / 1000f * 6f, 1f, 6f);
                float maxSpeed = currentSpeed * multiplier;
                Debug.Log($"最大可能速度: {maxSpeed} (倍率: {multiplier:F1}x)");
                
                if (maxSpeed > maxAllowedSpeed)
                {
                    Debug.LogWarning($"⚠️ 危険な高速移動: {maxSpeed} > {maxAllowedSpeed}");
                    
                    if (autoFixIssues)
                    {
                        Debug.Log("🔧 CharacterMovementを無効化し、安全な移動処理に切り替えます");
                        characterMovement.EnableMovement(false);
                        isOriginalMovementDisabled = true;
                    }
                }
            }
        }
        
        // 衝突チェック機能の有無を確認
        Debug.Log("CharacterMovement問題点:");
        Debug.Log("  - 移動前の衝突チェック: なし ❌");
        Debug.Log("  - 安全な移動処理: なし ❌");
        Debug.Log("  - 速度制限: なし ❌");
        Debug.Log("  - 壁抜け対策: なし ❌");
    }
    
    /// <summary>
    /// 安全な移動処理を実行
    /// </summary>
    private void HandleSafeMovement()
    {
        if (!isOriginalMovementDisabled || playerRb == null || playerCollider == null)
            return;
        
        // キーボード入力に基づく移動方向の計算
        Vector2 keyboardDirection = Vector2.zero;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) keyboardDirection.y -= 1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) keyboardDirection.y += 1;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) keyboardDirection.x -= 1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) keyboardDirection.x += 1;

        // ジョイスティック入力の取得
        var joystickField = characterMovement.GetType().GetField("variableJoystick", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Vector2 joystickDirection = Vector2.zero;
        
        if (joystickField != null)
        {
            var joystick = joystickField.GetValue(characterMovement);
            if (joystick != null)
            {
                var directionProperty = joystick.GetType().GetProperty("Direction");
                if (directionProperty != null)
                {
                    joystickDirection = (Vector2)directionProperty.GetValue(joystick);
                }
            }
        }

        // ジョイスティック入力がない場合はキーボード入力を使用
        Vector2 movementDirection = (joystickDirection.magnitude > 0) ? joystickDirection : keyboardDirection;

        // 移動処理
        if (movementDirection != Vector2.zero)
        {
            // 安全な移動速度（制限あり）
            float safeSpeed = Mathf.Min(maxAllowedSpeed, 3f);
            Vector2 normalizedDirection = movementDirection.normalized;
            
            // 移動先の位置を計算
            Vector2 targetPosition = playerRb.position + normalizedDirection * safeSpeed * Time.deltaTime;
            
            // 衝突チェック
            if (IsSafeToMove(playerRb.position, targetPosition, normalizedDirection))
            {
                playerRb.MovePosition(targetPosition);
                
                // LastMoveDirectionを更新
                var lastMoveDirectionProperty = characterMovement.GetType().GetProperty("LastMoveDirection");
                if (lastMoveDirectionProperty != null)
                {
                    lastMoveDirectionProperty.SetValue(characterMovement, normalizedDirection);
                }
                
                // アニメーション更新
                UpdatePlayerAnimation(normalizedDirection);
            }
            else
            {
                Debug.LogWarning("⚠️ 移動ブロック: 壁との衝突を防ぎました");
            }
        }
    }
    
    /// <summary>
    /// 移動が安全かチェック
    /// </summary>
    private bool IsSafeToMove(Vector2 from, Vector2 to, Vector2 direction)
    {
        // 現在のレイヤーマスクを取得（現在のレイヤーを除外）
        int currentLayer = gameObject.layer;
        int layerMask = ~(1 << currentLayer);
        
        // 移動距離を計算
        float distance = Vector2.Distance(from, to);
        
        // 円形の衝突チェック
        if (playerCollider is CircleCollider2D circleCollider)
        {
            float radius = circleCollider.radius * transform.lossyScale.x;
            
            // 移動先での円形衝突チェック
            Collider2D hit = Physics2D.OverlapCircle(to, radius, layerMask);
            if (hit != null)
            {
                if (showDetailedInfo)
                {
                    Debug.Log($"衝突検出: {hit.name} (Layer: {LayerMask.LayerToName(hit.gameObject.layer)})");
                }
                return false;
            }
            
            // 移動経路での連続チェック
            RaycastHit2D rayHit = Physics2D.CircleCast(from, radius, direction, distance + 0.1f, layerMask);
            if (rayHit.collider != null)
            {
                if (showDetailedInfo)
                {
                    Debug.Log($"経路衝突検出: {rayHit.collider.name} (距離: {rayHit.distance:F3})");
                }
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// プレイヤーアニメーションを更新
    /// </summary>
    private void UpdatePlayerAnimation(Vector2 direction)
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle > -45 && angle <= 45)
            {
                animator.SetInteger("direction", 2); // 右
            }
            else if (angle > 45 && angle <= 135)
            {
                animator.SetInteger("direction", 3); // 下
            }
            else if (angle > -135 && angle <= -45)
            {
                animator.SetInteger("direction", 0); // 左
            }
            else
            {
                animator.SetInteger("direction", 1); // 上
            }
        }
    }
    
    /// <summary>
    /// 初期設定を診断
    /// </summary>
    private void DiagnoseInitialSetup()
    {
        Debug.Log("--- Cainos初期設定診断 ---");
        
        // 1. プレイヤーをLayer 1に設定（開始レイヤー）
        SetCainosLayer(cainosLayer1, sortingLayer1);
        
        Debug.Log($"プレイヤーレイヤー: {LayerMask.LayerToName(gameObject.layer)} ({gameObject.layer})");
        
        if (playerRb != null)
        {
            Debug.Log($"Rigidbody2D設定:");
            Debug.Log($"  - GravityScale: {playerRb.gravityScale}");
            Debug.Log($"  - Drag: {playerRb.linearDamping}");
            Debug.Log($"  - Mass: {playerRb.mass}");
            Debug.Log($"  - CollisionDetection: {playerRb.collisionDetectionMode}");
            Debug.Log($"  - Constraints: {playerRb.constraints}");
            Debug.Log($"  - Interpolation: {playerRb.interpolation}");
        }
        
        if (playerSpriteRenderer != null)
        {
            Debug.Log($"SpriteRenderer設定:");
            Debug.Log($"  - SortingLayer: {playerSpriteRenderer.sortingLayerName}");
            Debug.Log($"  - SortingOrder: {playerSpriteRenderer.sortingOrder}");
        }
        
        // 2. 壁の設定
        DiagnoseWallSettings();
        
        // 3. レイヤー衝突設定（Cainosシステム用）
        SetupCainosCollisions();
        
        // 4. 物理設定
        DiagnosePhysicsSettings();
    }
    
    /// <summary>
    /// Cainosレイヤーを設定（StairsLayerTriggerと同じ方式）
    /// </summary>
    private void SetCainosLayer(string layerName, string sortingLayerName)
    {
        // レイヤーが存在するかチェック
        int layerIndex = LayerMask.NameToLayer(layerName);
        if (layerIndex != -1)
        {
            gameObject.layer = layerIndex;
            Debug.Log($"🔧 Cainosレイヤー設定: {layerName}");
        }
        else
        {
            Debug.LogError($"⚠️ レイヤー '{layerName}' が見つかりません。Project Settings > Tags and Layers で設定してください。");
            return;
        }

        // ソーティングレイヤーが存在するかチェック
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sortingLayerName = sortingLayerName;
            Debug.Log($"🔧 SortingLayer設定: {sortingLayerName}");
        }
        
        // 子オブジェクトのSpriteRendererも設定
        SpriteRenderer[] childSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in childSpriteRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
        }
        
        // 子オブジェクトのレイヤーも設定
        Transform[] childTransforms = GetComponentsInChildren<Transform>();
        foreach (Transform child in childTransforms)
        {
            if (child != transform)
            {
                child.gameObject.layer = layerIndex;
            }
        }
    }
    
    /// <summary>
    /// Cainosシステムの衝突制御を設定
    /// </summary>
    private void SetupCainosCollisions()
    {
        Debug.Log("🔧 Cainosシステム: 衝突制御を設定");
        
        int currentLayer = gameObject.layer;
        int layer1Index = LayerMask.NameToLayer(cainosLayer1);
        int layer2Index = LayerMask.NameToLayer(cainosLayer2);
        int layer3Index = LayerMask.NameToLayer(cainosLayer3);
        
        // Cainosシステム: 現在のレイヤーに対応する壁とのみ衝突
        if (currentLayer == layer1Index)
        {
            // CainosLayer1: Layer 1の壁とのみ衝突
            if (layer2Index != -1) Physics2D.IgnoreLayerCollision(currentLayer, layer2Index, true);
            if (layer3Index != -1) Physics2D.IgnoreLayerCollision(currentLayer, layer3Index, true);
            Debug.Log("Layer 1モード: Layer 1の壁とのみ衝突");
        }
        else if (currentLayer == layer2Index)
        {
            // CainosLayer2: Layer 2の壁とのみ衝突
            if (layer1Index != -1) Physics2D.IgnoreLayerCollision(currentLayer, layer1Index, true);
            if (layer3Index != -1) Physics2D.IgnoreLayerCollision(currentLayer, layer3Index, true);
            Debug.Log("Layer 2モード: Layer 2の壁とのみ衝突");
        }
        else if (currentLayer == layer3Index)
        {
            // CainosLayer3: Layer 3の壁とのみ衝突
            if (layer1Index != -1) Physics2D.IgnoreLayerCollision(currentLayer, layer1Index, true);
            if (layer2Index != -1) Physics2D.IgnoreLayerCollision(currentLayer, layer2Index, true);
            Debug.Log("Layer 3モード: Layer 3の壁とのみ衝突");
        }
        
        // DefaultレイヤーとTriggerレイヤーとは必ず衝突
        Physics2D.IgnoreLayerCollision(currentLayer, 0, false); // Default
        
        // StairsLayerTriggerのトリガーとも衝突する必要がある
        int triggerLayer = LayerMask.NameToLayer("Trigger");
        if (triggerLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(currentLayer, triggerLayer, false);
        }
        
        // 診断情報を表示
        DisplayCainosLayerStatus();
    }
    
    /// <summary>
    /// Cainosレイヤー状態を表示
    /// </summary>
    private void DisplayCainosLayerStatus()
    {
        Debug.Log("=== Cainosレイヤー衝突状態 ===");
        
        int currentLayer = gameObject.layer;
        string currentLayerName = LayerMask.LayerToName(currentLayer);
        Debug.Log($"現在のレイヤー: {currentLayerName} ({currentLayer})");
        
        if (playerSpriteRenderer != null)
        {
            Debug.Log($"現在のSortingLayer: {playerSpriteRenderer.sortingLayerName}");
        }
        
        string[] testLayers = {"Default", cainosLayer1, cainosLayer2, cainosLayer3, "Trigger"};
        
        foreach (string layerName in testLayers)
        {
            int layerIndex = LayerMask.NameToLayer(layerName);
            if (layerIndex != -1)
            {
                bool canCollide = !Physics2D.GetIgnoreLayerCollision(currentLayer, layerIndex);
                string status = canCollide ? "衝突あり" : "通り抜け";
                Debug.Log($"  {layerName}: {status}");
            }
        }
        
        Vector3 pos = transform.position;
        Debug.Log($"プレイヤー位置: ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
    }
    
    /// <summary>
    /// 壁設定を診断
    /// </summary>
    private void DiagnoseWallSettings()
    {
        Debug.Log("--- 壁設定診断 ---");
        
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
        Debug.Log($"Tilemap数: {tilemaps.Length}");
        
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.name.Contains("Wall"))
            {
                Debug.Log($"壁Tilemap: {tilemap.name}");
                Debug.Log($"  - Layer: {LayerMask.LayerToName(tilemap.gameObject.layer)}");
                
                var tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
                var compositeCollider = tilemap.GetComponent<CompositeCollider2D>();
                
                if (tilemapCollider != null)
                {
                    Debug.Log($"  - TilemapCollider2D: Enabled={tilemapCollider.enabled}, IsTrigger={tilemapCollider.isTrigger}");
                }
                
                if (compositeCollider != null)
                {
                    Debug.Log($"  - CompositeCollider2D: Enabled={compositeCollider.enabled}, PathCount={compositeCollider.pathCount}");
                }
            }
        }
    }
    
    /// <summary>
    /// 物理設定を診断
    /// </summary>
    private void DiagnosePhysicsSettings()
    {
        Debug.Log("--- 物理設定診断 ---");
        Debug.Log($"重力: {Physics2D.gravity}");
        Debug.Log($"デフォルト衝突検出: {Physics2D.defaultContactOffset}");
        Debug.Log($"固定タイムステップ: {Time.fixedDeltaTime}");
        
        // 移動速度チェック
        if (characterMovement != null)
        {
            var speedField = characterMovement.GetType().GetField("baseMoveSpeed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (speedField != null)
            {
                float currentSpeed = (float)speedField.GetValue(characterMovement);
                Debug.Log($"基本移動速度: {currentSpeed}");
                
                if (currentSpeed > maxAllowedSpeed && autoFixIssues)
                {
                    speedField.SetValue(characterMovement, maxAllowedSpeed);
                    Debug.Log($"  → 自動修正: 移動速度を {maxAllowedSpeed} に制限");
                }
            }
        }
    }
    
    /// <summary>
    /// リアルタイム診断
    /// </summary>
    private void PerformRealtimeDebug()
    {
        if (playerRb == null) return;
        
        Vector2 velocity = playerRb.linearVelocity;
        Vector2 position = playerRb.position;
        
        if (showDetailedInfo)
        {
            Debug.Log($"--- リアルタイム診断 ---");
            Debug.Log($"位置: {position}");
            Debug.Log($"速度: {velocity} (大きさ: {velocity.magnitude:F2})");
            Debug.Log($"移動処理: {(isOriginalMovementDisabled ? "安全版" : "原版")}");
        }
        
        // 速度異常チェック
        if (velocity.magnitude > maxAllowedSpeed + 1f)
        {
            Debug.LogWarning($"⚠️ 速度異常: {velocity.magnitude:F2} > {maxAllowedSpeed}");
            
            if (autoFixIssues)
            {
                playerRb.linearVelocity = velocity.normalized * maxAllowedSpeed;
                Debug.Log("自動修正: 速度を制限しました");
            }
        }
    }
    
    /// <summary>
    /// 衝突検出テスト
    /// </summary>
    private void TestCollisionDetection()
    {
        if (playerRb.linearVelocity.magnitude < 0.1f) return;
        
        Vector2 position = playerRb.position;
        Vector2 direction = playerRb.linearVelocity.normalized;
        
        // 移動方向に壁があるかチェック
        RaycastHit2D hit = Physics2D.Raycast(position, direction, testRayDistance, ~(1 << gameObject.layer));
        
        if (hit.collider != null)
        {
            string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
            if (layerName.Contains("Wall") || layerName.Contains("Layer"))
            {
                float distance = hit.distance;
                if (distance < 0.1f)
                {
                    Debug.LogWarning($"⚠️ 壁接触直前: {hit.collider.name} (距離: {distance:F3})");
                }
                
                // 衝突予測
                if (playerRb.linearVelocity.magnitude * Time.fixedDeltaTime > distance)
                {
                    Debug.LogWarning($"⚠️ 壁抜け予測: 速度({playerRb.linearVelocity.magnitude:F2}) × 時間({Time.fixedDeltaTime:F4}) > 距離({distance:F3})");
                }
            }
        }
    }
    
    /// <summary>
    /// Cainosレイヤー変更（StairsLayerTriggerを模倣）
    /// </summary>
    [ContextMenu("Layer 1に移動")]
    public void MoveToCainosLayer1()
    {
        SetCainosLayer(cainosLayer1, sortingLayer1);
    }
    
    [ContextMenu("Layer 2に移動")]
    public void MoveToCainosLayer2()
    {
        SetCainosLayer(cainosLayer2, sortingLayer2);
    }
    
    [ContextMenu("Layer 3に移動")]
    public void MoveToCainosLayer3()
    {
        SetCainosLayer(cainosLayer3, sortingLayer3);
    }
    
    /// <summary>
    /// 手動修正実行
    /// </summary>
    [ContextMenu("緊急修正実行")]
    public void EmergencyFix()
    {
        Debug.Log("=== 緊急修正実行 ===");
        
        // 1. 速度制限
        if (playerRb != null && playerRb.linearVelocity.magnitude > maxAllowedSpeed)
        {
            playerRb.linearVelocity = playerRb.linearVelocity.normalized * maxAllowedSpeed;
            Debug.Log("速度を制限しました");
        }
        
        // 2. Cainosシステムの衝突制御を再設定
        SetupCainosCollisions();
        
        // 3. 物理設定最適化
        if (playerRb != null)
        {
            playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            playerRb.interpolation = RigidbodyInterpolation2D.Interpolate;
            Debug.Log("物理設定を最適化しました");
        }
        
        // 4. 安全な移動処理に切り替え
        if (characterMovement != null && !isOriginalMovementDisabled)
        {
            characterMovement.EnableMovement(false);
            isOriginalMovementDisabled = true;
            Debug.Log("🔧 安全な移動処理に切り替えました");
        }
        
        Debug.Log("緊急修正完了");
    }
    
    /// <summary>
    /// 元の移動処理に戻す
    /// </summary>
    [ContextMenu("元の移動処理に戻す")]
    public void RestoreOriginalMovement()
    {
        if (characterMovement != null && isOriginalMovementDisabled)
        {
            characterMovement.EnableMovement(true);
            isOriginalMovementDisabled = false;
            Debug.Log("🔧 元の移動処理に戻しました");
        }
    }
    
    /// <summary>
    /// 診断機能を一時無効化
    /// </summary>
    [ContextMenu("診断機能を無効化")]
    public void DisableDebugTemporarily()
    {
        enableRealtimeDebug = false;
        enableCollisionTest = false;
        autoFixIssues = false;
        Debug.Log("診断機能を一時無効化しました");
    }
    
    /// <summary>
    /// 診断機能を再有効化
    /// </summary>
    [ContextMenu("診断機能を有効化")]
    public void EnableDebugAgain()
    {
        enableRealtimeDebug = true;
        enableCollisionTest = true;
        autoFixIssues = true;
        Debug.Log("診断機能を再有効化しました");
    }
    
    private void OnDrawGizmos()
    {
        if (enableCollisionTest && playerRb != null && playerRb.linearVelocity.magnitude > 0.1f)
        {
            // 移動方向を表示
            Gizmos.color = Color.red;
            Vector2 pos = transform.position;
            Vector2 dir = playerRb.linearVelocity.normalized;
            Gizmos.DrawRay(pos, dir * testRayDistance);
            
            // 速度ベクトルを表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(pos, playerRb.linearVelocity * 0.1f);
        }
        
        // 衝突チェック範囲を表示
        if (enableMovementFix && playerCollider is CircleCollider2D circleCollider)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, circleCollider.radius * transform.lossyScale.x);
        }
    }
} 