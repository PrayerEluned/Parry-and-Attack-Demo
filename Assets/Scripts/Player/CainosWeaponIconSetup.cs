using UnityEngine;

/// <summary>
/// CainosWeaponIconDisplayの設定とセットアップを支援するヘルパークラス
/// 
/// 【使用方法】
/// 1. プレイヤーGameObjectにCainosWeaponIconSetupコンポーネントを追加
/// 2. 必要に応じてアイコンの位置オフセットやスケールを調整
/// 3. 実行時に自動的にCainosWeaponIconDisplayがセットアップされます
/// 
/// 【完全自動化】
/// - 現在装備中の武器アイコンを自動取得・表示（135度回転）
/// - プレイヤーのレイヤー変更に自動追従
/// - 最後の入力方向に応じて表示層を動的変更（上向き入力時は前面表示）
/// - 上向き入力時は武器アイコンを水平反転（より自然な見た目）
/// - 武器変更時のアイコン自動更新
/// - SpriteRendererの自動作成
/// 
/// 【必要なコンポーネント】
/// - WeaponManager (現在の武器情報を取得)
/// - CharacterMovement (最後の入力方向を取得)
/// - SpriteRenderer (プレイヤーのソーティングレイヤー取得)
/// </summary>
[System.Serializable]
public class WeaponIconSettings
{
    [Header("位置設定")]
    [Tooltip("プレイヤーからの相対位置")]
    public Vector3 iconOffset = new Vector3(0.5f, 0.5f, 0f);
    
    [Header("スケール設定")]
    [Tooltip("アイコンのサイズ倍率")]
    [Range(0.1f, 3.0f)]
    public float iconScale = 1.0f;
    
    [Header("レイヤー設定")]
    [Tooltip("プレイヤーのソーティングオーダーからのオフセット（負の値でプレイヤーより後ろに表示）")]
    [Range(-50, 50)]
    public int sortingOrderOffset = -1;
    
    [Tooltip("上向き移動時のソーティングオーダーオフセット（正の値でプレイヤーより前に表示）")]
    [Range(-50, 50)]
    public int upwardSortingOrderOffset = 1;
    
    [Header("デバッグ")]
    [Tooltip("デバッグログを表示するか")]
    public bool showDebugInfo = false;
}

public class CainosWeaponIconSetup : MonoBehaviour
{
    [Header("武器アイコン設定")]
    [SerializeField] private WeaponIconSettings iconSettings = new WeaponIconSettings();
    
    [Header("自動セットアップ")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool removeAfterSetup = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupWeaponIconDisplay();
            
            if (removeAfterSetup)
            {
                // セットアップ後にこのヘルパーコンポーネントを削除
                Destroy(this);
            }
        }
    }
    
    /// <summary>
    /// CainosWeaponIconDisplayを自動セットアップ
    /// </summary>
    [ContextMenu("武器アイコン表示をセットアップ")]
    public void SetupWeaponIconDisplay()
    {
        // 既存のCainosWeaponIconDisplayコンポーネントをチェック
        CainosWeaponIconDisplay existingIcon = GetComponent<CainosWeaponIconDisplay>();
        
        if (existingIcon == null)
        {
            // 新しく追加
            existingIcon = gameObject.AddComponent<CainosWeaponIconDisplay>();
            Debug.Log("[CainosWeaponIconSetup] CainosWeaponIconDisplayコンポーネントを追加しました");
        }
        
        // 設定を適用
        existingIcon.SetIconOffset(iconSettings.iconOffset);
        existingIcon.SetIconScale(iconSettings.iconScale);
        existingIcon.SetSortingOrderOffset(iconSettings.sortingOrderOffset);
        existingIcon.SetUpwardSortingOrderOffset(iconSettings.upwardSortingOrderOffset);
        existingIcon.SetDebugMode(iconSettings.showDebugInfo);
        
        // 必要なコンポーネントの確認
        ValidateRequiredComponents();
        
        Debug.Log("[CainosWeaponIconSetup] 武器アイコン表示のセットアップが完了しました");
    }
    
    /// <summary>
    /// 必要なコンポーネントが揃っているかチェック
    /// </summary>
    private void ValidateRequiredComponents()
    {
        bool hasWeaponManager = GetComponent<WeaponManager>() != null;
        bool hasCharacterMovement = GetComponent<CharacterMovement>() != null;
        bool hasSpriteRenderer = GetComponent<SpriteRenderer>() != null;
        
        if (!hasWeaponManager)
        {
            Debug.LogWarning("[CainosWeaponIconSetup] WeaponManagerコンポーネントが必要です");
        }
        
        if (!hasCharacterMovement)
        {
            Debug.LogWarning("[CainosWeaponIconSetup] CharacterMovementコンポーネントが必要です");
        }
        
        if (!hasSpriteRenderer)
        {
            Debug.LogWarning("[CainosWeaponIconSetup] SpriteRendererコンポーネントが必要です");
        }
        
        if (hasWeaponManager && hasCharacterMovement && hasSpriteRenderer)
        {
            Debug.Log("[CainosWeaponIconSetup] ✅ 必要なコンポーネントが全て揃っています");
        }
    }
    
    /// <summary>
    /// 設定を更新（インスペクターから呼び出し用）
    /// </summary>
    [ContextMenu("設定を適用")]
    public void ApplySettings()
    {
        CainosWeaponIconDisplay iconDisplay = GetComponent<CainosWeaponIconDisplay>();
        if (iconDisplay != null)
        {
            iconDisplay.SetIconOffset(iconSettings.iconOffset);
            iconDisplay.SetIconScale(iconSettings.iconScale);
            iconDisplay.SetSortingOrderOffset(iconSettings.sortingOrderOffset);
            iconDisplay.SetUpwardSortingOrderOffset(iconSettings.upwardSortingOrderOffset);
            iconDisplay.SetDebugMode(iconSettings.showDebugInfo);
            Debug.Log("[CainosWeaponIconSetup] 設定を適用しました");
        }
        else
        {
            Debug.LogWarning("[CainosWeaponIconSetup] CainosWeaponIconDisplayコンポーネントが見つかりません");
        }
    }
} 