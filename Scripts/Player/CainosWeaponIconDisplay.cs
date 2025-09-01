using UnityEngine;

/// <summary>
/// Cainosレイヤーシステム対応の武器アイコン表示システム
/// プレイヤーのレイヤー変更に自動的に追従し、現在装備中の武器アイコンを適切な層に自動表示します
/// 
/// 【完全自動化】
/// - 武器アイコン用のSpriteRendererを自動作成（135度回転）
/// - WeaponManagerから現在装備中の武器アイコンを自動取得・表示
/// - プレイヤーのレイヤー変更に自動追従
/// - 最後の入力方向に応じて表示層、回転角度、位置、スケールを動的変更：
///   * 上向き入力：前面表示、-135度回転（上向き斬り）
///   * 左向き入力：背面表示、180度回転、左専用位置、縦1/√2スケール（左横向き斬り）
///   * 右向き入力：背面表示、-180度回転、右専用位置、縦1/√2スケール（右横向き斬り）
///   * その他：背面表示、135度回転、通常位置・スケール（通常の下向き斬り）
/// - 左右それぞれの向き時の位置はインスペクターで個別調整可能
/// </summary>
public class CainosWeaponIconDisplay : MonoBehaviour
{
    [Header("武器アイコン設定")]
    [SerializeField] private float iconScale = 1.0f;
    [SerializeField] private Vector3 iconOffset = new Vector3(0.5f, 0.5f, 0f);
    
    [Header("左右向き時の設定")]
    [SerializeField] private Vector3 leftIconOffset = new Vector3(-0.7f, 0.3f, 0f); // 左向き時の位置オフセット
    [SerializeField] private Vector3 rightIconOffset = new Vector3(0.7f, 0.3f, 0f); // 右向き時の位置オフセット
    
    [Header("レイヤー設定")]
    [SerializeField] private int sortingOrderOffset = -1; // プレイヤーより後ろに表示
    [SerializeField] private int upwardSortingOrderOffset = 1; // 上向き移動時はプレイヤーより前
    
    [Header("デバッグ情報")]
    [SerializeField] private bool showDebugInfo = false;
    
    [Header("スキル表示設定")]
    [SerializeField] private bool isSkillMode = false; // スキル表示モードかどうか
    [SerializeField] private float skillIconScale = 1.0f; // スキルアイコンのスケール
    [SerializeField] private float skillIconAlpha = 0.8f; // スキルアイコンの透明度
    [SerializeField] private Vector3 skillIconOffset = new Vector3(0f, 0f, 0f); // スキルアイコンの位置オフセット
    
    // 内部参照
    private WeaponManager weaponManager;
    private CharacterMovement characterMovement;
    private SpriteRenderer playerSpriteRenderer;
    private SpriteRenderer iconRenderer; // 武器アイコン用（自動作成）
    private Sprite lastDisplayedIcon;
    private string currentSortingLayerName;
    private int currentSortingOrder;
    
    // 最後の入力方向検知用
    private bool isLastInputUpward = false;
    private bool isLastInputHorizontal = false;
    private bool isLastInputLeft = false; // 左向き判定用
    private Vector2 lastCheckedDirection = Vector2.zero;
    
    private void Awake()
    {
        // 必要なコンポーネントを取得
        weaponManager = GetComponent<WeaponManager>();
        characterMovement = GetComponent<CharacterMovement>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        
        // 武器アイコン用のSpriteRendererを自動作成
        if (iconRenderer == null)
        {
            CreateWeaponIconRenderer();
        }
        
        // 初期レイヤー設定
        InitializeIconLayer();
    }
    
    private void Start()
    {
        // 初回の武器アイコン更新
        UpdateWeaponIcon();
        UpdateIconLayer();
    }
    
    private void Update()
    {
        // 最後の入力方向を検知
        UpdateLastInputDirection();
        
        // 武器が変更されたかチェック
        UpdateWeaponIcon();
        
        // プレイヤーのレイヤーが変更されたかチェック
        UpdateIconLayer();
    }
    
    /// <summary>
    /// 武器アイコン用のSpriteRendererを作成
    /// </summary>
    private void CreateWeaponIconRenderer()
    {
        // 子オブジェクトとして武器アイコン用のGameObjectを作成
        GameObject iconObject = new GameObject("WeaponIcon");
        iconObject.transform.SetParent(transform);
        iconObject.transform.localPosition = iconOffset;
        iconObject.transform.localScale = Vector3.one * iconScale;
        
        // SpriteRendererを追加
        iconRenderer = iconObject.AddComponent<SpriteRenderer>();
        iconRenderer.sortingOrder = sortingOrderOffset;
        
        // 135度回転を適用
        iconObject.transform.rotation = Quaternion.Euler(0, 0, 135f);
        
        // 重要：適切なサイズと位置を強制設定
        iconObject.transform.localScale = Vector3.one * iconScale;
        iconObject.transform.localPosition = iconOffset;
        
        if (showDebugInfo)
        {
            Debug.Log($"[CainosWeaponIconDisplay] 武器アイコンRendererを作成しました。位置: {iconOffset}, 回転: 135度, スケール: {iconScale}");
        }
    }
    
