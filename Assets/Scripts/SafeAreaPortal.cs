using UnityEngine;
using AudioSystem;

/// <summary>
/// フリーズ対策済みワープポータル
/// SafeAreaManagerと連動してエリア間瞬間移動を実現
/// </summary>
public class SafeAreaPortal : MonoBehaviour
{
    [Header("ワープ設定")]
    [SerializeField] private string targetAreaName;         // 移動先エリア名
    [SerializeField] private bool requireInteraction = false; // Eキー必要か
    [SerializeField] private KeyCode interactionKey = KeyCode.E; // インタラクションキー
    [SerializeField] private bool useTransition = true;      // 遷移エフェクト使用
    
    [Header("UI設定")]
    [SerializeField] private GameObject interactionPrompt;   // "Eキーでワープ" 表示
    [SerializeField] private string promptMessage = "Eキーでワープ"; // プロンプトメッセージ
    
    [Header("エフェクト設定")]
    [SerializeField] private GameObject warpEffect;          // ワープエフェクト
    [SerializeField] private AudioClip warpSE;               // ワープ音
    [SerializeField] private bool showWarpEffect = true;     // エフェクト表示
    
    [Header("デバッグ")]
    [SerializeField] private bool enableDebugLogs = true;    // デバッグログ
    [SerializeField] private Color gizmoColor = Color.cyan;  // ギズモ色
    
    private bool playerInRange = false;
    private PlayerStats player;
    private AudioSystem.AudioManager audioManager;

    private void Start()
    {
        // 初期化
        audioManager = AudioSystem.AudioManager.Instance;
        
        // インタラクションプロンプトを非表示
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
            
        // ワープエフェクトを非表示
        if (warpEffect != null)
            warpEffect.SetActive(false);
            
        // SafeAreaManagerの存在確認
        if (SafeAreaManager.Instance == null)
        {
            Debug.LogWarning("SafeAreaPortal: SafeAreaManagerが見つかりません。ワープ機能が利用できません");
        }
        else if (!SafeAreaManager.Instance.HasArea(targetAreaName))
        {
            Debug.LogError($"SafeAreaPortal: 移動先エリア '{targetAreaName}' が存在しません。利用可能エリア: {string.Join(", ", SafeAreaManager.Instance.GetAreaNames())}");
        }
        
        if (enableDebugLogs) Debug.Log($"SafeAreaPortal: 初期化完了 - {targetAreaName}へのワープポータル");
    }

    private void Update()
    {
        // インタラクション型の処理
        if (requireInteraction && playerInRange && Input.GetKeyDown(interactionKey))
        {
            ExecuteWarp();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤー検出
        if (IsPlayer(other))
        {
            player = GetPlayerStats(other);
            playerInRange = true;

            if (requireInteraction)
            {
                // インタラクション型：プロンプト表示
                ShowInteractionPrompt(true);
                if (enableDebugLogs) Debug.Log($"SafeAreaPortal: インタラクション可能 - {promptMessage}");
            }
            else
            {
                // 自動型：即座にワープ
                ExecuteWarp();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // プレイヤーが離れた
        if (IsPlayer(other))
        {
            playerInRange = false;
            player = null;
            ShowInteractionPrompt(false);
        }
    }
    
    /// <summary>
    /// ワープ実行
    /// </summary>
    private void ExecuteWarp()
    {
        // SafeAreaManagerの存在確認
        if (SafeAreaManager.Instance == null)
        {
            Debug.LogError("SafeAreaPortal: SafeAreaManagerが見つかりません");
            return;
        }

        // 移動先エリアの確認
        if (string.IsNullOrEmpty(targetAreaName))
        {
            Debug.LogError("SafeAreaPortal: 移動先エリア名が設定されていません");
            return;
        }

        if (!SafeAreaManager.Instance.HasArea(targetAreaName))
        {
            Debug.LogError($"SafeAreaPortal: エリア '{targetAreaName}' が存在しません");
            return;
        }

        if (enableDebugLogs) Debug.Log($"SafeAreaPortal: ワープ実行 - {targetAreaName}");

        // エフェクト再生
        if (showWarpEffect)
        {
            PlayWarpEffect();
        }

        // 音声再生
        if (audioManager != null && warpSE != null)
        {
            audioManager.PlaySE(warpSE);
        }

        // プロンプト非表示
        ShowInteractionPrompt(false);

        // ワープ実行
        SafeAreaManager.Instance.WarpToArea(targetAreaName, useTransition);
    }
    
    /// <summary>
    /// ワープエフェクトの再生
    /// </summary>
    private void PlayWarpEffect()
    {
        if (warpEffect != null)
        {
            warpEffect.SetActive(true);
            
            // 一定時間後に非表示
            Invoke(nameof(HideWarpEffect), 1f);
        }
    }
    
    /// <summary>
    /// ワープエフェクトを非表示
    /// </summary>
    private void HideWarpEffect()
    {
        if (warpEffect != null)
        {
            warpEffect.SetActive(false);
        }
    }
    
    /// <summary>
    /// インタラクションプロンプトの表示/非表示
    /// </summary>
    private void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(show);
        }
    }
    
    /// <summary>
    /// プレイヤーかどうかの判定
    /// </summary>
    private bool IsPlayer(Collider2D collider)
    {
        return collider.CompareTag("Player") || collider.GetComponent<PlayerStats>() != null;
    }
    
    /// <summary>
    /// PlayerStatsコンポーネントの取得
    /// </summary>
    private PlayerStats GetPlayerStats(Collider2D collider)
    {
        var stats = collider.GetComponent<PlayerStats>();
        if (stats == null)
        {
            stats = collider.GetComponentInParent<PlayerStats>();
        }
        return stats;
    }
    
    /// <summary>
    /// 移動先エリア名を設定
    /// </summary>
    public void SetTargetArea(string areaName)
    {
        targetAreaName = areaName;
        if (enableDebugLogs) Debug.Log($"SafeAreaPortal: 移動先エリアを変更 - {areaName}");
    }
    
    /// <summary>
    /// インタラクション要件を設定
    /// </summary>
    public void SetRequireInteraction(bool require)
    {
        requireInteraction = require;
        if (!require && interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        // ワープポータルの可視化
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // ポータルの範囲
            Gizmos.color = requireInteraction ? Color.yellow : gizmoColor;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
            
            // ワープ先の表示
            if (!string.IsNullOrEmpty(targetAreaName))
            {
                Gizmos.color = Color.white;
                #if UNITY_EDITOR
                string displayText = $"WARP → {targetAreaName}";
                if (requireInteraction) displayText += " (E)";
                UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, displayText);
                #endif
            }
            
            // ワープ方向の矢印
            Gizmos.color = gizmoColor;
            Vector3 arrowStart = transform.position;
            Vector3 arrowEnd = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawLine(arrowStart, arrowEnd);
            
            // 矢印の先端
            Vector3 arrowHead1 = arrowEnd + (Vector3.left + Vector3.down) * 0.1f;
            Vector3 arrowHead2 = arrowEnd + (Vector3.right + Vector3.down) * 0.1f;
            Gizmos.DrawLine(arrowEnd, arrowHead1);
            Gizmos.DrawLine(arrowEnd, arrowHead2);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // 選択時の詳細表示
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        #if UNITY_EDITOR
        if (SafeAreaManager.Instance != null)
        {
            var areaNames = SafeAreaManager.Instance.GetAreaNames();
            string info = $"利用可能エリア:\n{string.Join("\n", areaNames)}";
            UnityEditor.Handles.Label(transform.position + Vector3.down * 2f, info);
        }
        #endif
    }
} 