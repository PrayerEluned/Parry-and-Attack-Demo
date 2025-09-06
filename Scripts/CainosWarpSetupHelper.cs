using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Cainosマップ + ワープシステムの統合ヘルパー
/// マップエリアとワープポータルを簡単に設定できます
/// </summary>
[ExecuteInEditMode]
public class CainosWarpSetupHelper : MonoBehaviour
{
    [Header("エリア自動設定")]
    [SerializeField] private bool autoSetupAreas = true;
    [SerializeField] private bool createSampleWarpPortals = true;
    
    [Header("ワープポータル設定")]
    [SerializeField] private GameObject warpPortalPrefab;           // ワープポータルプレファブ
    [SerializeField] private bool enableInteractionPortals = true; // Eキー型ポータル
    [SerializeField] private bool enableAutoPortals = false;       // 自動ワープポータル
    
    [Header("レイヤー設定")]
    [SerializeField] private string groundLayer = "Layer 1";
    [SerializeField] private string upperLayer1 = "Layer 2";
    [SerializeField] private string upperLayer2 = "Layer 3";
    
    [Header("デバッグ")]
    [SerializeField] private bool enableDebugLogs = true;

    [Header("必須コンポーネント")]
    [SerializeField] private BoxCollider2D triggerCollider;

    private SafeAreaManager areaManager;

    private void Start()
    {
        if (autoSetupAreas)
        {
            SetupCainosWarpSystem();
        }
    }

    /// <summary>
    /// Cainosワープシステムの自動設定
    /// </summary>
    [ContextMenu("Cainosワープシステムを自動設定")]
    public void SetupCainosWarpSystem()
    {
        if (enableDebugLogs) Debug.Log("CainosWarpSetupHelper: システム設定開始");

        // SafeAreaManagerを作成または取得
        SetupAreaManager();
        
        // エリアの自動検出と設定
        SetupGameAreas();
        
        // サンプルワープポータルの作成
        if (createSampleWarpPortals)
        {
            CreateSampleWarpPortals();
        }
        
        // Cainosプレイヤー設定の確認
        SetupCainosPlayerCompatibility();
        
        if (enableDebugLogs) Debug.Log("CainosWarpSetupHelper: システム設定完了！");
    }
    
    /// <summary>
    /// SafeAreaManagerの設定
    /// </summary>
    private void SetupAreaManager()
    {
        areaManager = FindFirstObjectByType<SafeAreaManager>();
        
        if (areaManager == null)
        {
            // SafeAreaManagerを新規作成
            GameObject areaManagerObj = new GameObject("SafeAreaManager");
            areaManager = areaManagerObj.AddComponent<SafeAreaManager>();
            
            if (enableDebugLogs) Debug.Log("CainosWarpSetupHelper: SafeAreaManagerを作成しました");
        }
        else
        {
            if (enableDebugLogs) Debug.Log("CainosWarpSetupHelper: 既存のSafeAreaManagerを使用します");
        }
    }
    
    /// <summary>
    /// ゲームエリアの設定
    /// </summary>
    private void SetupGameAreas()
    {
        if (areaManager == null) return;
        
        // エリアの自動検出を実行
        areaManager.GetComponent<SafeAreaManager>().SendMessage("RegenerateAreaList", SendMessageOptions.DontRequireReceiver);
        
        if (enableDebugLogs) Debug.Log("CainosWarpSetupHelper: ゲームエリア設定完了");
    }
    
    /// <summary>
    /// サンプルワープポータルの作成
    /// </summary>
    private void CreateSampleWarpPortals()
    {
        // FirstGameエリアからNewTownエリアへのワープポータル
        CreateWarpPortal("FirstGame_to_NewTown", "NewTown", new Vector3(10, 0, 0), enableInteractionPortals);
        
        // NewTownエリアからFirstGameエリアへの戻りポータル
        CreateWarpPortal("NewTown_to_FirstGame", "FirstGame", new Vector3(-10, 0, 0), enableInteractionPortals);
        
        if (enableDebugLogs) Debug.Log("CainosWarpSetupHelper: サンプルワープポータル作成完了");
    }
    
