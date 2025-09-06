using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class AreaManager : MonoBehaviour
{
    [System.Serializable]
    public class GameArea
    {
        public string areaName;
        public Transform areaRoot; // エリアのルートオブジェクト
        public Vector3 playerSpawnPoint; // プレイヤーのスポーン位置
        public Vector3 cameraPosition; // カメラの位置
        public bool isActive = true;
        public GameObject[] areaObjects; // エリア固有のオブジェクト
    }

    [Header("エリア設定")]
    [SerializeField] private List<GameArea> gameAreas = new List<GameArea>();
    [SerializeField] private string startingAreaName = "FirstGame";
    
    [Header("カメラ設定")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraTransitionSpeed = 2f;
    
    [Header("プレイヤー設定")]
    [SerializeField] private Transform playerTransform;
    
    private GameArea currentArea;
    private bool isTransitioning = false;
    
    public static AreaManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // エリア機能を無効化（フリーズ対策）
        Debug.Log("AreaManager: エリア機能は無効化されています");
        return;
        
        System.IO.File.AppendAllText(GetLogPath(), $"[AreaManager] Start {Time.time}\n");
        // 参照の自動取得
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (playerTransform == null)
        {
            // フリーズ対策：Singletonインスタンスを使用
            var playerStats = PlayerStats.Instance;
            if (playerStats != null)
                playerTransform = playerStats.transform;
        }

        // エリアリストが空の場合の自動生成を一時的に無効化
        // フリーズ防止のため手動でエリアを設定してください
        if (gameAreas.Count == 0)
        {
            Debug.LogWarning("AreaManager: エリアリストが空です。Inspectorで手動設定するか、右クリックメニューから「エリアリストを自動生成」を実行してください。");
        }

        // 開始エリアに移動（エリアが存在する場合のみ）
        if (gameAreas.Count > 0)
        {
            SwitchToArea(startingAreaName, false);
        }
        else
        {
            Debug.LogWarning("AreaManager: 利用可能なエリアがありません。");
        }
        System.IO.File.AppendAllText(GetLogPath(), $"[AreaManager] Start 終了 {Time.time}\n");
    }

    public void SwitchToArea(string areaName, bool useTransition = true)
    {
        // エリア機能を無効化（フリーズ対策）
        Debug.Log($"AreaManager: エリア機能は無効化されています。SwitchToArea({areaName})をスキップします");
        return;
        
        System.IO.File.AppendAllText(GetLogPath(), $"[AreaManager] SwitchToArea({areaName}, useTransition={useTransition}) {Time.time}\n");
        if (isTransitioning) return;

        GameArea targetArea = gameAreas.Find(area => area.areaName == areaName);
        if (targetArea == null)
        {
            Debug.LogError($"エリア '{areaName}' が見つかりません。登録済みエリア: {string.Join(", ", gameAreas.Select(a => a.areaName))}");
            return;
        }

        if (useTransition)
        {
            StartCoroutine(TransitionToArea(targetArea));
        }
        else
        {
            SetActiveArea(targetArea);
        }
        System.IO.File.AppendAllText(GetLogPath(), $"[AreaManager] SwitchToArea({areaName}) 終了 {Time.time}\n");
    }

    private System.Collections.IEnumerator TransitionToArea(GameArea targetArea)
    {
        System.IO.File.AppendAllText(GetLogPath(), $"[AreaManager] TransitionToArea({targetArea?.areaName}) 開始 {Time.time}\n");
        isTransitioning = true;
        Debug.Log($"エリア遷移開始: {targetArea.areaName}");

        // フェードアウト効果があればここに追加
        yield return new WaitForSeconds(0.1f);

        // エリア切り替え
        SetActiveArea(targetArea);

        // カメラ移動（フリーズ対策：安全な実装）
        if (mainCamera != null)
        {
            Vector3 startPos = mainCamera.transform.position;
            Vector3 targetPos = targetArea.cameraPosition;
            
            // 無限ループ対策：最大時間制限と安全チェック
            float elapsedTime = 0f;
            float maxTransitionTime = 2f; // 最大2秒でタイムアウト
            float safeSpeed = Mathf.Max(cameraTransitionSpeed, 0.1f); // 最低速度保証
            
            Debug.Log($"カメラ遷移開始: {startPos} → {targetPos}, 速度: {safeSpeed}");
            
            while (elapsedTime < 1f && (elapsedTime * safeSpeed) < maxTransitionTime)
            {
                float deltaTime = Time.unscaledDeltaTime; // unscaledDeltaTimeで時間停止対策
                elapsedTime += deltaTime * safeSpeed;
                
                // 無限ループ防止：elapsedTimeが異常値の場合は強制終了
                if (elapsedTime > 10f || float.IsNaN(elapsedTime) || float.IsInfinity(elapsedTime))
                {
                    Debug.LogWarning($"カメラ遷移で異常値検出: elapsedTime={elapsedTime}, 強制終了");
                    elapsedTime = 1f;
                    break;
                }
                
                float t = Mathf.Clamp01(elapsedTime); // 0-1に制限
                mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                
                yield return null;
                
                // 緊急脱出：フレーム数制限
                if (Time.frameCount % 300 == 0) // 約5秒で警告
                {
                    Debug.LogWarning("カメラ遷移が長時間実行中、強制終了します");
                    break;
                }
            }
            
            // 最終位置を確実に設定
            mainCamera.transform.position = targetPos;
            Debug.Log($"カメラ遷移完了: {targetPos}");
        }

        isTransitioning = false;
        Debug.Log($"エリア遷移完了: {targetArea.areaName}");
        System.IO.File.AppendAllText(GetLogPath(), $"[AreaManager] TransitionToArea({targetArea?.areaName}) 終了 {Time.time}\n");
    }

    private void SetActiveArea(GameArea area)
    {
        System.IO.File.AppendAllText(GetLogPath(), $"[AreaManager] SetActiveArea({area?.areaName}) {Time.time}\n");
        Debug.Log($"SetActiveArea開始: {area.areaName}");
        
        // フリーズ対策：非同期的な処理でオブジェクト操作
        StartCoroutine(SetActiveAreaAsync(area));
        System.IO.File.AppendAllText(GetLogPath(), $"[AreaManager] SetActiveArea({area?.areaName}) 終了 {Time.time}\n");
    }
    
    private System.Collections.IEnumerator SetActiveAreaAsync(GameArea area)
    {
        System.IO.File.AppendAllText(GetLogPath(), $"[AreaManager] SetActiveAreaAsync({area?.areaName}) 開始 {Time.time}\n");
        // 現在のエリアを非アクティブ化（分割処理）
        if (currentArea != null && currentArea.areaObjects != null)
        {
            Debug.Log($"[AreaManager] 前エリア非アクティブ化: {currentArea.areaName}, オブジェクト数: {currentArea.areaObjects.Length}");
            
            for (int i = 0; i < currentArea.areaObjects.Length; i++)
            {
                var objToDeactivate = currentArea.areaObjects[i];
                if (objToDeactivate != null)
                {
                    // 永続オブジェクトは非アクティブ化しないように保護する
                    if (objToDeactivate.CompareTag("Player") || 
                        objToDeactivate.GetComponent<PlayerStats>() != null ||
                        objToDeactivate.GetComponent<UIManager>() != null ||
                        objToDeactivate.GetComponent<AreaManager>() != null)
                    {
                        Debug.Log($"[AreaManager] スキップ: 永続オブジェクト '{objToDeactivate.name}' は非アクティブ化しません。");
                        continue;
                    }

                    Debug.Log($"[AreaManager] 非アクティブ化しようとしているオブジェクト: {objToDeactivate.name}");
                    objToDeactivate.SetActive(false);
                }
                
                // 10個ごとにフレーム待機（フリーズ防止）
                if (i % 10 == 9)
                {
                    yield return null;
                }
            }
        }
        
        // 少し待機
        yield return null;

        // 新しいエリアをアクティブ化（分割処理）
        currentArea = area;
        if (area.areaObjects != null)
        {
            Debug.Log($"[AreaManager] 新エリアアクティブ化: {area.areaName}, オブジェクト数: {area.areaObjects.Length}");
            
            for (int i = 0; i < area.areaObjects.Length; i++)
            {
                if (area.areaObjects[i] != null)
                {
                    Debug.Log($"[AreaManager] アクティブ化しようとしているオブジェクト: {area.areaObjects[i].name}");
                    area.areaObjects[i].SetActive(true);
                }
                
                // 10個ごとにフレーム待機（フリーズ防止）
                if (i % 10 == 9)
                {
                    yield return null;
                }
            }
        }

        // プレイヤーを移動
        if (playerTransform != null)
        {
            playerTransform.position = area.playerSpawnPoint;
            Debug.Log($"プレイヤー移動: {area.playerSpawnPoint}");
        }

        // カメラを即座に移動（トランジションなしの場合）
        if (mainCamera != null && !isTransitioning)
        {
            mainCamera.transform.position = area.cameraPosition;
            Debug.Log($"カメラ即座移動: {area.cameraPosition}");
        }

        Debug.Log($"エリア '{area.areaName}' への切り替え完了");
        System.IO.File.AppendAllText(GetLogPath(), $"[AreaManager] SetActiveArea({area?.areaName}) 終了 {Time.time}\n");
    }

    public string GetCurrentAreaName()
    {
        return currentArea?.areaName ?? "Unknown";
    }

    // Inspectorでエリアを簡単に設定するためのヘルパーメソッド
    [ContextMenu("エリアリストを自動生成（注意：重い処理）")]
    private void AutoGenerateAreas()
    {
        Debug.LogWarning("AutoGenerateAreas: 大量オブジェクト環境では非常に重い処理です。手動でエリアを設定することを推奨します。");
        
        gameAreas.Clear();
        
        Debug.Log("エリアオブジェクトを検索中...");
        
        // フリーズ対策：ルートオブジェクトのみ検索
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        Debug.Log($"ルートGameObjectを検索中... 総数: {rootObjects.Length}");
        
        int foundCount = 0;
        foreach (var obj in rootObjects)
        {
            if (obj.name.StartsWith("Area_"))
            {
                string areaName = obj.name.Replace("Area_", "");
                Debug.Log($"エリア検出: {obj.name} → エリア名: {areaName}");
                
                GameArea newArea = new GameArea
                {
                    areaName = areaName,
                    areaRoot = obj.transform,
                    playerSpawnPoint = obj.transform.position,
                    cameraPosition = obj.transform.position + new Vector3(0, 0, -10),
                    areaObjects = new GameObject[] { obj }
                };
                gameAreas.Add(newArea);
                foundCount++;
            }
        }
        
        Debug.Log($"{foundCount}個のエリアを検出しました: {string.Join(", ", gameAreas.Select(a => a.areaName))}");
        
        // エリアが見つからない場合の警告
        if (foundCount == 0)
        {
            Debug.LogWarning("Area_プレフィックスを持つオブジェクトが見つかりませんでした。手動でエリアを設定してください。");
        }
    }

    private string GetLogPath() {
        return System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "debug_log.txt");
    }
}