    /// <summary>
    /// 初期のアイコンレイヤー設定
    /// </summary>
    private void InitializeIconLayer()
    {
        if (iconRenderer == null || playerSpriteRenderer == null) return;
        
        // プレイヤーと同じソーティングレイヤーに設定
        iconRenderer.sortingLayerName = playerSpriteRenderer.sortingLayerName;
        iconRenderer.sortingOrder = playerSpriteRenderer.sortingOrder + sortingOrderOffset;
        
        currentSortingLayerName = iconRenderer.sortingLayerName;
        currentSortingOrder = iconRenderer.sortingOrder;
        
        if (showDebugInfo)
        {
            Debug.Log($"[CainosWeaponIconDisplay] 初期レイヤー設定完了。SortingLayer: {currentSortingLayerName}, Order: {currentSortingOrder}");
        }
    }
    
    /// <summary>
    /// 武器アイコンの更新（武器が変更された場合）
    /// </summary>
    private void UpdateWeaponIcon()
    {
        if (weaponManager == null || iconRenderer == null) return;
        
        Sprite newIcon = null;
        
        // 現在装備中の武器からアイコンを取得
        if (weaponManager.currentWeapon != null && weaponManager.currentWeapon.weaponItem != null)
        {
            newIcon = weaponManager.currentWeapon.weaponItem.icon;
        }
        
        // アイコンが変更された場合のみ更新
        if (newIcon != lastDisplayedIcon)
        {
            iconRenderer.sprite = newIcon;
            lastDisplayedIcon = newIcon;
            
            // アイコンの表示/非表示を制御
            iconRenderer.gameObject.SetActive(newIcon != null);
            
            if (showDebugInfo)
            {
                if (newIcon != null)
                {
                    Debug.Log($"[CainosWeaponIconDisplay] 武器アイコンを更新: {weaponManager.currentWeapon.weaponItem.weaponName}");
                }
                else
                {
                    Debug.Log("[CainosWeaponIconDisplay] 武器アイコンを非表示にしました（武器未装備）");
                }
            }
        }
    }
    
    /// <summary>
    /// アイコンのレイヤー更新（プレイヤーのレイヤーが変更された場合）
    /// </summary>
    private void UpdateIconLayer()
    {
        if (iconRenderer == null || playerSpriteRenderer == null) return;
        
        string playerSortingLayer = playerSpriteRenderer.sortingLayerName;
        int playerSortingOrder = playerSpriteRenderer.sortingOrder;
        
        // プレイヤーのソーティングレイヤーが変更された場合
        if (playerSortingLayer != currentSortingLayerName)
        {
            iconRenderer.sortingLayerName = playerSortingLayer;
            currentSortingLayerName = playerSortingLayer;
            
            if (showDebugInfo)
            {
                Debug.Log($"[CainosWeaponIconDisplay] ソーティングレイヤーを更新: {currentSortingLayerName}");
            }
        }
        
        // スキルモードの場合は常に前面表示
        if (isSkillMode)
        {
            int skillSortingOrder = playerSortingOrder + 10; // スキルは常に前面
            if (iconRenderer.sortingOrder != skillSortingOrder)
            {
                iconRenderer.sortingOrder = skillSortingOrder;
                currentSortingOrder = skillSortingOrder;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[CainosWeaponIconDisplay] スキル表示用ソーティングオーダーを更新: {currentSortingOrder}");
                }
            }
            return; // スキルモードの場合は通常の方向制御をスキップ
        }
        
        // 最後の入力方向に応じてソーティングオーダーを決定
        int orderOffset = isLastInputUpward ? upwardSortingOrderOffset : sortingOrderOffset;
        int targetSortingOrder = playerSortingOrder + orderOffset;
        
        if (iconRenderer.sortingOrder != targetSortingOrder)
        {
            iconRenderer.sortingOrder = targetSortingOrder;
            currentSortingOrder = targetSortingOrder;
            
            if (showDebugInfo)
            {
                string direction = isLastInputUpward ? "上向き入力中" : "通常";
                Debug.Log($"[CainosWeaponIconDisplay] ソーティングオーダーを更新: {currentSortingOrder} ({direction})");
            }
        }
        