    /// <summary>
    /// ワープポータルの作成
    /// </summary>
    public GameObject CreateWarpPortal(string portalName, string targetArea, Vector3 position, bool requireInteraction = true)
    {
        GameObject portalObj;
        
        if (warpPortalPrefab != null)
        {
            // プレファブを使用
            portalObj = Instantiate(warpPortalPrefab, position, Quaternion.identity);
            portalObj.name = portalName;
        }
        else
        {
            // 基本的なポータルを作成
            portalObj = new GameObject(portalName);
            portalObj.transform.position = position;
            
            // Collider2Dを追加
            var collider = portalObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(2f, 2f);
        }
        
        // SafeAreaPortalコンポーネントを追加/設定
        var portal = portalObj.GetComponent<SafeAreaPortal>();
        if (portal == null)
        {
            portal = portalObj.AddComponent<SafeAreaPortal>();
        }
        
        // ポータルの設定
        portal.SetTargetArea(targetArea);
        portal.SetRequireInteraction(requireInteraction);
        
        // インタラクションプロンプトの作成
        if (requireInteraction)
        {
            CreateInteractionPrompt(portalObj, $"Eキーで{targetArea}に移動");
        }
        
        if (enableDebugLogs) Debug.Log($"CainosWarpSetupHelper: ワープポータル作成 - {portalName} → {targetArea}");
        
        return portalObj;
    }
    
    /// <summary>
    /// インタラクションプロンプトUIの作成
    /// </summary>
    private void CreateInteractionPrompt(GameObject parent, string message)
    {
        // Canvas作成
        GameObject promptObj = new GameObject("InteractionPrompt");
        promptObj.transform.SetParent(parent.transform);
        promptObj.transform.localPosition = Vector3.up * 1.5f;
        
        // Canvas設定
        var canvas = promptObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
        // CanvasScaler設定
        var scaler = promptObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;
        
        // テキスト作成
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(promptObj.transform);
        
        var text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = message;
        text.fontSize = 12;
        text.color = Color.yellow;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        
        // RectTransform設定
        var rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 50);
        rectTransform.localPosition = Vector3.zero;
        
        // 初期状態は非表示
        promptObj.SetActive(false);
    }
    
    /// <summary>
    /// Cainosプレイヤーの互換性設定
    /// </summary>
    private void SetupCainosPlayerCompatibility()
    {
        var playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogWarning("CainosWarpSetupHelper: PlayerStatsが見つかりません");
            return;
        }
        
        // CainosPlayerAdapterが未設定の場合は追加
        var adapter = playerStats.GetComponent<CainosPlayerAdapter>();
        if (adapter == null)
        {
            adapter = playerStats.gameObject.AddComponent<CainosPlayerAdapter>();
            if (enableDebugLogs) Debug.Log("CainosWarpSetupHelper: CainosPlayerAdapterを追加しました");
        }
        
        // プレイヤーのレイヤー設定
        if (playerStats.gameObject.layer != LayerMask.NameToLayer(groundLayer))
        {
            if (LayerMask.NameToLayer(groundLayer) != -1)
            {
                playerStats.gameObject.layer = LayerMask.NameToLayer(groundLayer);
                if (enableDebugLogs) Debug.Log($"CainosWarpSetupHelper: プレイヤーレイヤーを{groundLayer}に設定");
            }
        }
    }
    
    /// <summary>
    /// 既存のワープポータルをすべて削除
    /// </summary>
    [ContextMenu("既存のワープポータルを削除")]
    public void ClearAllWarpPortals()
    {
        var portals = FindObjectsByType<SafeAreaPortal>(FindObjectsSortMode.None);
        foreach (var portal in portals)
        {
            if (Application.isPlaying)
            {
                Destroy(portal.gameObject);
            }
            else
            {
                DestroyImmediate(portal.gameObject);
            }
        }
        
        if (enableDebugLogs) Debug.Log($"CainosWarpSetupHelper: {portals.Length}個のワープポータルを削除しました");
    }
    
    /// <summary>
    /// ワープシステムのテスト
    /// </summary>
    [ContextMenu("ワープシステムをテスト")]
    public void TestWarpSystem()
    {
        if (SafeAreaManager.Instance == null)
        {
            Debug.LogError("CainosWarpSetupHelper: SafeAreaManagerが見つかりません");
            return;
        }
        
        var areaNames = SafeAreaManager.Instance.GetAreaNames();
        Debug.Log($"CainosWarpSetupHelper: 利用可能エリア数 - {areaNames.Count}");
        foreach (var area in areaNames)
        {
            Debug.Log($"  - {area}");
        }
        
        var portals = FindObjectsByType<SafeAreaPortal>(FindObjectsSortMode.None);
        Debug.Log($"CainosWarpSetupHelper: ワープポータル数 - {portals.Length}");
        
        Debug.Log("CainosWarpSetupHelper: ワープシステムテスト完了");
    }
    
    private void OnDrawGizmosSelected()
    {
        // ヘルパーの可視化
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 3f);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 4f, "Cainos Warp Setup Helper");
        #endif
    }
} 