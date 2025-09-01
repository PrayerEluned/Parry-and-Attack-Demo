using UnityEngine;
using UnityEditor;

/// <summary>
/// Cainosシステムの設定を安全に追加するエディターツール
/// </summary>
public class CainosSetupTool : EditorWindow
{
    private bool showCurrentSettings = true;
    private bool showInstructions = true;

    [MenuItem("Tools/Cainos Setup Tool")]
    public static void ShowWindow()
    {
        GetWindow<CainosSetupTool>("Cainos Setup Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Cainos システム設定ツール", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 現在の設定表示
        showCurrentSettings = EditorGUILayout.Foldout(showCurrentSettings, "現在の設定");
        if (showCurrentSettings)
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("現在のレイヤー:", EditorStyles.boldLabel);
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    GUILayout.Label($"Layer {i}: {layerName}");
                }
            }
            
            GUILayout.Space(5);
            GUILayout.Label("現在のソーティングレイヤー:", EditorStyles.boldLabel);
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");
            
            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                string layerName = sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
                if (!string.IsNullOrEmpty(layerName))
                {
                    GUILayout.Label($"SortingLayer {i}: {layerName}");
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        GUILayout.Space(10);

        // 設定追加ボタン
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("安全な設定追加", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Cainosレイヤーを追加", GUILayout.Height(30)))
        {
            AddCainosLayers();
        }
        
        if (GUILayout.Button("Cainosソーティングレイヤーを追加", GUILayout.Height(30)))
        {
            AddCainosSortingLayers();
        }
        
        if (GUILayout.Button("すべて一括追加", GUILayout.Height(40)))
        {
            AddCainosLayers();
            AddCainosSortingLayers();
        }
        
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // 説明
        showInstructions = EditorGUILayout.Foldout(showInstructions, "使用方法");
        if (showInstructions)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("1. 上のボタンでレイヤーとソーティングレイヤーを追加");
            GUILayout.Label("2. プレイヤーオブジェクトにCainosPlayerAdapterコンポーネントを追加");
            GUILayout.Label("3. 階段プレファブを配置してStairsLayerTriggerの設定を確認");
            GUILayout.Label("4. レイヤー名: Layer 1, Layer 2, Layer 3");
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Cainosレイヤーを安全に追加
    /// </summary>
    private void AddCainosLayers()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        // レイヤー20-22に追加
        string[] cainosLayers = { "Layer 1", "Layer 2", "Layer 3" };
        int[] layerIndices = { 20, 21, 22 };

        for (int i = 0; i < cainosLayers.Length; i++)
        {
            int layerIndex = layerIndices[i];
            string layerName = cainosLayers[i];
            
            SerializedProperty layerProperty = layers.GetArrayElementAtIndex(layerIndex);
            string currentLayerName = layerProperty.stringValue;
            
            if (string.IsNullOrEmpty(currentLayerName))
            {
                layerProperty.stringValue = layerName;
                Debug.Log($"レイヤー {layerIndex} に '{layerName}' を追加しました");
            }
            else if (currentLayerName != layerName)
            {
                Debug.LogWarning($"レイヤー {layerIndex} は既に '{currentLayerName}' が設定されています");
            }
            else
            {
                Debug.Log($"レイヤー {layerIndex} には既に '{layerName}' が設定されています");
            }
        }

        tagManager.ApplyModifiedProperties();
        EditorUtility.SetDirty(tagManager.targetObject);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Cainosソーティングレイヤーを安全に追加
    /// </summary>
    private void AddCainosSortingLayers()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");

        string[] cainosLayers = { "Layer 1", "Layer 2", "Layer 3" };

        foreach (string layerName in cainosLayers)
        {
            bool layerExists = false;
            
            // 既存のレイヤーをチェック
            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                string existingName = sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
                if (existingName == layerName)
                {
                    layerExists = true;
                    Debug.Log($"ソーティングレイヤー '{layerName}' は既に存在します");
                    break;
                }
            }

            // レイヤーが存在しない場合は追加
            if (!layerExists)
            {
                sortingLayers.arraySize++;
                SerializedProperty newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
                newLayer.FindPropertyRelative("name").stringValue = layerName;
                newLayer.FindPropertyRelative("uniqueID").intValue = Random.Range(1000000, int.MaxValue);
                newLayer.FindPropertyRelative("locked").boolValue = false;
                
                Debug.Log($"ソーティングレイヤー '{layerName}' を追加しました");
            }
        }

        tagManager.ApplyModifiedProperties();
        EditorUtility.SetDirty(tagManager.targetObject);
        AssetDatabase.SaveAssets();
        
        Debug.Log("ソーティングレイヤーの設定が完了しました");
    }
} 