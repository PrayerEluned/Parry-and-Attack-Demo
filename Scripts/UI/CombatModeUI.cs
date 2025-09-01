using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 攻撃モードに応じたUI表示を管理するコントローラー
/// </summary>
public class CombatModeUI : MonoBehaviour
{
    [Header("ボタン参照")]
    [SerializeField] private Button modeToggleButton;
    [SerializeField] private Button manualAttackButton;
    [SerializeField] private Button defenseButton; // 防御ボタン
    [SerializeField] private Button parryButton; // パリィボタン（新規追加）
    
    [Header("UI要素")]
    [SerializeField] private AttackGaugeUI attackGaugeUI;
    [SerializeField] private GameObject autoModePanel;
    [SerializeField] private GameObject manualModePanel;
    
    [Header("ボタンの色設定")]
    [SerializeField] private Color enabledColor = Color.white;
    [SerializeField] private Color disabledColor = Color.gray;
    [SerializeField] private Color defendingColor = Color.blue;
    [SerializeField] private Color parrySuccessColor = Color.yellow;
    [SerializeField] private Color cooldownColor = Color.Lerp(Color.white, Color.gray, 0.5f);
    
    [Header("ボタン状態")]
    [SerializeField] private DefenseButtonState currentDefenseButtonState = DefenseButtonState.Normal;
    [SerializeField] private DefenseButtonState currentParryButtonState = DefenseButtonState.Normal;
    
    [Header("ボタン色設定")]
    [SerializeField] private Color parryWindowColor = Color.red; // パリィウィンドウ中の色
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebugInfo = true;
    
    public enum DefenseButtonState
    {
        Normal,         // 通常（白）
        Defending,      // 防御中（青）
        ParrySuccess,   // パリィ成功（黄）
        Cooldown        // クールダウン（グレー）
    }
    
    [Header("テキスト表示")]
    [SerializeField] private Text modeToggleText;
    [SerializeField] private Text attackButtonText;
    [SerializeField] private Text defenseButtonText;
    
    private CombatModeController combatModeController;
    private DefenseController defenseController;
    
    private void Awake()
    {
        // 参照の自動取得
        combatModeController = FindFirstObjectByType<CombatModeController>();
        defenseController = FindFirstObjectByType<DefenseController>();
        
        // ボタンイベントの設定
        if (modeToggleButton)
            modeToggleButton.onClick.AddListener(ToggleCombatMode);
        if (manualAttackButton)
            manualAttackButton.onClick.AddListener(PerformManualAttack);
        if (defenseButton)
        {
            // 防御ボタンはクリックで防御開始
            defenseButton.onClick.AddListener(OnDefenseButtonClicked);
        }
        if (parryButton)
        {
            // パリィボタンはクリックでパリィ実行
            parryButton.onClick.AddListener(OnParryButtonClicked);
        }
    }
    
    private void Start()
    {
        // CombatModeControllerからのイベント購読
        if (combatModeController)
        {
            combatModeController.OnModeChanged.AddListener(OnModeChanged);
            
            // 初期状態を適用
            OnModeChanged(combatModeController.IsAutoMode);
        }
        
        // イベントの登録
        if (defenseController)
        {
            defenseController.OnDefenseStart.AddListener(OnDefenseStart);
            defenseController.OnDefenseEnd.AddListener(OnDefenseEnd);
            defenseController.OnParrySuccess.AddListener(OnParrySuccess);
        }
        
        // パリィボタンの初期状態を設定
        if (parryButton)
        {
            UpdateParryButtonState(DefenseButtonState.Normal);
        }
    }
    
    /// <summary>
    /// 攻撃モード切り替え
    /// </summary>
    private void ToggleCombatMode()
    {
        if (combatModeController)
        {
            combatModeController.ToggleMode();
        }
    }
    
    /// <summary>
    /// マニュアル攻撃実行
    /// </summary>
    private void PerformManualAttack()
    {
        if (combatModeController)
        {
            // 防御中なら防御を解除
            if (defenseController && defenseController.CurrentState == DefenseController.DefenseState.Defending)
            {
                defenseController.EndDefense();
                Debug.Log("CombatModeUI: 攻撃ボタン押下 - 防御解除");
            }
            
            combatModeController.PerformManualAttack();
        }
    }
    