        // 方向に応じて武器アイコンの回転制御（スキルモードでない場合のみ）
        if (iconRenderer != null && !isSkillMode)
        {
            // 武器が装備されている場合は常に表示
            bool shouldShowIcon = lastDisplayedIcon != null;
            
            // 表示/非表示制御
            if (iconRenderer.gameObject.activeSelf != shouldShowIcon)
            {
                iconRenderer.gameObject.SetActive(shouldShowIcon);
            }
            
            // 表示時のみ回転制御
            if (shouldShowIcon)
            {
                float targetRotation;
                string rotationReason;
                
                // 入力方向に応じて回転角度を決定
                if (isLastInputUpward)
                {
                    targetRotation = -135f; // 上向き
                    rotationReason = "上向き(-135度)";
                }
                else if (isLastInputHorizontal)
                {
                    if (isLastInputLeft)
                    {
                        targetRotation = 180f; // 左向き（270度から-90度）
                        rotationReason = "左向き(180度)";
                    }
                    else
                    {
                        targetRotation = -180f; // 右向き（-45度から-135度）
                        rotationReason = "右向き(-180度)";
                    }
                }
                else
                {
                    targetRotation = 135f; // 通常（下向き等）
                    rotationReason = "通常(135度)";
                }
                
                float currentRotation = iconRenderer.transform.rotation.eulerAngles.z;
                
                // 角度を正規化して比較
                float normalizedCurrent = currentRotation > 180f ? currentRotation - 360f : currentRotation;
                float normalizedTarget = targetRotation > 180f ? targetRotation - 360f : targetRotation;
                
                if (Mathf.Abs(normalizedCurrent - normalizedTarget) > 1f)
                {
                    iconRenderer.transform.rotation = Quaternion.Euler(0, 0, targetRotation);
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"[CainosWeaponIconDisplay] 武器アイコン回転変更: {rotationReason}");
                    }
                }
                
                // 左右向きの時の位置とスケール調整
                Vector3 targetPosition;
                Vector3 targetScale;
                
                if (isLastInputHorizontal)
                {
                    // 左右向きの時：専用オフセットと縦横スケール調整
                    targetPosition = isLastInputLeft ? leftIconOffset : rightIconOffset;
                    float verticalScale = iconScale / Mathf.Sqrt(2f); // 縦：1/√2倍
                    float horizontalScale = iconScale * 0.9f; // 横：0.9倍
                    targetScale = new Vector3(horizontalScale, verticalScale, 1f);
                }
                else
                {
                    // 通常時：通常のオフセットとスケール
                    targetPosition = iconOffset;
                    targetScale = Vector3.one * iconScale;
                }
                
                // 位置の更新
                if (Vector3.Distance(iconRenderer.transform.localPosition, targetPosition) > 0.01f)
                {
                    iconRenderer.transform.localPosition = targetPosition;
                }
                
