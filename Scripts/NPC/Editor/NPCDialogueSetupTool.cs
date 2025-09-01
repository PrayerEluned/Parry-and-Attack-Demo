using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace NPCSystem.Editor
{
    /// <summary>
    /// NPC会話システムのセットアップツール
    /// </summary>
    public class NPCDialogueSetupTool : EditorWindow
    {
        [MenuItem("Tools/NPC Dialogue Setup Tool")]
        public static void ShowWindow()
        {
            GetWindow<NPCDialogueSetupTool>("NPC会話セットアップ");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("NPC会話システムセットアップツール", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("会話UIプレハブを作成"))
            {
                CreateDialogueUIPrefab();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("既存のNPCに会話システムを追加"))
            {
                AddDialogueSystemToNPC();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("NPC会話プレハブを作成"))
            {
                CreateNPCDialoguePrefab();
            }
        }
        
        /// <summary>
        /// 会話UIプレハブを作成
        /// </summary>
        private void CreateDialogueUIPrefab()
        {
            // 会話UIのGameObjectを作成
            GameObject uiObject = new GameObject("DialogueUI");
            
            // 会話UIを作成
            CreateDialogueUI(uiObject);
            
            // プレハブとして保存
            string prefabPath = "Assets/Prefabs/UI/DialogueUI.prefab";
            CreatePrefabDirectory(prefabPath);
            
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(uiObject, prefabPath);
            
            // シーンのGameObjectを削除
            DestroyImmediate(uiObject);
            
            Debug.Log($"会話UIプレハブを作成しました: {prefabPath}");
            
            // プレハブを選択
            Selection.activeObject = prefab;
        }
        
        /// <summary>
        /// 会話UIを作成
        /// </summary>
        private void CreateDialogueUI(GameObject parent)
        {
            // Canvasを作成
            GameObject canvasObject = new GameObject("DialogueCanvas");
            canvasObject.transform.SetParent(parent.transform);
            
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // 最前面に表示
            
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            
            // 会話パネルを作成
            GameObject panelObject = new GameObject("DialoguePanel");
            panelObject.transform.SetParent(canvasObject.transform);
            
            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.3f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // 会話テキストを作成
            GameObject textObject = new GameObject("DialogueText");
            textObject.transform.SetParent(panelObject.transform);
            
            TextMeshProUGUI dialogueText = textObject.AddComponent<TextMeshProUGUI>();
            dialogueText.text = "NPC: こんにちは！";
            dialogueText.fontSize = 16;
            dialogueText.color = Color.white;
            dialogueText.alignment = TextAlignmentOptions.Left;
            
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.1f);
            textRect.anchorMax = new Vector2(0.95f, 0.8f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // 次のボタンを作成
            GameObject nextButtonObject = new GameObject("NextButton");
            nextButtonObject.transform.SetParent(panelObject.transform);
            
            Button nextButton = nextButtonObject.AddComponent<Button>();
            Image nextButtonImage = nextButtonObject.AddComponent<Image>();
            nextButtonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            TextMeshProUGUI nextButtonText = nextButtonObject.AddComponent<TextMeshProUGUI>();
            nextButtonText.text = "次へ";
            nextButtonText.fontSize = 14;
            nextButtonText.color = Color.white;
            nextButtonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform nextButtonRect = nextButtonObject.GetComponent<RectTransform>();
            nextButtonRect.anchorMin = new Vector2(0.7f, 0.05f);
            nextButtonRect.anchorMax = new Vector2(0.95f, 0.25f);
            nextButtonRect.offsetMin = Vector2.zero;
            nextButtonRect.offsetMax = Vector2.zero;
            
            // 閉じるボタンを作成
            GameObject closeButtonObject = new GameObject("CloseButton");
            closeButtonObject.transform.SetParent(panelObject.transform);
            
            Button closeButton = closeButtonObject.AddComponent<Button>();
            Image closeButtonImage = closeButtonObject.AddComponent<Image>();
            closeButtonImage.color = new Color(0.5f, 0.2f, 0.2f, 0.8f);
            
            TextMeshProUGUI closeButtonText = closeButtonObject.AddComponent<TextMeshProUGUI>();
            closeButtonText.text = "閉じる";
            closeButtonText.fontSize = 14;
            closeButtonText.color = Color.white;
            closeButtonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform closeButtonRect = closeButtonObject.GetComponent<RectTransform>();
            closeButtonRect.anchorMin = new Vector2(0.05f, 0.05f);
            closeButtonRect.anchorMax = new Vector2(0.3f, 0.25f);
            closeButtonRect.offsetMin = Vector2.zero;
            closeButtonRect.offsetMax = Vector2.zero;
            
            // NPCDialogueUIコンポーネントを追加
            NPCDialogueUI dialogueUI = parent.AddComponent<NPCDialogueUI>();
            
            // 参照を設定
            SerializedObject serializedObject = new SerializedObject(dialogueUI);
            serializedObject.FindProperty("dialoguePanel").objectReferenceValue = panelObject;
            serializedObject.FindProperty("dialogueText").objectReferenceValue = dialogueText;
            serializedObject.FindProperty("nextButton").objectReferenceValue = nextButton;
            serializedObject.FindProperty("closeButton").objectReferenceValue = closeButton;
            serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// 既存のNPCに会話システムを追加
        /// </summary>
        private void AddDialogueSystemToNPC()
        {
            GameObject selectedObject = Selection.activeGameObject;
            
            if (selectedObject == null)
            {
                EditorUtility.DisplayDialog("エラー", "NPCのGameObjectを選択してください。", "OK");
                return;
            }
            
            // NPCDialogueSystemコンポーネントを追加
            NPCDialogueSystem dialogueSystem = selectedObject.GetComponent<NPCDialogueSystem>();
            if (dialogueSystem == null)
            {
                dialogueSystem = selectedObject.AddComponent<NPCDialogueSystem>();
            }
            
            // Collider2Dを追加（なければ）
            if (selectedObject.GetComponent<Collider2D>() == null)
            {
                BoxCollider2D collider = selectedObject.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = new Vector2(2f, 2f);
            }
            
            Debug.Log($"NPCに会話システムを追加しました: {selectedObject.name}");
            Debug.Log("Dialogue UI PrefabフィールドにUIプレハブを設定してください。");
        }
        
        /// <summary>
        /// NPC会話プレハブを作成
        /// </summary>
        private void CreateNPCDialoguePrefab()
        {
            // NPCのGameObjectを作成
            GameObject npcObject = new GameObject("NPCDialogue");
            
            // 必要なコンポーネントを追加
            npcObject.AddComponent<NPCDialogueSystem>();
            
            // Collider2Dを追加（トリガー用）
            BoxCollider2D collider = npcObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(2f, 2f);
            
            // プレハブとして保存
            string prefabPath = "Assets/Prefabs/NPC/NPCDialogue.prefab";
            CreatePrefabDirectory(prefabPath);
            
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(npcObject, prefabPath);
            
            // シーンのGameObjectを削除
            DestroyImmediate(npcObject);
            
            Debug.Log($"NPC会話プレハブを作成しました: {prefabPath}");
            Debug.Log("Dialogue UI PrefabフィールドにUIプレハブを設定してください。");
            
            // プレハブを選択
            Selection.activeObject = prefab;
        }
        
        /// <summary>
        /// プレハブディレクトリを作成
        /// </summary>
        private void CreatePrefabDirectory(string prefabPath)
        {
            string directory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
        }
    }
} 