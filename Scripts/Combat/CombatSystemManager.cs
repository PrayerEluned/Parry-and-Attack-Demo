using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 戦闘システム全体を管理し、各コンポーネント間の連携を設定するマネージャー
/// </summary>
public class CombatSystemManager : MonoBehaviour
{
    [Header("Combat Controllers")]
    [SerializeField] private CombatModeController combatModeController;
    [SerializeField] private DefenseController defenseController;
    [SerializeField] private BasicAttackController basicAttackController;
    
    [Header("UI Components")]
    [SerializeField] private CombatModeUI combatModeUI;
    [SerializeField] private AttackGaugeUI attackGaugeUI;
    
    [Header("UI Buttons")]
    [SerializeField] private Button modeToggleButton;
    [SerializeField] private Button manualAttackButton;
    [SerializeField] private Button defenseButton;
    
    [Header("UI Panels")]
    [SerializeField] private GameObject autoModePanel;  // AttackGaugeCanvas
    [SerializeField] private GameObject manualModePanel; // ManualModeContainer
    
    private void Start()
    {
        Application.targetFrameRate = 60;
        SetupCombatSystem();
    }
    
    /// <summary>
    /// 戦闘システム全体のセットアップ
    /// </summary>
    private void SetupCombatSystem()
    {
        // 参照の自動取得を試行
        AutoFindReferences();
        
        // 参照が正しく設定されているかチェック
        if (!ValidateReferences())
        {
            Debug.LogError("CombatSystemManager: 必要な参照が不足しています。手動で設定してください。");
            return;
        }
        
        // 各コンポーネントに参照を設定
        SetupCombatModeController();
        SetupCombatModeUI();
        
        // 初期状態の設定
        SetInitialState();
        
        Debug.Log("CombatSystemManager: 戦闘システムのセットアップが完了しました。");
    }
    
    /// <summary>
    /// 参照の自動取得
    /// </summary>
    private void AutoFindReferences()
    {
        // Combat Controllers
        if (!combatModeController)
            combatModeController = FindFirstObjectByType<CombatModeController>();
        if (!defenseController)
            defenseController = FindFirstObjectByType<DefenseController>();
        if (!basicAttackController)
            basicAttackController = FindFirstObjectByType<BasicAttackController>();
        
        // UI Components
        if (!combatModeUI)
            combatModeUI = FindFirstObjectByType<CombatModeUI>();
        if (!attackGaugeUI)
            attackGaugeUI = FindFirstObjectByType<AttackGaugeUI>();
        
        // UI Buttons (名前で検索)
        if (!modeToggleButton)
        {
            var toggleObj = GameObject.Find("CombatModeToggleButton");
            if (toggleObj) modeToggleButton = toggleObj.GetComponent<Button>();
        }
        if (!manualAttackButton)
        {
            var attackObj = GameObject.Find("ManualAttackButton");
            if (attackObj) manualAttackButton = attackObj.GetComponent<Button>();
        }
        if (!defenseButton)
        {
            var defenseObj = GameObject.Find("DefenseButton");
            if (defenseObj) defenseButton = defenseObj.GetComponent<Button>();
        }
        
        // UI Panels
        if (!autoModePanel)
        {
            var gaugePanelObj = GameObject.Find("AttackGaugeCanvas");
            if (gaugePanelObj) autoModePanel = gaugePanelObj;
        }
        if (!manualModePanel)
        {
            var manualPanelObj = GameObject.Find("ManualModeContainer");
            if (manualPanelObj) manualModePanel = manualPanelObj;
        }
    }
    
    /// <summary>
    /// 参照の検証
    /// </summary>
    private bool ValidateReferences()
    {
        bool isValid = true;
        
        if (!combatModeController)
        {
            Debug.LogWarning("CombatSystemManager: CombatModeController が見つかりません");
            isValid = false;
        }
        if (!basicAttackController)
        {
            Debug.LogWarning("CombatSystemManager: BasicAttackController が見つかりません");
            isValid = false;
        }
        if (!modeToggleButton)
        {
            Debug.LogWarning("CombatSystemManager: Mode Toggle Button が見つかりません");
            isValid = false;
        }
        
        return isValid;
    }
    
    /// <summary>
    /// CombatModeControllerのセットアップ
    /// </summary>
    private void SetupCombatModeController()
    {
        if (combatModeController)
        {
            // 基本的な参照は CombatModeController の Awake で自動設定される
            Debug.Log("CombatSystemManager: CombatModeController セットアップ完了");
        }
    }
    
    /// <summary>
    /// CombatModeUIのセットアップ
    /// </summary>
    private void SetupCombatModeUI()
    {
        if (combatModeUI)
        {
            // UI参照の手動設定（もしCombatModeUIに手動設定プロパティがある場合）
            Debug.Log("CombatSystemManager: CombatModeUI セットアップ完了");
        }
    }
    
    /// <summary>
    /// 初期状態の設定
    /// </summary>
    private void SetInitialState()
    {
        // オートモードから開始
        if (autoModePanel) autoModePanel.SetActive(true);
        if (manualModePanel) manualModePanel.SetActive(false);
        
        // マニュアルボタンを無効化
        if (manualAttackButton) 
        {
            manualAttackButton.interactable = false;
            var image = manualAttackButton.GetComponent<Image>();
            if (image) image.color = Color.gray;
        }
        if (defenseButton) 
        {
            defenseButton.interactable = false;
            var image = defenseButton.GetComponent<Image>();
            if (image) image.color = Color.gray;
        }
    }
    
    /// <summary>
    /// 手動でボタン参照を設定する（Inspector用）
    /// </summary>
    [ContextMenu("手動参照設定")]
    public void ManualSetupReferences()
    {
        AutoFindReferences();
        Debug.Log("CombatSystemManager: 参照を手動で再設定しました");
    }
} 