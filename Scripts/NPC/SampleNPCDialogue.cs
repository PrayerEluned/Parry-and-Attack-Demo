using UnityEngine;
using NPCSystem;

/// <summary>
/// NPC会話システムの使用例
/// このスクリプトはNPCDialogueSystemの設定例を示します
/// </summary>
public class SampleNPCDialogue : MonoBehaviour
{
    [Header("NPC設定例")]
    [SerializeField] private string npcName = "村人A";
    [SerializeField] private string npcID = "villager_001";
    
    [Header("会話内容例")]
    [SerializeField] private string[] firstDialogueLines = { 
        "こんにちは！", 
        "私はこの村の村人です。", 
        "あなたに特別なpatchをあげますね！" 
    };
    
    [SerializeField] private string[] repeatDialogueLines = { 
        "また来てくれてありがとう！", 
        "何か新しいことがあれば教えてください。" 
    };
    
    [SerializeField] private string[] hasPatchDialogueLines = { 
        "あ、あなたは既にそのpatchを持っていますね！", 
        "他に何かお手伝いできることはありますか？" 
    };
    
    [Header("Patch設定例")]
    [SerializeField] private string patchDialogueLine = "この攻撃力強化patchをあなたにあげます！";
    [SerializeField] private string patchReceivedDialogueLine = "ありがとうございます！大切に使います。";
    
    [Header("プレイヤー設定")]
    [SerializeField] private GameObject player;
    
    private NPCDialogueSystem dialogueSystem;
    
    private void Start()
    {
        // NPCDialogueSystemコンポーネントを取得
        dialogueSystem = GetComponent<NPCDialogueSystem>();
        
        if (dialogueSystem == null)
        {
            Debug.LogError("SampleNPCDialogue: NPCDialogueSystemコンポーネントが見つかりません。");
            return;
        }
        
        // 設定を適用
        ApplyDialogueSettings();
        
        Debug.Log($"SampleNPCDialogue: 初期化完了 - NPC: {npcName}");
    }
    
    /// <summary>
    /// 会話設定を適用
    /// </summary>
    private void ApplyDialogueSettings()
    {
        // このメソッドはNPCDialogueSystemの設定を動的に変更する例です
        // 実際の使用では、Inspectorで設定することを推奨します
        
        // 注意: この例ではNPCDialogueSystemのprivateフィールドにアクセスできないため、
        // 実際の実装では、NPCDialogueSystemにpublicメソッドを追加することを推奨します
        
        Debug.Log($"SampleNPCDialogue: 会話設定を適用 - NPC: {npcName}");
    }
    
    /// <summary>
    /// 会話を手動で開始（デバッグ用）
    /// </summary>
    [ContextMenu("会話を開始")]
    public void StartDialogueManually()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue();
        }
    }
    
    /// <summary>
    /// 会話履歴をリセット（デバッグ用）
    /// </summary>
    [ContextMenu("会話履歴をリセット")]
    public void ResetDialogueHistory()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.ResetDialogueHistory();
        }
    }
    
    /// <summary>
    /// プレイヤーとの距離をチェック
    /// </summary>
    private void Update()
    {
        if (player != null && dialogueSystem != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            
            // デバッグ用：距離を表示
            if (distance < 3f)
            {
                Debug.Log($"SampleNPCDialogue: プレイヤーとの距離 - {distance:F2}");
            }
        }
    }
    
    /// <summary>
    /// ギズモでNPCの範囲を表示
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 会話範囲を表示
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(2f, 2f, 0.1f));
        
        // NPC名を表示
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, npcName);
        #endif
    }
} 