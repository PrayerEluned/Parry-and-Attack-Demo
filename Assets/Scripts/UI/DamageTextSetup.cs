using UnityEngine;
using TMPro;

public class DamageTextSetup : MonoBehaviour
{
    [Header("Setup Settings")]
    public bool createPrefabOnStart = false;
    public string prefabName = "DamageTextPrefab";
    
    private void Start()
    {
        if (createPrefabOnStart)
        {
            CreateDamageTextPrefab();
        }
    }
    
    [ContextMenu("Create Damage Text Prefab")]
    public void CreateDamageTextPrefab()
    {
        // メインのGameObject
        GameObject damageTextObj = new GameObject(prefabName);
        
        // CanvasGroupを追加
        CanvasGroup canvasGroup = damageTextObj.AddComponent<CanvasGroup>();
        
        // DamageTextスクリプトを追加
        DamageText damageText = damageTextObj.AddComponent<DamageText>();
        
        // TextMeshProUGUIを追加
        GameObject textObj = new GameObject("DamageText");
        textObj.transform.SetParent(damageTextObj.transform);
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = "0";
        textComponent.fontSize = 24;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        // RectTransformの設定
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(100, 50);
        
        // Canvas設定（ワールド座標モード）
        Canvas canvas = textComponent.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = textComponent.gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        
        // DamageTextLayerFixerを追加してSortingLayerを確実に設定
        var layerFixer = textComponent.gameObject.GetComponent<DamageTextLayerFixer>();
        if (layerFixer == null)
        {
            layerFixer = textComponent.gameObject.AddComponent<DamageTextLayerFixer>();
        }
        layerFixer.FixSortingLayer();
        
        // UIレイヤーに設定
        damageTextObj.layer = LayerMask.NameToLayer("UI");
        textObj.layer = LayerMask.NameToLayer("UI");
        
        // DamageTextスクリプトに参照を設定
        damageText.damageText = textComponent;
        damageText.canvasGroup = canvasGroup;
        
        // プレハブとして保存
        #if UNITY_EDITOR
        string prefabPath = "Assets/Prefabs/" + prefabName + ".prefab";
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(damageTextObj, prefabPath);
        Debug.Log("DamageText prefab created at: " + prefabPath);
        #endif
        
        // シーンから削除
        DestroyImmediate(damageTextObj);
    }
    
    [ContextMenu("Setup Damage Text Manager")]
    public void SetupDamageTextManager()
    {
        // DamageTextManagerを探すか作成
        DamageTextManager manager = FindObjectOfType<DamageTextManager>();
        
        if (manager == null)
        {
            GameObject managerObj = new GameObject("DamageTextManager");
            manager = managerObj.AddComponent<DamageTextManager>();
        }
        
        // プレハブを探して設定
        GameObject prefab = Resources.Load<GameObject>("Prefabs/DamageTextPrefab");
        if (prefab == null)
        {
            // Assets/Prefabsから探す
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("DamageTextPrefab");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            #endif
        }
        
        if (prefab != null)
        {
            manager.damageTextPrefab = prefab;
            Debug.Log("DamageTextManager setup completed!");
        }
        else
        {
            Debug.LogWarning("DamageTextPrefab not found! Please create it first.");
        }
    }
} 