    /// <summary>
    /// 防御ボタンクリック時の処理
    /// </summary>
    private void OnDefenseButtonClicked()
    {
        if (defenseController && CombatModeController.Instance && !CombatModeController.Instance.IsAutoMode)
        {
            defenseController.StartDefense();
            Debug.Log("CombatModeUI: 防御ボタン押下 - 防御開始");
        }
    }
    
    /// <summary>
    /// パリィボタンクリック時の処理
    /// </summary>
    private void OnParryButtonClicked()
    {
        if (defenseController)
        {
            // 防御中なら防御を解除
            if (defenseController.CurrentState == DefenseController.DefenseState.Defending)
            {
                defenseController.EndDefense();
                Debug.Log("CombatModeUI: パリィボタン押下 - 防御解除");
            }
            
            defenseController.HandleParryInput();
            Debug.Log("CombatModeUI: パリィボタン押下 - パリィ実行");
        }
    }
    
    /// <summary>
    /// 攻撃モード変更時の処理
    /// </summary>
    /// <param name="isAutoMode">true: オートモード, false: マニュアルモード</param>
    private void OnModeChanged(bool isAutoMode)
    {
        // パネルの表示切り替え
        if (autoModePanel) autoModePanel.SetActive(isAutoMode);
        if (manualModePanel) manualModePanel.SetActive(!isAutoMode);
        
        // 攻撃ゲージUIの表示切り替え
        if (attackGaugeUI) attackGaugeUI.gameObject.SetActive(isAutoMode);
        
        // マニュアル攻撃・防御ボタンの有効化
        UpdateManualButtons(!isAutoMode);
        
        // モード切り替えボタンのテキスト更新
        if (modeToggleText)
        {
            modeToggleText.text = isAutoMode ? "マニュアル" : "オート";
        }
        
        Debug.Log($"CombatModeUI: UI更新 - {(isAutoMode ? "オートモード" : "マニュアルモード")}");
    }
    
    /// <summary>
    /// マニュアルモードボタンの有効・無効を更新
    /// </summary>
    /// <param name="enabled">ボタンを有効にするか</param>
    private void UpdateManualButtons(bool enabled)
    {
        // 攻撃ボタン
        if (manualAttackButton)
        {
            manualAttackButton.interactable = enabled;
            var attackImage = manualAttackButton.GetComponent<Image>();
            if (attackImage)
                attackImage.color = enabled ? enabledColor : disabledColor;
        }
        
        // 防御ボタン
        if (defenseButton)
        {
            defenseButton.interactable = enabled;
            var defenseImage = defenseButton.GetComponent<Image>();
            if (defenseImage)
                defenseImage.color = enabled ? enabledColor : disabledColor;
        }
    }
    
    /// <summary>
    /// 防御ボタンの状態を更新
    /// </summary>
    private void UpdateDefenseButtonState(DefenseButtonState newState)
    {
        if (defenseButton == null) return;
        
        currentDefenseButtonState = newState;
        ApplyDefenseButtonColor(newState);
    }
    
    /// <summary>
    /// 防御ボタンの色を適用
    /// </summary>
    private void ApplyDefenseButtonColor(DefenseButtonState state)
    {
        if (defenseButton == null) return;
        
        var defenseImage = defenseButton.GetComponent<Image>();
        if (defenseImage == null) return;
        
        switch (state)
        {
            case DefenseButtonState.Normal:
                defenseImage.color = enabledColor;
                break;
            case DefenseButtonState.Defending:
                defenseImage.color = defendingColor;
                break;
            case DefenseButtonState.ParrySuccess:
                defenseImage.color = parrySuccessColor;
                break;
            case DefenseButtonState.Cooldown:
                defenseImage.color = cooldownColor;
                break;
        }
    }
    
    /// <summary>
    /// パリィボタンの状態を更新
    /// </summary>
    /// <param name="newState">新しい状態</param>
    private void UpdateParryButtonState(DefenseButtonState newState)
    {
        if (currentParryButtonState == newState) return;
        
        currentParryButtonState = newState;
        ApplyParryButtonColor(newState);
        
        if (showDebugInfo)
            Debug.Log($"CombatModeUI: パリィボタン状態変更 -> {newState}");
    }
    
