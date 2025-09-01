using UnityEngine;
using System.Collections.Generic;
using Game.Items;

namespace NPCSystem
{
    /// <summary>
    /// NPCとの会話システム
    /// patchの受け渡し機能付き
    /// </summary>
    public class NPCDialogueSystem : MonoBehaviour
    {
        [Header("UI設定")]
        [SerializeField] private NPCDialogueUI dialogueUIPrefab;
        
        [Header("プレイヤー設定")]
        [SerializeField] private GameObject playerObject;
        
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
        

        
        // 会話状態管理
        private bool isInDialogue = false;
        private bool hasTalkedBefore = false;
        private bool hasGivenPatch = false;
        
        // プレイヤーのインベントリ
        private ArtifactInventory playerInventory;
        
        // 会話キュー
        private Queue<string> dialogueQueue = new Queue<string>();
        
        private void Start()
        {
            InitializeDialogueSystem();
        }
        
        /// <summary>
        /// 会話システムの初期化
        /// </summary>
        private void InitializeDialogueSystem()
        {
            // プレイヤーのインベントリを取得（シングルトンを使用）
            playerInventory = ArtifactInventory.Instance;
            if (playerInventory == null)
            {
                Debug.LogError($"PATCH: ArtifactInventory.Instance が null です");
            }
            else
            {
                Debug.Log($"PATCH: ArtifactInventory を取得しました - Instance");
            }
            
            // UIプレハブからUIを生成
            if (dialogueUIPrefab != null)
            {
                CreateDialogueUI();
            }
                
            // 保存された会話履歴を読み込み
            LoadDialogueHistory();
        }
        
        /// <summary>
        /// 会話UIを作成
        /// </summary>
        private void CreateDialogueUI()
        {
            if (dialogueUIPrefab == null) return;
            
            // UIプレハブをインスタンス化
            NPCDialogueUI dialogueUI = Instantiate(dialogueUIPrefab, transform);
            
            // UIイベントの設定
            dialogueUI.OnNextClicked += ShowNextDialogue;
            dialogueUI.OnCloseClicked += CloseDialogue;
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
            
            // 会話UIを表示
            NPCDialogueUI dialogueUI = GetComponentInChildren<NPCDialogueUI>();
            if (dialogueUI != null)
            {
                dialogueUI.ShowPanel();
            }
            
            // 最初の会話を表示
            ShowNextDialogue();
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
            if (playerInventory == null)
            {
                Debug.Log($"PATCH: playerInventory が null のためチェックできません");
                return false;
            }
            
            if (patchToGive == null)
            {
                Debug.Log($"PATCH: patchToGive が null のためチェックできません");
                return false;
            }
            
            var ownedPatches = playerInventory.GetAllOwnedPatches();
            bool hasPatch = ownedPatches.Contains(patchToGive);
            
            Debug.Log($"PATCH: CheckIfPlayerHasPatch() - ownedPatches.Count: {ownedPatches.Count}, hasPatch: {hasPatch}");
            
            return hasPatch;
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
            NPCDialogueUI dialogueUI = GetComponentInChildren<NPCDialogueUI>();
            if (dialogueUI != null)
            {
                dialogueUI.DisplayDialogueText($"{npcName}: {text}");
            }
        }
        
        /// <summary>
        /// patchを渡すタイミングかチェック
        /// </summary>
        private bool ShouldGivePatch()
        {
            if (patchToGive == null)
            {
                Debug.Log($"PATCH: patchToGive が null のため渡せません");
                return false;
            }
            
            bool hasPatch = CheckIfPlayerHasPatch();
            // パッチは常に渡せるようにする（hasGivenPatchチェックを無効化）
            bool shouldGive = !hasPatch;
            
            Debug.Log($"PATCH: ShouldGivePatch() - hasPatch: {hasPatch}, hasGivenPatch: {hasGivenPatch}, shouldGive: {shouldGive}");
            
            return shouldGive;
        }
        
        /// <summary>
        /// プレイヤーにpatchを渡す
        /// </summary>
        private void GivePatchToPlayer()
        {
            Debug.Log($"PATCH: GivePatchToPlayer() が呼ばれました");
            
            if (playerInventory == null)
            {
                Debug.LogError($"PATCH: playerInventory が null です");
                return;
            }
            
            if (patchToGive == null)
            {
                Debug.LogError($"PATCH: patchToGive が null です");
                return;
            }
            
            Debug.Log($"PATCH: AddPatch() を呼び出します - {patchToGive.patchName} x{patchAmount}");
            playerInventory.AddPatch(patchToGive, patchAmount);
            // hasGivenPatch = true; // パッチは常に渡せるようにするため無効化
            
            // 会話履歴を保存（パッチ関連は除外）
            SaveDialogueHistory();
            
            Debug.Log($"PATCH: {patchToGive.patchName} x{patchAmount} を渡しました");
        }
        
        /// <summary>
        /// 会話を閉じる
        /// </summary>
        public void CloseDialogue()
        {
            isInDialogue = false;
            
            NPCDialogueUI dialogueUI = GetComponentInChildren<NPCDialogueUI>();
            if (dialogueUI != null)
            {
                dialogueUI.HidePanel();
            }
            
            // プレイヤーの移動を再開
            ResumePlayerMovement();
        }
        
        /// <summary>
        /// 会話完了時の処理
        /// </summary>
        private void OnDialogueComplete()
        {
            hasTalkedBefore = true;
            SaveDialogueHistory();
        }
        
        /// <summary>
        /// プレイヤーの移動を停止
        /// </summary>
        private void StopPlayerMovement()
        {
            GameObject player = playerObject != null ? playerObject : GameObject.FindGameObjectWithTag("Player");
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
            GameObject player = playerObject != null ? playerObject : GameObject.FindGameObjectWithTag("Player");
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
            
            Debug.Log($"PATCH: 会話履歴をリセットしました");
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
} 