                // スケールの更新
                if (Vector3.Distance(iconRenderer.transform.localScale, targetScale) > 0.01f)
                {
                    iconRenderer.transform.localScale = targetScale;
                }
            }
        }
    }
    
    /// <summary>
    /// プレイヤーの最後の入力方向を検知
    /// </summary>
    private void UpdateLastInputDirection()
    {
        if (characterMovement == null) return;
        
        // CharacterMovementから最後の移動方向を取得
        Vector2 currentLastDirection = characterMovement.LastMoveDirection;
        

        
        // 方向が変わった場合のみ処理
        if (currentLastDirection != lastCheckedDirection)
        {
            bool wasInputUpward = isLastInputUpward;
            bool wasInputHorizontal = isLastInputHorizontal;
            bool wasInputLeft = isLastInputLeft;
            
            // 入力方向判定
            float absX = Mathf.Abs(currentLastDirection.x);
            float absY = Mathf.Abs(currentLastDirection.y);
            
            if (absX > 0.1f && absX > absY)
            {
                // 左右入力（X方向が主要）
                isLastInputUpward = false;
                isLastInputHorizontal = true;
                isLastInputLeft = currentLastDirection.x < 0; // 左向きかどうか判定
            }
            else if (currentLastDirection.y > 0.1f)
            {
                // 上向き入力
                isLastInputUpward = true;
                isLastInputHorizontal = false;
                // 左右情報はリセットしない（維持）
            }
            else
            {
                // その他（下向きや停止）
                isLastInputUpward = false;
                isLastInputHorizontal = false;
                // 左右情報はリセットしない（維持）
            }
            
            if (showDebugInfo && (wasInputUpward != isLastInputUpward || wasInputHorizontal != isLastInputHorizontal || wasInputLeft != isLastInputLeft))
            {
                string directionName = isLastInputHorizontal ? (isLastInputLeft ? "左向き" : "右向き") : (isLastInputUpward ? "上向き" : "その他");
                Debug.Log($"[CainosWeaponIconDisplay] 入力方向変更: {directionName}");
            }
            
            lastCheckedDirection = currentLastDirection;
        }
    }
    
    /// <summary>
    /// 手動でアイコンを設定（外部から呼び出し可能）
    /// </summary>
    public void SetWeaponIcon(Sprite icon)
    {
        if (iconRenderer != null)
        {
            iconRenderer.sprite = icon;
            lastDisplayedIcon = icon;
            iconRenderer.gameObject.SetActive(icon != null);
            
            if (showDebugInfo)
            {
                Debug.Log($"[CainosWeaponIconDisplay] 手動で武器アイコンを設定: {(icon != null ? icon.name : "null")}");
            }
        }
    }
    
    /// <summary>
    /// アイコンの位置オフセットを設定（通常時）
    /// </summary>
    public void SetIconOffset(Vector3 offset)
    {
        iconOffset = offset;
        if (iconRenderer != null && !isLastInputHorizontal)
        {
            iconRenderer.transform.localPosition = iconOffset;
        }
    }
    
    /// <summary>
    /// 左向き時のアイコン位置オフセットを設定
    /// </summary>
    public void SetLeftIconOffset(Vector3 offset)
    {
        leftIconOffset = offset;
        if (iconRenderer != null && isLastInputHorizontal && isLastInputLeft)
        {
            iconRenderer.transform.localPosition = leftIconOffset;
        }
    }
    
    /// <summary>
    /// 右向き時のアイコン位置オフセットを設定
    /// </summary>
    public void SetRightIconOffset(Vector3 offset)
    {
        rightIconOffset = offset;
        if (iconRenderer != null && isLastInputHorizontal && !isLastInputLeft)
        {
            iconRenderer.transform.localPosition = rightIconOffset;
        }
    }
    
    /// <summary>
    /// アイコンのスケールを設定
    /// </summary>
    public void SetIconScale(float scale)
    {
        iconScale = scale;
        if (iconRenderer != null)
        {
            iconRenderer.transform.localScale = Vector3.one * iconScale;
        }
    }
    
    /// <summary>
    /// ソーティングオーダーオフセットを設定
    /// </summary>
    public void SetSortingOrderOffset(int offset)
    {
        sortingOrderOffset = offset;
        // 即座にレイヤーを更新
        UpdateIconLayer();
    }
    
    /// <summary>
    /// 上向き移動時のソーティングオーダーオフセットを設定
    /// </summary>
    public void SetUpwardSortingOrderOffset(int offset)
    {
        upwardSortingOrderOffset = offset;
        // 即座にレイヤーを更新
        UpdateIconLayer();
    }
    
    /// <summary>
    /// デバッグ情報の表示切り替え
    /// </summary>
    public void SetDebugMode(bool enabled)
    {
        showDebugInfo = enabled;
    }
    
    /// <summary>
    /// スキルアイコンを設定（外部から呼び出し可能）
    /// </summary>
    public void SetSkillIcon(Sprite icon, float scale = 1.0f, float alpha = 0.8f, Vector3? offset = null)
    {
        if (iconRenderer != null)
        {
            isSkillMode = true;
            iconRenderer.sprite = icon;
            lastDisplayedIcon = icon;
            iconRenderer.gameObject.SetActive(icon != null);
            
            // スキル用の設定を適用
            skillIconScale = scale;
            skillIconAlpha = alpha;
            if (offset.HasValue)
            {
                skillIconOffset = offset.Value;
            }
            
            // スキル表示用の設定を即座に適用
            iconRenderer.transform.localScale = Vector3.one * skillIconScale;
            iconRenderer.color = new Color(1, 1, 1, skillIconAlpha);
            iconRenderer.transform.localPosition = skillIconOffset;
            
            if (showDebugInfo)
            {
                Debug.Log($"[CainosWeaponIconDisplay] スキルアイコンを設定: {(icon != null ? icon.name : "null")}, スケール: {skillIconScale}, 透明度: {skillIconAlpha}");
            }
        }
    }
    
    /// <summary>
    /// スキル表示モードを終了して通常の武器アイコン表示に戻す
    /// </summary>
    public void EndSkillMode()
    {
        isSkillMode = false;
        UpdateWeaponIcon(); // 通常の武器アイコン表示に戻す
        UpdateIconLayer(); // レイヤー設定も更新
        
        if (showDebugInfo)
        {
            Debug.Log("[CainosWeaponIconDisplay] スキル表示モードを終了しました");
        }
    }
    
    /// <summary>
    /// スキルアイコンの位置を更新
    /// </summary>
    public void UpdateSkillIconPosition(Vector3 position)
    {
        if (iconRenderer != null && isSkillMode)
        {
            iconRenderer.transform.position = position;
        }
    }
    
    /// <summary>
    /// スキルアイコンの回転を更新
    /// </summary>
    public void UpdateSkillIconRotation(float angle)
    {
        if (iconRenderer != null && isSkillMode)
        {
            iconRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    private void OnDestroy()
    {
        // 必要に応じてクリーンアップ処理
    }
} 