    /// <summary>
    /// パリィボタンの色を適用
    /// </summary>
    /// <param name="state">状態</param>
    private void ApplyParryButtonColor(DefenseButtonState state)
    {
        if (!parryButton) return;
        
        var image = parryButton.GetComponent<Image>();
        if (!image) return;
        
        switch (state)
        {
            case DefenseButtonState.Normal:
                image.color = Color.white;
                break;
            case DefenseButtonState.Defending:
                image.color = parryWindowColor; // パリィウィンドウ中は赤色
                break;
            case DefenseButtonState.ParrySuccess:
                image.color = parrySuccessColor;
                break;
            case DefenseButtonState.Cooldown:
                image.color = cooldownColor;
                break;
        }
    }
    
    /// <summary>
    /// 防御開始時の処理
    /// </summary>
    private void OnDefenseStart()
    {
        UpdateDefenseButtonState(DefenseButtonState.Defending);
        UpdateParryButtonState(DefenseButtonState.Normal);
    }
    
    /// <summary>
    /// 防御終了時の処理
    /// </summary>
    private void OnDefenseEnd()
    {
        UpdateDefenseButtonState(DefenseButtonState.Normal);
        UpdateParryButtonState(DefenseButtonState.Normal);
    }
    
    /// <summary>
    /// パリィ成功時の処理
    /// </summary>
    private void OnParrySuccess()
    {
        UpdateDefenseButtonState(DefenseButtonState.ParrySuccess);
        UpdateParryButtonState(DefenseButtonState.ParrySuccess);
    }
    
    /// <summary>
    /// パリィ成功エフェクト
    /// </summary>
    private System.Collections.IEnumerator ParrySuccessEffect()
    {
        var defenseImage = defenseButton.GetComponent<Image>();
        if (defenseImage)
        {
            Color originalColor = defenseImage.color;
            
            // 黄色に点滅
            for (int i = 0; i < 3; i++)
            {
                defenseImage.color = parrySuccessColor;
                yield return new WaitForSeconds(0.1f);
                defenseImage.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
            
            // エフェクト終了後、通常状態に戻す
            UpdateDefenseButtonState(DefenseButtonState.Normal);
        }
    }
    
    private void Update()
    {
        // 防御ボタンのクールダウン表示
        if (defenseController)
        {
            if (defenseController.IsInDefenseCooldown)
            {
                // パリィ成功中でない場合のみクールダウン色を適用
                if (currentDefenseButtonState != DefenseButtonState.ParrySuccess)
                {
                    UpdateDefenseButtonState(DefenseButtonState.Cooldown);
                }
                if (currentParryButtonState != DefenseButtonState.ParrySuccess)
                {
                    UpdateParryButtonState(DefenseButtonState.Cooldown);
                }
            }
            else if (defenseController.IsInParryCooldown)
            {
                // パリィクールダウン中
                if (currentParryButtonState != DefenseButtonState.ParrySuccess)
                {
                    UpdateParryButtonState(DefenseButtonState.Cooldown);
                }
            }
            else if (currentDefenseButtonState == DefenseButtonState.Cooldown || currentParryButtonState == DefenseButtonState.Cooldown)
            {
                // クールダウン終了時は通常状態に戻す
                UpdateDefenseButtonState(DefenseButtonState.Normal);
                UpdateParryButtonState(DefenseButtonState.Normal);
            }
            
            // パリィウィンドウ中の状態管理
            if (defenseController.CurrentState == DefenseController.DefenseState.ParryWindow)
            {
                UpdateParryButtonState(DefenseButtonState.Defending); // パリィウィンドウ中は赤色
            }
        }
    }
    
    private void OnDestroy()
    {
        // イベントの購読解除
        if (combatModeController)
            combatModeController.OnModeChanged.RemoveListener(OnModeChanged);
        
        if (defenseController)
        {
            defenseController.OnDefenseStart.RemoveListener(OnDefenseStart);
            defenseController.OnDefenseEnd.RemoveListener(OnDefenseEnd);
            defenseController.OnParrySuccess.RemoveListener(OnParrySuccess);
        }
    }
} 