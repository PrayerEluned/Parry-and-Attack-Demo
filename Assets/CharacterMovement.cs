using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// プレイヤーキャラクターの移動とレイヤー制御を行います。
/// Cainosアセットの設計に準拠し、安定した移動と衝突判定を実現します。
/// </summary>
public class CharacterMovement : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private VariableJoystick variableJoystick;

    [Header("コンポーネント")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Animator parameter existence flags
    private bool hasIsMovingParam;
    private bool hasDirectionParam;
    private bool hasMoveXParam;
    private bool hasMoveYParam;

    // 現在の向き (0:Front, 1:Back, 2:Right, 3:Left)
    public int FacingDirectionIndex { get; private set; } = 0;

    // フォールバック用：現在再生中のステート名（AM Player S/N/E/W）
    private string currentAnimState = string.Empty;

    [Header("状態")]
    private Vector2 moveInput;
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down;
    public bool CanMove { get; set; } = true;

    private string directionParamName = "Direction";

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Check animator parameters once
        if (animator != null)
        {
            foreach (var p in animator.parameters)
            {
                switch (p.name)
                {
                    case "IsMoving":
                        hasIsMovingParam = true; break;
                    case "Direction":
                    case "direction":
                        hasDirectionParam = true;
                        directionParamName = p.name;
                        break;
                    case "MoveX":
                        hasMoveXParam = true; break;
                    case "MoveY":
                        hasMoveYParam = true; break;
                }
            }
        }
    }

    void Start()
    {
        // 起動時にプレイヤーの初期レイヤーを設定
        string initialLayer = LayerMask.LayerToName(gameObject.layer);
        if (string.IsNullOrEmpty(initialLayer) || initialLayer == "Default")
        {
            initialLayer = "Layer 1";
        }
        ChangeLayer(initialLayer);
    }

    void Update()
    {
        if (!CanMove)
        {
            moveInput = Vector2.zero;
            return;
        }

        // キーボード入力
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 keyboardInput = new Vector2(moveX, moveY).normalized;

        // ジョイスティック入力
        Vector2 joystickInput = (variableJoystick != null && variableJoystick.Direction.magnitude > 0.1f)
            ? variableJoystick.Direction.normalized
            : Vector2.zero;
        
        // ジョイスティック入力を優先
        moveInput = (joystickInput.magnitude > keyboardInput.magnitude) ? joystickInput : keyboardInput;

        // 入力がある場合は最終的な向きを更新
        if (moveInput.magnitude > 0.1f)
        {
            LastMoveDirection = moveInput.normalized;
        }

        UpdateAnimation(moveInput);
    }

    void FixedUpdate()
    {
        if (rb == null || !CanMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // DynamicなRigidbodyを動かす際は、velocityの変更が一般的
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (animator == null) return;

        bool isMoving = direction.magnitude > 0.1f;

        // Animator パラメータを優先的に使う (汎用的な Animator Controller 対応)
        if (hasIsMovingParam) animator.SetBool("IsMoving", isMoving);

        // Direction パラメータが存在する場合は 0:Front(Down)、1:Back(Up)、2:Right、3:Left とする
        if (hasDirectionParam)
        {
            int dirIndex = 0; // Front / Down
            float angle = Mathf.Atan2(LastMoveDirection.y, LastMoveDirection.x) * Mathf.Rad2Deg;
            if (angle > 45f && angle <= 135f) dirIndex = 3;       // Back / Up
            else if (angle > -45f && angle <= 45f) dirIndex = 2;  // Right
            else if (angle > 135f || angle <= -135f) dirIndex = 1; // Left
            // Front / Down が既定 (0) なのでその他範囲は 0 のまま

            animator.SetInteger(directionParamName, dirIndex);

            // 外部で利用できるように向きを保持
            FacingDirectionIndex = dirIndex;
        }

        if (!isMoving)
        {
            // Idle 時は Animator のステートマシンに任せる (IsMoving=false かつ Direction設定済み)
            return;
        }

        // --- 動き中のフォールバック処理 ---
        if (!hasDirectionParam)
        {
            float deg = Mathf.Atan2(LastMoveDirection.y, LastMoveDirection.x) * Mathf.Rad2Deg;

            string state = "AM Player S"; // Front / Down (South)
            if (deg > 45f && deg <= 135f)
                state = "AM Player N"; // Up (North)
            else if (deg > -45f && deg <= 45f)
                state = "AM Player E"; // Right (East)
            else if (deg > 135f || deg <= -135f)
                state = "AM Player W"; // Left (West)
            else if (deg > -135f && deg <= -45f)
                state = "AM Player S";

            if (currentAnimState != state)
            {
                animator.Play(state);
                currentAnimState = state;
            }
        }

        // 既存の MoveX/MoveY パラメータがある場合はブレンド用に値を渡す
        if (hasMoveXParam && hasMoveYParam)
        {
            Vector2 norm = LastMoveDirection.normalized;
            animator.SetFloat("MoveX", norm.x);
            animator.SetFloat("MoveY", norm.y);
        }
    }
    
    /// <summary>
    /// プレイヤーのレイヤーと、それに伴う物理衝突設定を更新します。
    /// Cainosアセットの階段トリガーなどから呼び出されることを想定しています。
    /// </summary>
    /// <param name="newLayerName">移動先の新しいレイヤー名 (例: "Layer 1", "Layer 2")</param>
    public void ChangeLayer(string newLayerName)
    {
        int newLayerIndex = LayerMask.NameToLayer(newLayerName);
        if (newLayerIndex == -1)
        {
            Debug.LogError($"[CharacterMovement] レイヤー '{newLayerName}' が見つかりません。UnityのTag and Layers設定を確認してください。");
            return;
        }

        // プレイヤー自身のゲームオブジェクトのレイヤーを変更
        gameObject.layer = newLayerIndex;

        // スプライトのソーティングレイヤーも同じ名前に変更
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = newLayerName;
        }

        // Dynamicモードでは物理エンジンに衝突を任せるため、手動でのマトリックス更新は不要
        // UpdateCollisionMatrix(newLayerIndex);

        Debug.Log($"プレイヤーのレイヤーが '{newLayerName}' (インデックス: {newLayerIndex}) に変更されました。");
    }

    /// <summary>
    /// 現在のプレイヤーレイヤーに基づいて、他のレイヤーとの衝突設定を更新します。
    /// </summary>
    /// <param name="currentPlayerLayerIndex">プレイヤーの現在のレイヤーインデックス</param>
    private void UpdateCollisionMatrix(int currentPlayerLayerIndex)
    {
        // 32個全てのレイヤーをチェック
        for (int i = 0; i < 32; i++)
        {
            // 自分自身のレイヤーとは常に衝突する（Ignoreしない）
            if (i == currentPlayerLayerIndex)
            {
                Physics2D.IgnoreLayerCollision(currentPlayerLayerIndex, i, false);
                continue;
            }

            string layerName = LayerMask.LayerToName(i);
            
            // "Layer"で始まる名前のレイヤー（Cainosアセットの階層レイヤー）の場合
            if (layerName.StartsWith("Layer"))
            {
                // 現在いる階層のレイヤーとは衝突させ、他の階層のレイヤーとは衝突させない
                bool shouldIgnore = (i != currentPlayerLayerIndex);
                Physics2D.IgnoreLayerCollision(currentPlayerLayerIndex, i, shouldIgnore);
                 Debug.Log($"'{LayerMask.LayerToName(currentPlayerLayerIndex)}' と '{layerName}' の衝突を {(shouldIgnore ? "無効" : "有効")} に設定");
            }
            // それ以外のレイヤー（Default, Wallなど）は、個別の要件に応じて設定する。
            // ここでは、デフォルトの物理設定に任せるため、明示的な変更は行わない。
        }
    }

    /// <summary>
    /// プレイヤーの移動を有効または無効にします。
    /// </summary>
    /// <param name="enable">trueで有効、falseで無効</param>
    public void EnableMovement(bool enable)
    {
        CanMove = enable;
    }
}

/// <summary>
/// 移動情報構造体
/// </summary>
[System.Serializable]
public struct MovementInfo
{
    public Vector2 position;
    public Vector2 velocity;
    public Vector2 lastMoveDirection;
    public bool canMove;
    public int currentLayer;
    public LayerMask wallLayerMask;
}