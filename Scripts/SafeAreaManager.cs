using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

/// <summary>
/// フリーズ対策済みエリア管理システム
/// 既存のAreaManagerの安全版
/// </summary>
public class SafeAreaManager : MonoBehaviour
{
    [System.Serializable]
    public class GameArea
    {
        public string areaName;
        public Transform areaRoot;
        public Vector3 playerSpawnPoint;
        public Vector3 cameraPosition;
        public bool isActive = true;
        public GameObject[] areaObjects;
    }

    [Header("エリア設定")]
    [SerializeField] private List<GameArea> gameAreas = new List<GameArea>();
    [SerializeField] private string startingAreaName = "FirstGame";
    
    [Header("カメラ設定")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraTransitionSpeed = 2f;
    [SerializeField] private bool enableCameraTransition = true;
    
    [Header("プレイヤー設定")]
    [SerializeField] private Transform playerTransform;
    
    [Header("最適化設定")]
    [SerializeField] private float maxObjectsPerFrame = 5f;     // フレームあたりの最大処理オブジェクト数
    [SerializeField] private bool enableDebugLogs = true;       // デバッグログの有効/無効
    [SerializeField] private bool enableFadeEffect = false;     // フェード効果（重いので無効）
    
    private GameArea currentArea;
    private bool isTransitioning = false;
    
    public static SafeAreaManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeAreaSystem();
    }
    
    /// <summary>
    /// エリアシステムの安全な初期化
    /// </summary>
    private void InitializeAreaSystem()
    {
        if (enableDebugLogs) Debug.Log("SafeAreaManager: エリアシステム初期化開始");
        
        // 自動参照取得
        if (mainCamera == null) mainCamera = Camera.main;
        if (playerTransform == null)
        {
            var playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats != null) playerTransform = playerStats.transform;
        }
        
        // エリア自動検出
        if (gameAreas.Count == 0)
        {
            AutoDetectAreas();
        }
        
        // 開始エリアの設定
        if (gameAreas.Count > 0)
        {
            var startArea = gameAreas.Find(area => area.areaName == startingAreaName);
            if (startArea != null)
            {
                SetActiveAreaImmediate(startArea);
            }
            else
            {
                SetActiveAreaImmediate(gameAreas[0]);
                if (enableDebugLogs) Debug.LogWarning($"SafeAreaManager: 開始エリア '{startingAreaName}' が見つかりません。最初のエリアを使用します");
            }
        }
        else
        {
            Debug.LogWarning("SafeAreaManager: エリアが見つかりません。Inspectorで手動設定してください");
        }
        
