using UnityEngine;

/// <summary>
/// ダメージテキストのSortingLayerを確実に設定するためのスクリプト
/// </summary>
public class DamageTextLayerFixer : MonoBehaviour
{
    [Header("Sorting Layer Settings")]
    [SerializeField] private string targetSortingLayerName = "Player";
    [SerializeField] private int targetSortingOrder = 3;
    
    [Header("Auto Fix Settings")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool autoFixOnEnable = true;
    
    private Canvas canvas;
    private DamageText damageText;
    
    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        damageText = GetComponent<DamageText>();
        
        if (canvas == null)
        {
            Debug.LogError("DamageTextLayerFixer: Canvas component not found!");
            return;
        }
    }
    
    private void Start()
    {
        if (autoFixOnStart)
        {
            FixSortingLayer();
        }
    }
    
    private void OnEnable()
    {
        if (autoFixOnEnable)
        {
            FixSortingLayer();
        }
    }
    
    /// <summary>
    /// SortingLayerを修正する
    /// </summary>
    public void FixSortingLayer()
    {
        if (canvas == null) return;
        
        // プレイヤーのSortingLayerを取得
        var playerSpriteRenderer = FindFirstObjectByType<PlayerStats>()?.GetComponent<SpriteRenderer>();
        if (playerSpriteRenderer != null)
        {
            targetSortingLayerName = playerSpriteRenderer.sortingLayerName;
            targetSortingOrder = playerSpriteRenderer.sortingOrder + 2;
        }
        
        // Canvasの設定を確実に行う
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingLayerName = targetSortingLayerName;
        canvas.sortingOrder = targetSortingOrder;
    }
    
    /// <summary>
    /// 手動でSortingLayerを設定
    /// </summary>
    /// <param name="layerName">SortingLayer名</param>
    /// <param name="order">Order in Layer</param>
    public void SetSortingLayer(string layerName, int order)
    {
        if (canvas == null) return;
        
        canvas.sortingLayerName = layerName;
        canvas.sortingOrder = order;
    }
    
    /// <summary>
    /// 現在の設定をログ出力
    /// </summary>
    [ContextMenu("Log Current Settings")]
    public void LogCurrentSettings()
    {
        if (canvas == null) return;
        
        // デバッグ時のみログ出力
        // Debug.Log($"DamageTextLayerFixer: Current sorting layer = '{canvas.sortingLayerName}', order = {canvas.sortingOrder}");
    }
}