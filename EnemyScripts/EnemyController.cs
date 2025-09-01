using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("検出設定")]
    [SerializeField] private float detectRange = 75f;
    [SerializeField] private float attackRange = 50f;
    
    [Header("追跡設定")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 2f;
    
    [Header("デバッグ設定")]
    [SerializeField] private bool showDebugInfo = false;

    private Transform player;
    private EnemyHealth health;
    private EnemyAttackSystem attackSystem;

    private void Start()
    {
        try
        {
            // 基本的な敵の初期化
            InitializeEnemy();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EnemyController: 初期化エラー - {e.Message}");
        }
    }
    
    private void InitializeEnemy()
    {
        // 基本的な敵の初期化処理
        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
        }
        
        if (attackSystem == null)
        {
            attackSystem = GetComponent<EnemyAttackSystem>();
        }
        
        // プレイヤーの参照取得
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogWarning("EnemyController: プレイヤーが見つかりません");
            }
            else
            {
                player = playerObj.transform; // Transformとして設定
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"EnemyController: 初期化完了 - 攻撃システム: {(attackSystem != null ? "有効" : "無効")}");
        }
    }

    private void Update()
    {
        try
        {
            // 軽量な敵のUpdate処理
            if (player != null)
            {
                // 基本的な敵の行動処理
                UpdateEnemyBehavior();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EnemyController: Update エラー - {e.Message}");
        }
    }
    
    private void UpdateEnemyBehavior()
    {
        // プレイヤーとの距離チェック
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // 基本的な敵の行動ロジック
            if (distanceToPlayer < detectRange) // 検出範囲
            {
                // 攻撃範囲内の場合
                if (distanceToPlayer <= attackRange)
                {
                    // 攻撃可能な場合は攻撃
                    if (attackSystem != null && attackSystem.CanAttack)
                    {
                        if (showDebugInfo)
                        {
                            Debug.Log($"EnemyController: 攻撃開始 - プレイヤー距離: {distanceToPlayer:F1}");
                        }
                        
                        attackSystem.StartAttack();
                    }
                }
                else
                {
                    // 攻撃範囲外の場合は追跡
                    if (distanceToPlayer > stopDistance)
                    {
                        MoveTowardsPlayer();
                        
                        if (showDebugInfo && Time.frameCount % 60 == 0) // 1秒に1回程度ログ出力
                        {
                            Debug.Log($"EnemyController: プレイヤーを追跡中 - 距離: {distanceToPlayer:F1}");
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// プレイヤーに向かって移動
    /// </summary>
    private void MoveTowardsPlayer()
    {
        if (player == null) return;
        
        // プレイヤーの方向を計算
        Vector3 direction = (player.position - transform.position).normalized;
        
        // 移動（敵自体は回転しない）
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        if (showDebugInfo && Time.frameCount % 120 == 0) // 2秒に1回程度ログ出力
        {
            Debug.Log($"EnemyController: 移動中 - 方向: {direction}, 速度: {moveSpeed}");
        }
    }
    
    // デバッグ用のGizmos描画
    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // 検出範囲を可視化
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        
        // 攻撃範囲を可視化
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 停止距離を可視化
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        
        // プレイヤーへの方向を可視化
        if (player != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}