        if (enableDebugLogs) Debug.Log("SafeAreaManager: エリアシステム初期化完了");
    }
    
    /// <summary>
    /// エリアの自動検出
    /// </summary>
    private void AutoDetectAreas()
    {
        if (enableDebugLogs) Debug.Log("SafeAreaManager: エリア自動検出開始");
        
        var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        
        foreach (var obj in rootObjects)
        {
            if (obj.name.StartsWith("Area_"))
            {
                string areaName = obj.name.Replace("Area_", "");
                
                GameArea newArea = new GameArea
                {
                    areaName = areaName,
                    areaRoot = obj.transform,
                    playerSpawnPoint = obj.transform.position,
                    cameraPosition = obj.transform.position + new Vector3(0, 0, -10),
                    areaObjects = new GameObject[] { obj }
                };
                
                gameAreas.Add(newArea);
                if (enableDebugLogs) Debug.Log($"SafeAreaManager: エリア検出 - {areaName}");
            }
        }
        
        if (enableDebugLogs) Debug.Log($"SafeAreaManager: {gameAreas.Count}個のエリアを検出しました");
    }
    
    /// <summary>
    /// 指定エリアへのワープ（公開API）
    /// </summary>
    public void WarpToArea(string areaName, bool useTransition = true)
    {
        if (isTransitioning)
        {
            if (enableDebugLogs) Debug.LogWarning("SafeAreaManager: エリア遷移中のため処理をスキップします");
            return;
        }
        
        var targetArea = gameAreas.Find(area => area.areaName == areaName);
        if (targetArea == null)
        {
            Debug.LogError($"SafeAreaManager: エリア '{areaName}' が見つかりません。利用可能エリア: {string.Join(", ", gameAreas.Select(a => a.areaName))}");
            return;
        }
        
        if (enableDebugLogs) Debug.Log($"SafeAreaManager: ワープ開始 - {areaName}");
        
        if (useTransition)
        {
            StartCoroutine(WarpToAreaCoroutine(targetArea));
        }
        else
        {
            SetActiveAreaImmediate(targetArea);
        }
    }
    
    /// <summary>
    /// ワープのコルーチン（安全な実装）
    /// </summary>
    private IEnumerator WarpToAreaCoroutine(GameArea targetArea)
    {
        isTransitioning = true;
        
        if (enableDebugLogs) Debug.Log($"SafeAreaManager: エリア遷移開始 - {targetArea.areaName}");
        
        // フェード効果（軽量版）
        if (enableFadeEffect)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // エリア切り替え（非同期版）
        yield return StartCoroutine(SetActiveAreaAsync(targetArea));
        
        // カメラ移動（安全版）
        if (enableCameraTransition && mainCamera != null)
        {
            yield return StartCoroutine(MoveCameraSafely(targetArea.cameraPosition));
        }
        else if (mainCamera != null)
        {
            mainCamera.transform.position = targetArea.cameraPosition;
        }
        
        isTransitioning = false;
        
        if (enableDebugLogs) Debug.Log($"SafeAreaManager: エリア遷移完了 - {targetArea.areaName}");
    }
    
    /// <summary>
    /// エリアの即座切り替え
    /// </summary>
    private void SetActiveAreaImmediate(GameArea area)
    {
        currentArea = area;
        
        // プレイヤー位置設定
        if (playerTransform != null)
        {
            playerTransform.position = area.playerSpawnPoint;
        }
        
        // カメラ位置設定
        if (mainCamera != null)
        {
            mainCamera.transform.position = area.cameraPosition;
        }
        
        if (enableDebugLogs) Debug.Log($"SafeAreaManager: エリア即座切り替え完了 - {area.areaName}");
    }
    
    /// <summary>
    /// エリアの非同期切り替え
    /// </summary>
    private IEnumerator SetActiveAreaAsync(GameArea area)
    {
        // 現在のエリアを非アクティブ化
        if (currentArea != null)
        {
            yield return StartCoroutine(DeactivateAreaObjects(currentArea));
        }
        
        // 新しいエリアをアクティブ化
        currentArea = area;
        yield return StartCoroutine(ActivateAreaObjects(area));
        
        // プレイヤー移動
        if (playerTransform != null)
        {
            playerTransform.position = area.playerSpawnPoint;
        }
    }
    
    /// <summary>
    /// エリアオブジェクトの非アクティブ化（フリーズ対策版）
    /// </summary>
    private IEnumerator DeactivateAreaObjects(GameArea area)
    {
        if (area.areaObjects == null) yield break;
        
        int processedCount = 0;
        foreach (var obj in area.areaObjects)
        {
            if (obj != null)
            {
                // 永続オブジェクトの保護
                if (obj.CompareTag("Player") || 
                    obj.GetComponent<PlayerStats>() != null ||
                    obj.GetComponent<UIManager>() != null ||
                    obj.GetComponent<SafeAreaManager>() != null)
                {
                    continue;
                }
                
                obj.SetActive(false);
                processedCount++;
                
                // フレーム分散処理
                if (processedCount >= maxObjectsPerFrame)
                {
                    processedCount = 0;
                    yield return null;
                }
            }
        }
    }
    
    /// <summary>
    /// エリアオブジェクトのアクティブ化（フリーズ対策版）
    /// </summary>
    private IEnumerator ActivateAreaObjects(GameArea area)
    {
        if (area.areaObjects == null) yield break;
        
        int processedCount = 0;
        foreach (var obj in area.areaObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                processedCount++;
                
                // フレーム分散処理
                if (processedCount >= maxObjectsPerFrame)
                {
                    processedCount = 0;
                    yield return null;
                }
            }
        }
    }
    
    /// <summary>
    /// カメラの安全な移動
    /// </summary>
    private IEnumerator MoveCameraSafely(Vector3 targetPosition)
    {
        if (mainCamera == null) yield break;
        
        Vector3 startPosition = mainCamera.transform.position;
        float elapsedTime = 0f;
        float duration = Vector3.Distance(startPosition, targetPosition) / cameraTransitionSpeed;
        duration = Mathf.Clamp(duration, 0.1f, 2f); // 最小0.1秒、最大2秒
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            
            yield return null;
            
            // 安全装置：無限ループ防止
            if (elapsedTime > 5f)
            {
                Debug.LogWarning("SafeAreaManager: カメラ移動がタイムアウトしました");
                break;
            }
        }
        
        mainCamera.transform.position = targetPosition;
    }
    
    /// <summary>
    /// 現在のエリア名を取得
    /// </summary>
    public string GetCurrentAreaName()
    {
        return currentArea?.areaName ?? "Unknown";
    }
    
    /// <summary>
    /// エリアリストを取得
    /// </summary>
    public List<string> GetAreaNames()
    {
        return gameAreas.Select(area => area.areaName).ToList();
    }
    
    /// <summary>
    /// エリアが存在するかチェック
    /// </summary>
    public bool HasArea(string areaName)
    {
        return gameAreas.Any(area => area.areaName == areaName);
    }
    
    /// <summary>
    /// Inspectorでエリアリストを自動生成
    /// </summary>
    [ContextMenu("エリアリストを自動生成")]
    private void RegenerateAreaList()
    {
        gameAreas.Clear();
        AutoDetectAreas();
        Debug.Log($"SafeAreaManager: エリアリストを再生成しました（{gameAreas.Count}個）");
    }
    
    private void OnDrawGizmosSelected()
    {
        if (gameAreas != null)
        {
            foreach (var area in gameAreas)
            {
                if (area.areaRoot != null)
                {
                    // エリアの可視化
                    Gizmos.color = area == currentArea ? Color.green : Color.yellow;
                    Gizmos.DrawWireSphere(area.areaRoot.position, 2f);
                    
                    // スポーン位置の可視化
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(area.playerSpawnPoint, Vector3.one * 0.5f);
                    
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(area.areaRoot.position + Vector3.up * 3f, area.areaName);
                    #endif
                }
            }
        }
    }
} 