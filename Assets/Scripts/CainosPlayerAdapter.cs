using UnityEngine;

/// <summary>
/// 既存プレイヤーをCainosシステムに対応させるアダプター
/// 既存のプレイヤーコンポーネントに追加して使用
/// </summary>
public class CainosPlayerAdapter : MonoBehaviour
{
    [Header("Cainos レイヤー設定")]
    [SerializeField] private string defaultLayer = "Layer 1";
    [SerializeField] private string defaultSortingLayer = "Layer 1";
    
    [Header("設定")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    private SpriteRenderer playerSpriteRenderer;
    private SpriteRenderer[] childSpriteRenderers;

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupCainosCompatibility();
        }
    }

    /// <summary>
    /// プレイヤーをCainosシステムに対応させる
    /// </summary>
    public void SetupCainosCompatibility()
    {
        // 必要なコンポーネントをチェック
        if (!CheckRequiredComponents())
        {
            return;
        }

        // レイヤーを設定
        SetPlayerLayer(defaultLayer);
        
        // ソーティングレイヤーを設定
        SetPlayerSortingLayer(defaultSortingLayer);
        
        // タグを設定（まだ設定されていない場合）
        if (gameObject.tag == "Untagged")
        {
            gameObject.tag = "Player";
        }
        
        Debug.Log($"プレイヤーをCainosシステムに対応させました: Layer={defaultLayer}, SortingLayer={defaultSortingLayer}");
    }

    /// <summary>
    /// 必要なコンポーネントがあるかチェック
    /// </summary>
    private bool CheckRequiredComponents()
    {
        bool hasRigidbody = GetComponent<Rigidbody2D>() != null;
        bool hasCollider = GetComponent<Collider2D>() != null;
        bool hasSpriteRenderer = GetComponent<SpriteRenderer>() != null;

        if (!hasRigidbody)
        {
            Debug.LogWarning("プレイヤーにRigidbody2Dが必要です");
        }
        
        if (!hasCollider)
        {
            Debug.LogWarning("プレイヤーにCollider2Dが必要です");
        }
        
        if (!hasSpriteRenderer)
        {
            Debug.LogWarning("プレイヤーにSpriteRendererが必要です");
        }

        return hasRigidbody && hasCollider && hasSpriteRenderer;
    }

    /// <summary>
    /// プレイヤーのレイヤーを設定
    /// </summary>
    public void SetPlayerLayer(string layerName)
    {
        int layerIndex = LayerMask.NameToLayer(layerName);
        if (layerIndex != -1)
        {
            gameObject.layer = layerIndex;
            Debug.Log($"プレイヤーレイヤーを '{layerName}' に設定しました");
        }
        else
        {
            Debug.LogError($"レイヤー '{layerName}' が見つかりません。Project Settings > Tags and Layers で設定してください。");
        }
    }

    /// <summary>
    /// プレイヤーのソーティングレイヤーを設定
    /// </summary>
    public void SetPlayerSortingLayer(string sortingLayerName)
    {
        // メインのSpriteRenderer
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sortingLayerName = sortingLayerName;
        }

        // 子オブジェクトのSpriteRenderer
        childSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in childSpriteRenderers)
        {
            sr.sortingLayerName = sortingLayerName;
        }
        
        Debug.Log($"プレイヤーソーティングレイヤーを '{sortingLayerName}' に設定しました");
    }

    /// <summary>
    /// 現在のレイヤー情報を取得
    /// </summary>
    public string GetCurrentLayerInfo()
    {
        string layerName = LayerMask.LayerToName(gameObject.layer);
        string sortingLayerName = playerSpriteRenderer?.sortingLayerName ?? "None";
        return $"Layer: {layerName} ({gameObject.layer}), SortingLayer: {sortingLayerName}";
    }

    /// <summary>
    /// Inspector用ボタン: セットアップ実行
    /// </summary>
    [ContextMenu("Cainosシステムセットアップ")]
    public void ForceSetup()
    {
        SetupCainosCompatibility();
    }

    /// <summary>
    /// Inspector用ボタン: 現在の設定表示
    /// </summary>
    [ContextMenu("現在の設定を表示")]
    public void ShowCurrentSettings()
    {
        Debug.Log(GetCurrentLayerInfo());
    }
} 