using UnityEngine;
using Game.Items;

/// <summary>
/// 簡単に使用できるNPC会話システム
/// SpriteRendererのあるオブジェクトに付けるだけで動作
/// </summary>
public class SimpleNPCDialogue : MonoBehaviour
{
    [Header("NPC設定")]
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private string npcID = "npc_001";
    
    [Header("会話内容")]
    [SerializeField] private string[] firstDialogueLines = { "こんにちは！", "何かお手伝いできることはありますか？" };
    [SerializeField] private string[] repeatDialogueLines = { "また来てくれてありがとう！", "何か新しいことがあれば教えてください。" };
    [SerializeField] private string[] hasPatchDialogueLines = { "あ、あなたは既にそのpatchを持っていますね！", "他に何かお手伝いできることはありますか？" };
    
    [Header("Patch設定")]
    [SerializeField] private EnhancePatch patchToGive;
    [SerializeField] private int patchAmount = 1;
    [SerializeField] private string patchDialogueLine = "このpatchをあなたにあげます！";
    [SerializeField] private string patchReceivedDialogueLine = "ありがとうございます！";
    
    [Header("UI設定")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private UnityEngine.UI.Text dialogueText;
    [SerializeField] private UnityEngine.UI.Button nextButton;
    [SerializeField] private UnityEngine.UI.Button closeButton;
    
    [Header("デバッグ")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // 会話状態管理
    private bool isInDialogue = false;
    private bool hasTalkedBefore = false;
    private bool hasGivenPatch = false;
    
    // プレイヤーのインベントリ
    private ArtifactInventory playerInventory;
    
    // 会話キュー
    private System.Collections.Generic.Queue<string> dialogueQueue = new System.Collections.Generic.Queue<string>();
    
    private void Start()
    {
        InitializeDialogueSystem();
    }
    
    /// <summary>
    /// 会話システムの初期化
    /// </summary>
    private void InitializeDialogueSystem()
    {
        // プレイヤーのインベントリを取得
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInventory = player.GetComponent<ArtifactInventory>();
            if (playerInventory == null)
            {
                Debug.LogWarning($"SimpleNPCDialogue: プレイヤーにArtifactInventoryが見つかりません: {player.name}");
            }
        }
        
        // UIボタンの設定
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ShowNextDialogue);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseDialogue);
        }
        
        // 会話パネルを非表示
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        // 保存された会話履歴を読み込み
        LoadDialogueHistory();
        
        if (enableDebugLogs)
        {
            Debug.Log($"SimpleNPCDialogue: 初期化完了 - NPC: {npcName}");
        }
    }
    
    /// <summary>
    /// 会話を開始
    /// </summary>
    public void StartDialogue()
    {
        if (isInDialogue) return;
        
        isInDialogue = true;
        
        // プレイヤーの移動を停止
        StopPlayerMovement();
        
        // 会話内容を決定
        DetermineDialogueContent();
        
        // 会話パネルを表示
        ShowDialoguePanel();
        
        // 最初の会話を表示
        ShowNextDialogue();
        
        if (enableDebugLogs)
        {
            Debug.Log($"SimpleNPCDialogue: 会話開始 - NPC: {npcName}");
        }
    }
    
    /// <summary>
    /// 会話内容を決定
    /// </summary>
    private void DetermineDialogueContent()
    {
        dialogueQueue.Clear();
        
        // patchを持っているかチェック
        bool hasPatch = CheckIfPlayerHasPatch();
        
        if (!hasTalkedBefore)
        {
            // 初回会話
            if (patchToGive != null && !hasPatch)
            {
                // patchを渡す会話
                foreach (string line in firstDialogueLines)
                {
                    dialogueQueue.Enqueue(line);
                }
                dialogueQueue.Enqueue(patchDialogueLine);
                dialogueQueue.Enqueue(patchReceivedDialogueLine);
            }
            else
            {
                // 通常の初回会話
                foreach (string line in firstDialogueLines)
                {
                    dialogueQueue.Enqueue(line);
                }
            }
        }
        else
        {
            // 二回目以降の会話
            if (patchToGive != null && !hasPatch && !hasGivenPatch)
            {
                // patchを渡す会話
                foreach (string line in repeatDialogueLines)
                {
                    dialogueQueue.Enqueue(line);
                }
                dialogueQueue.Enqueue(patchDialogueLine);
                dialogueQueue.Enqueue(patchReceivedDialogueLine);
            }
            else if (hasPatch)
            {
                // patchを持っている場合の会話
                foreach (string line in hasPatchDialogueLines)
                {
                    dialogueQueue.Enqueue(line);
                }
            }
            else
            {
                // 通常の繰り返し会話
                foreach (string line in repeatDialogueLines)
                {
                    dialogueQueue.Enqueue(line);
                }
            }
        }
    }
    
    /// <summary>
    /// プレイヤーがpatchを持っているかチェック
    /// </summary>
    private bool CheckIfPlayerHasPatch()
    {
        if (playerInventory == null || patchToGive == null) return false;
        
        var ownedPatches = playerInventory.GetAllOwnedPatches();
        return ownedPatches.Contains(patchToGive);
    }
    
    /// <summary>
    /// 次の会話を表示
    /// </summary>
    public void ShowNextDialogue()
    {
        if (dialogueQueue.Count > 0)
        {
            string nextLine = dialogueQueue.Dequeue();
            DisplayDialogueText(nextLine);
            
            // 最後の会話かチェック
            if (dialogueQueue.Count == 0)
            {
                // patchを渡すタイミングかチェック
                if (ShouldGivePatch())
                {
                    GivePatchToPlayer();
                }
                
                // 会話終了処理
                OnDialogueComplete();
            }
        }
        else
        {
            CloseDialogue();
        }
    }
    
    /// <summary>
    /// 会話テキストを表示
    /// </summary>
    private void DisplayDialogueText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = $"{npcName}: {text}";
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"SimpleNPCDialogue: 会話表示 - {npcName}: {text}");
        }
    }
    
    /// <summary>
    /// patchを渡すタイミングかチェック
    /// </summary>
    private bool ShouldGivePatch()
    {
        if (patchToGive == null) return false;
        
        // パッチは常に渡せるようにする（hasGivenPatchチェックを無効化）
        return !CheckIfPlayerHasPatch();
    }
    
    /// <summary>
    /// プレイヤーにpatchを渡す
    /// </summary>
    private void GivePatchToPlayer()
    {
        if (playerInventory == null || patchToGive == null) return;
        
        playerInventory.AddPatch(patchToGive, patchAmount);
        // hasGivenPatch = true; // パッチは常に渡せるようにするため無効化
        
        // 会話履歴を保存（パッチ関連は除外）
        SaveDialogueHistory();
        
        if (enableDebugLogs)
        {
            Debug.Log($"SimpleNPCDialogue: patchを渡しました - {patchToGive.patchName} x{patchAmount}");
        }
    }
    
    /// <summary>
    /// 会話パネルを表示
    /// </summary>
    private void ShowDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// 会話を閉じる
    /// </summary>
    public void CloseDialogue()
    {
        isInDialogue = false;
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        // プレイヤーの移動を再開
        ResumePlayerMovement();
        
        if (enableDebugLogs)
        {
            Debug.Log($"SimpleNPCDialogue: 会話終了 - NPC: {npcName}");
        }
    }
    
    /// <summary>
    /// 会話完了時の処理
    /// </summary>
    private void OnDialogueComplete()
    {
        hasTalkedBefore = true;
        SaveDialogueHistory();
        
        if (enableDebugLogs)
        {
            Debug.Log($"SimpleNPCDialogue: 会話完了 - NPC: {npcName}");
        }
    }
    
    /// <summary>
    /// プレイヤーの移動を停止
    /// </summary>
    private void StopPlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var moveScript = player.GetComponent<CharacterMovement>();
            if (moveScript != null)
            {
                moveScript.EnableMovement(false);
            }
        }
    }
    
    /// <summary>
    /// プレイヤーの移動を再開
    /// </summary>
    private void ResumePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var moveScript = player.GetComponent<CharacterMovement>();
            if (moveScript != null)
            {
                moveScript.EnableMovement(true);
            }
        }
    }
    
    /// <summary>
    /// 会話履歴を保存
    /// </summary>
    private void SaveDialogueHistory()
    {
        string key = $"NPC_Dialogue_{npcID}";
        PlayerPrefs.SetInt(key, hasTalkedBefore ? 1 : 0);
        
        // パッチ関連のセーブは無効化（常に渡せるようにするため）
        // string patchKey = $"NPC_Patch_{npcID}";
        // PlayerPrefs.SetInt(patchKey, hasGivenPatch ? 1 : 0);
        
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 会話履歴を読み込み
    /// </summary>
    private void LoadDialogueHistory()
    {
        string key = $"NPC_Dialogue_{npcID}";
        hasTalkedBefore = PlayerPrefs.GetInt(key, 0) == 1;
        
        // パッチ関連のロードは無効化（常に渡せるようにするため）
        // string patchKey = $"NPC_Patch_{npcID}";
        // hasGivenPatch = PlayerPrefs.GetInt(patchKey, 0) == 1;
        hasGivenPatch = false; // 常にfalseに設定
    }
    
    /// <summary>
    /// 会話履歴をリセット（デバッグ用）
    /// </summary>
    [ContextMenu("会話履歴をリセット")]
    public void ResetDialogueHistory()
    {
        hasTalkedBefore = false;
        hasGivenPatch = false;
        SaveDialogueHistory();
        
        if (enableDebugLogs)
        {
            Debug.Log($"SimpleNPCDialogue: 会話履歴をリセットしました - NPC: {npcName}");
        }
    }
    
    /// <summary>
    /// プレイヤーが範囲内に入った時の処理
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isInDialogue)
        {
            StartDialogue();
        }
    }
    
    /// <summary>
    /// プレイヤーが範囲外に出た時の処理
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isInDialogue)
        {
            CloseDialogue();
        }
    }
} 