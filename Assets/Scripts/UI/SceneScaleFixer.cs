using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// シーンとマップのサイズ感を修正するスクリプト
/// UIとゲーム世界のスケール比を適切に調整します
/// </summary>
public class SceneScaleFixer : MonoBehaviour
{
    [Header("推奨設定")]
    [SerializeField] private float recommendedCameraSize = 5f; // Cainosアセット推奨値
    [SerializeField] private float recommendedCanvasScaleFactor = 1f;
    [SerializeField] private Vector2 recommendedReferenceResolution = new Vector2(1920, 1080);
    
    [Header("自動修正設定")]
    [SerializeField] private bool fixOnStart = true;
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("検出された設定")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas[] canvases;
    [SerializeField] private CanvasScaler[] canvasScalers;
    
    private void Start()
    {
        if (fixOnStart)
        {
            AnalyzeCurrentSettings();
            FixScaleIssues();
        }
    }
    
    /// <summary>
    /// 現在の設定を分析
    /// </summary>
    [ContextMenu("現在の設定を分析")]
    public void AnalyzeCurrentSettings()
    {
        Debug.Log("=== シーン設定分析開始 ===");
        
        // メインカメラを取得
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera != null)
        {
            Debug.Log($"📷 カメラ設定:");
            Debug.Log($"  - Orthographic: {mainCamera.orthographic}");
            Debug.Log($"  - Orthographic Size: {mainCamera.orthographicSize}");
            
            if (mainCamera.orthographicSize > recommendedCameraSize + 1f)
            {
                Debug.LogWarning($"⚠️ カメラサイズが大きすぎます: {mainCamera.orthographicSize} (推奨: {recommendedCameraSize})");
            }
        }
        
        // Canvas設定を取得
        canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        canvasScalers = FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None);
        
        Debug.Log($"🖼️ Canvas数: {canvases.Length}");
        
        for (int i = 0; i < canvasScalers.Length; i++)
        {
            var scaler = canvasScalers[i];
            Debug.Log($"📊 CanvasScaler {i + 1}:");
            Debug.Log($"  - Scale Factor: {scaler.scaleFactor}");
            Debug.Log($"  - Reference Resolution: {scaler.referenceResolution}");
            Debug.Log($"  - UI Scale Mode: {scaler.uiScaleMode}");
            
            if (scaler.scaleFactor > recommendedCanvasScaleFactor + 0.1f)
            {
                Debug.LogWarning($"⚠️ Scale Factorが大きすぎます: {scaler.scaleFactor} (推奨: {recommendedCanvasScaleFactor})");
            }
        }
        
        Debug.Log("=== 分析完了 ===");
    }
    
    /// <summary>
    /// スケール問題を自動修正
    /// </summary>
    [ContextMenu("スケール問題を修正")]
    public void FixScaleIssues()
    {
        Debug.Log("🔧 スケール問題の修正を開始...");
        
        // カメラ設定の修正
        FixCameraSettings();
        
        // Canvas設定の修正
        FixCanvasSettings();
        
        Debug.Log("✅ スケール修正完了！");
        
        if (showDebugInfo)
        {
            AnalyzeCurrentSettings();
        }
    }
    
    /// <summary>
    /// カメラ設定を修正
    /// </summary>
    private void FixCameraSettings()
    {
        if (mainCamera == null) return;
        
        if (mainCamera.orthographicSize != recommendedCameraSize)
        {
            float oldSize = mainCamera.orthographicSize;
            mainCamera.orthographicSize = recommendedCameraSize;
            Debug.Log($"📷 カメラサイズを修正: {oldSize} → {recommendedCameraSize}");
        }
        
        if (!mainCamera.orthographic)
        {
            mainCamera.orthographic = true;
            Debug.Log("📷 カメラをOrthographicモードに変更");
        }
    }
    
    /// <summary>
    /// Canvas設定を修正
    /// </summary>
    private void FixCanvasSettings()
    {
        foreach (var scaler in canvasScalers)
        {
            bool wasModified = false;
            
            // Scale Factorの修正
            if (Mathf.Abs(scaler.scaleFactor - recommendedCanvasScaleFactor) > 0.01f)
            {
                float oldScale = scaler.scaleFactor;
                scaler.scaleFactor = recommendedCanvasScaleFactor;
                Debug.Log($"📊 {scaler.name} Scale Factorを修正: {oldScale} → {recommendedCanvasScaleFactor}");
                wasModified = true;
            }
            
            // Reference Resolutionの修正
            if (scaler.referenceResolution != recommendedReferenceResolution)
            {
                Vector2 oldResolution = scaler.referenceResolution;
                scaler.referenceResolution = recommendedReferenceResolution;
                Debug.Log($"📊 {scaler.name} Reference Resolutionを修正: {oldResolution} → {recommendedReferenceResolution}");
                wasModified = true;
            }
            
            // UI Scale Modeの推奨設定
            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                Debug.Log($"📊 {scaler.name} UI Scale Modeを 'Scale With Screen Size' に変更");
                wasModified = true;
            }
            
            if (wasModified)
            {
                // 変更を即座に反映
                scaler.enabled = false;
                scaler.enabled = true;
            }
        }
    }
    
    /// <summary>
    /// Cainosアセット推奨設定を適用
    /// </summary>
    [ContextMenu("Cainos推奨設定を適用")]
    public void ApplyCainosRecommendedSettings()
    {
        recommendedCameraSize = 5f;
        recommendedCanvasScaleFactor = 1f;
        recommendedReferenceResolution = new Vector2(1920, 1080);
        
        FixScaleIssues();
        Debug.Log("🎮 Cainos推奨設定を適用しました");
    }
    
    /// <summary>
    /// モバイル向け設定を適用
    /// </summary>
    [ContextMenu("モバイル向け設定を適用")]
    public void ApplyMobileSettings()
    {
        recommendedCameraSize = 6f;
        recommendedCanvasScaleFactor = 1f;
        recommendedReferenceResolution = new Vector2(1080, 1920); // 縦向き
        
        FixScaleIssues();
        Debug.Log("📱 モバイル向け設定を適用しました");
    }
    
    /// <summary>
    /// デスクトップ向け設定を適用
    /// </summary>
    [ContextMenu("デスクトップ向け設定を適用")]
    public void ApplyDesktopSettings()
    {
        recommendedCameraSize = 5f;
        recommendedCanvasScaleFactor = 1f;
        recommendedReferenceResolution = new Vector2(1920, 1080); // 横向き
        
        FixScaleIssues();
        Debug.Log("🖥️ デスクトップ向け設定を適用しました");
    }
} 