using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 攻撃モード（オート/マニュアル）を管理するコントローラー
/// </summary>
public class CombatModeController : MonoBehaviour
{
    [Header("攻撃モード設定")]
    [SerializeField] private bool isAutoMode = true; // デフォルトはオートモード
    
    [Header("イベント")]
    public UnityEvent<bool> OnModeChanged; // bool: true=オートモード, false=マニュアルモード
    
    [Header("参照")]
    [SerializeField] private BasicAttackController autoAttackController;
    [SerializeField] private DefenseController defenseController;
    
    // プロパティ
    public bool IsAutoMode => isAutoMode;
    public bool IsManualMode => !isAutoMode;
    
    // シングルトンパターン
    private static CombatModeController instance;
    public static CombatModeController Instance => instance;
    
    private void Awake()
    {
        // シングルトンの設定
        if (instance != null && instance != this)
        {
            Debug.LogWarning("CombatModeController: 重複インスタンス検出。新しいインスタンスを破棄します。");
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        // 参照の自動取得（シーン全体から検索）
        if (!autoAttackController)
            autoAttackController = FindFirstObjectByType<BasicAttackController>();
        if (!defenseController)
            defenseController = FindFirstObjectByType<DefenseController>();
    }
    
    private void Start()
    {
        // 初期モードを適用
        ApplyCurrentMode();
    }
    
    private void Update()
    {
        // マニュアルモードでのキーボード入力処理
        if (!IsAutoMode)
        {
            // Jキーでマニュアル攻撃
            if (Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log("CombatModeController: マニュアル攻撃実行");
                if (autoAttackController != null)
                {
                    autoAttackController.PerformManualAttack();
                }
            }
        }
    }
    
    /// <summary>
    /// 攻撃モードを切り替える
    /// </summary>
    public void ToggleMode()
    {
        SetMode(!isAutoMode);
    }
    
    /// <summary>
    /// 攻撃モードを設定する
    /// </summary>
    /// <param name="autoMode">true: オートモード, false: マニュアルモード</param>
    public void SetMode(bool autoMode)
    {
        if (isAutoMode == autoMode) return; // 既に同じモードの場合は何もしない
        
        isAutoMode = autoMode;
        ApplyCurrentMode();
        
        // イベント発火
        OnModeChanged?.Invoke(isAutoMode);
        
        Debug.Log($"CombatModeController: モード変更 -> {(isAutoMode ? "オートモード" : "マニュアルモード")}");
    }
    
    /// <summary>
    /// 現在のモードを適用する
    /// </summary>
    private void ApplyCurrentMode()
    {
        if (autoAttackController)
        {
            autoAttackController.enabled = isAutoMode;
        }
        
        if (defenseController)
        {
            defenseController.enabled = !isAutoMode; // マニュアルモード時のみ防御システムを有効化
        }
    }
    
    /// <summary>
    /// オートモードに設定
    /// </summary>
    [ContextMenu("オートモードに設定")]
    public void SetAutoMode()
    {
        SetMode(true);
    }
    
    /// <summary>
    /// マニュアルモードに設定
    /// </summary>
    [ContextMenu("マニュアルモードに設定")]
    public void SetManualMode()
    {
        SetMode(false);
    }
    
    /// <summary>
    /// マニュアルモード時の基本攻撃実行
    /// </summary>
    public void PerformManualAttack()
    {
        if (isAutoMode) return; // オートモード時は無効
        
        // マニュアル攻撃の実装（BasicAttackControllerの攻撃ロジックを利用）
        if (autoAttackController)
        {
            // BasicAttackControllerのパブリックメソッドを呼び出し
            autoAttackController.PerformManualAttack();
        }
        
        Debug.Log("CombatModeController: マニュアル攻撃実行");
    }
}