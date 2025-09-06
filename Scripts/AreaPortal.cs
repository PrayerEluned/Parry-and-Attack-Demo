using UnityEngine;

public class AreaPortal : MonoBehaviour
{
    [Header("ポータル設定")]
    [SerializeField] private string targetAreaName; // 移動先のエリア名
    [SerializeField] private bool requireInteraction = false; // インタラクションが必要か
    [SerializeField] private KeyCode interactionKey = KeyCode.E; // インタラクションキー
    
    [Header("UI設定")]
    [SerializeField] private GameObject interactionPrompt; // "Eキーで移動" 表示
    
    private bool playerInRange = false;
    private PlayerStats player;

    private void Start()
    {
        // インタラクションプロンプトを非表示に設定
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        // インタラクション型の場合の処理
        if (requireInteraction && playerInRange && Input.GetKeyDown(interactionKey))
        {
            TeleportToArea();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーがポータルに入った時の処理
        if (other.CompareTag("Player") || other.GetComponent<PlayerStats>() != null)
        {
            player = other.GetComponent<PlayerStats>();
            if (player == null) 
                player = other.GetComponentInParent<PlayerStats>();

            playerInRange = true;

            if (requireInteraction)
            {
                // インタラクションが必要な場合はプロンプトを表示
                if (interactionPrompt != null)
                    interactionPrompt.SetActive(true);
                
                Debug.Log($"Eキーを押して '{targetAreaName}' エリアに移動");
            }
            else
            {
                // 自動移動の場合は即座にテレポート
                TeleportToArea();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // プレイヤーがポータルから出た時の処理
        if (other.CompareTag("Player") || other.GetComponent<PlayerStats>() != null)
        {
            playerInRange = false;
            player = null;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    private void TeleportToArea()
    {
        // エリア機能を無効化（フリーズ対策）
        Debug.Log("AreaPortal: エリア機能は無効化されています。テレポートをスキップします");
        return;
        
        if (AreaManager.Instance == null)
        {
            Debug.LogError("AreaManager.Instance が見つかりません");
            return;
        }

        if (string.IsNullOrEmpty(targetAreaName))
        {
            Debug.LogError("移動先エリア名が設定されていません");
            return;
        }

        Debug.Log($"ポータル実行: {targetAreaName} エリアに移動中...");
        AreaManager.Instance.SwitchToArea(targetAreaName, true);

        // インタラクションプロンプトを非表示
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        // シーンビューでポータルの範囲を可視化
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = requireInteraction ? Color.yellow : Color.green;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
            
            // 移動先の名前を表示
            if (!string.IsNullOrEmpty(targetAreaName))
            {
                Gizmos.color = Color.white;
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, $"→ {targetAreaName}");
                #endif
            }
        }
    }
}