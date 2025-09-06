using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using static EnhancePatch;
using AudioSystem;

[System.Serializable]
public struct WeaponEnhanceValues
{
    public float attack;
    public float defense;
    public float magicAttack;
    public float magicDefense;
    public float speed;
    public float fate;
    
    public WeaponEnhanceValues(float attack, float defense, float magicAttack, float magicDefense, float speed, float fate)
    {
        this.attack = attack;
        this.defense = defense;
        this.magicAttack = magicAttack;
        this.magicDefense = magicDefense;
        this.speed = speed;
        this.fate = fate;
    }
}

public class UIManager : MonoBehaviour
{
    [Header("UIパネル")]
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private GameObject statAllocationPanel; // ステータス割り振りパネル
    [SerializeField] private GameObject openButton;
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject playerControlObject;

    [Header("プレイヤー関連")]
    [SerializeField] private PlayerStats playerStats; // Inspectorで必ずセット

    [Header("ステータステキスト")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI magicAttackText;
    [SerializeField] private TextMeshProUGUI magicDefenseText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI fateText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statPointsText;
    [SerializeField] private TextMeshProUGUI statPointsAllocationText; // ステータス割り振り用

    [Header("ステータス割り振りパネル用UI")]
    [SerializeField] private Button showStatAllocationPanelButton; // 割り振りパネルを開くボタン
    [SerializeField] private Button returnToStatusPanelButton;    // ステータスパネルに戻るボタン
    [SerializeField] private Button confirmStatAllocationButton;  // 割り振り確定ボタン

    [Header("ステータス値表示 (割り振り用)")]
    [SerializeField] private TextMeshProUGUI[] statValueTexts; // 各ステータス値テキスト

    [Header("ステータス割り振りボタン")]
    [SerializeField] private Button[] statIncreaseButtons; // 増加ボタン
    [SerializeField] private Button[] statDecreaseButtons; // 減少ボタン

    [Header("HP・EXP表示")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private TextMeshProUGUI hpValueText;
    [SerializeField] private Slider xpBar;
    [SerializeField] private TextMeshProUGUI xpText;

    [Header("アーティファクトパネル")]
    [SerializeField] private GameObject artifactPanel;
    [SerializeField] private Button showArtifactPanelButton;
    [SerializeField] private Button closeArtifactPanelButton;
    
    [Header("軽量アーティファクト表示（Phase 2B）")]
    [SerializeField] private TextMeshProUGUI artifactListText; // 軽量なテキスト表示用
    [SerializeField] private TextMeshProUGUI artifactCountText; // 総数表示用

    [Header("武器パネル")]
    [SerializeField] private GameObject weaponSelectionPanel;
    [SerializeField] private Transform weaponListContent;
    [SerializeField] private GameObject weaponItemDisplayPrefab;
    [SerializeField] private List<WeaponItem> allWeapons;

    [Header("武器装備ボタン")]
    [SerializeField] private Image weaponEquipIcon;
    [SerializeField] private Button weaponEquipButton;

    [Header("現在の装備")]
    [SerializeField] private Image equippedWeaponIcon;
    [SerializeField] private TextMeshProUGUI equippedWeaponName;
    [SerializeField] private TextMeshProUGUI equippedWeaponDescription;
    [SerializeField] private TextMeshProUGUI equippedWeaponEnhancementLevel; // 強化値表示用（例: "+3"）
    [SerializeField] private TextMeshProUGUI statusPanelWeaponEnhancementText; // ステータスパネル用武器強化値表示

    [Header("パッチパネル")]
    [SerializeField] private GameObject patchSelectionPanel;
    [SerializeField] private Transform patchListContent;
    [SerializeField] private GameObject patchItemDisplayPrefab;

    [Header("パッチスロットパネル")]
    [SerializeField] private GameObject patchSlotPrefab;
    [SerializeField] private Transform patchSlotContainer;

    [Header("現在装備中パッチスロットパネル")]
    [SerializeField] private GameObject currentPatchSlotPrefab;
    [SerializeField] private Transform currentPatchSlotContainer;
    private List<GameObject> currentPatchSlotObjects = new List<GameObject>();

    [SerializeField] private EnhancePatch nonePatch; // InspectorでNonePatchをセット

    [Header("スキルパネル")]
    [SerializeField] private GameObject skillSelectionPanel;
    [SerializeField] private Transform skillListContent;
    [SerializeField] private GameObject skillItemDisplayPrefab;
    [SerializeField] private List<SkillData> allSkills; // Inspectorでセット
    [SerializeField] private Transform currentSkillSlotContainer;
    [SerializeField] private GameObject currentSkillSlotPrefab;
    [SerializeField] private GameObject statusOpenButton; // Inspectorでアサイン
    private List<GameObject> currentSkillSlotObjects = new List<GameObject>();
    private int selectedCurrentSkillSlotIndex = -1;
    private List<SkillItemDisplay> skillDisplays = new List<SkillItemDisplay>();
    private SkillData[] equippedSkills = new SkillData[3];

    [SerializeField] private GameObject skillOpenButton; // スキル開くボタン
    [SerializeField] private GameObject[] skillActivateButtons; // スキル発動ボタン（3つ）

    private bool isPanelOpen = false;
    private bool isPatchPanelOpen = false;
    private List<WeaponItemDisplay> weaponDisplays = new List<WeaponItemDisplay>();
    private List<PatchItemDisplay> patchDisplays = new List<PatchItemDisplay>();
    private List<GameObject> patchSlotObjects = new List<GameObject>();
    private int selectedPatchSlotIndex = -1;
    private List<EnhancePatch> patches = new List<EnhancePatch>();
    private int selectedCurrentPatchSlotIndex = -1; // 現在選択中のパッチスロットのインデックス

    // ステータス割り振りパネル用の一時的なステータスポイント
    private int[] tempStatPoints = new int[6]; // ATK, DEF, MATK, MDEF, SPD, FATE
    private int remainingPoints = 0;

    private ArtifactInventory artifactInventory; // イベント解除用に参照保持

    [Header("常時表示ボタン（Inspectorで登録）")]
    public List<Button> managedButtons; // AF/マテリアル/ステータス/他のボタンを全て登録

    [Header("パネル開閉システム")]
    [SerializeField] private GameObject[] openPanelButtons; // 開く系ボタンのリスト（インスペクターで登録）
    [Tooltip("パネルが開かれた時に隠すボタンを登録してください")]
    
    [Header("ボタンレイアウト管理")]
    [SerializeField] private Transform buttonContainer; // ボタンコンテナ（ホリゾンタルレイアウト用）
    [SerializeField] private HorizontalLayoutGroup buttonLayoutGroup; // レイアウトグループの参照

    [Header("マテリアルパネル用ボタン")]
    [SerializeField] private Button openMaterialPanelButton; // Inspectorで登録

    [Header("オプションメニュー")]
    [SerializeField] private Button openOptionsButton; // オプションメニューを開くボタン
    [SerializeField] private OptionsMenu optionsMenu; // オプションメニューコンポーネント

    // === キャッシュ化された参照（フリーズ対策） ===
    private SkillController cachedSkillController;
    private MaterialUIManager cachedMaterialUIManager;

    private bool hasWarnedAboutMissingPlayer = false;

    private float updateTimer = 0f;
    private const float UPDATE_INTERVAL = 0.1f; // 0.1秒間隔でUI更新

    private bool hasWarnedInLateUpdate = false;

    private void Awake()
    {
        // Unity 6フリーズ対策：システム全体を軽量化モードで実行中
        Debug.Log("=== Unity 6 フリーズ対策モード ===");
        Debug.Log("多くの機能が一時的に無効化されています。");
        Debug.Log("これは正常な動作で、フリーズを防ぐための措置です。");
        Debug.Log("==============================");
        
        ValidateComponents();
        InitStatAllocationPanel();

        // ArtifactInventoryの参照取得（Instanceプロパティを優先）
        if (artifactInventory == null)
        {
            artifactInventory = ArtifactInventory.Instance;
            if (artifactInventory == null)
            {
                artifactInventory = Object.FindFirstObjectByType<ArtifactInventory>();
                if (artifactInventory != null)
                {
                    // Debug.LogWarning("UIManager: artifactInventoryをFindObjectOfTypeで自動取得しました");
                }
                else
                {
                    // Debug.LogWarning("UIManager: ArtifactInventoryがAwakeで見つかりませんでした。Startで再試行します。");
                }
            }
            else
            {
                // Debug.Log("UIManager: ArtifactInventory.Instanceから参照を取得しました");
            }
        }
    }

    private void Start()
    {
        Debug.Log("UIManager: フリーズ対策版 - 完全機能復活を開始");
        
        try
        {
            // 段階的な初期化
            InitializeBasicUI();
            
            // コルーチンで重い処理を分散
            StartCoroutine(InitializeUIComponentsAsync());
            
            Debug.Log("UIManager: 基本初期化完了、非同期初期化開始");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: 初期化エラー - {e.Message}");
        }
    }
    
    private void InitializeBasicUI()
    {
        Debug.Log("UIManager: 基本UI要素の初期化を開始");
        
        // PlayerStatsの参照確認
        if (playerStats == null)
        {
            playerStats = PlayerStats.Instance != null ? PlayerStats.Instance : Object.FindFirstObjectByType<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogWarning("UIManager: PlayerStatsが見つかりませんでした");
                return;
            }
        }
        
        // ArtifactInventoryの参照確認
        if (artifactInventory == null)
        {
            artifactInventory = ArtifactInventory.Instance;
            if (artifactInventory == null)
            {
                Debug.LogWarning("UIManager: ArtifactInventoryが見つかりませんでした");
            }
        }
        
        // WeaponManagerの初期化
        if (playerStats != null && nonePatch != null)
        {
            WeaponManager.NonePatchRef = nonePatch;
        }
        else
        {
            Debug.LogError("UIManager: 【重要】NonePatchが設定されていません！Inspectorで設定してください。");
        }
        
        // MaterialUIManagerのキャッシュ
        if (cachedMaterialUIManager == null)
        {
            cachedMaterialUIManager = FindObjectOfType<MaterialUIManager>();
        }
        
        // SkillControllerのキャッシュ
        if (cachedSkillController == null)
        {
            cachedSkillController = FindObjectOfType<SkillController>();
        }
        
        // 基本的なUI表示を更新
        UpdateStatusTexts();
        UpdateCurrentEquipDisplay();
    }

    private void OnDestroy()
    {
        if (artifactInventory != null)
        {
            artifactInventory.OnInventoryChanged -= OnInventoryChangedHandler;
        }
    }

    private void OnInventoryChangedHandler()
    {
        if (playerStats != null)
        {
            playerStats.ApplyAllEffects();
            UpdateStatusTexts();
            UpdateCurrentEquipDisplay();
            UpdateStatPointButtons();
        }
    }

    // 必須コンポーネントの検証
    private void ValidateComponents()
    {
        if (statusPanel == null) { /* Debug.LogError("UIManager: statusPanel が設定されていません"); */ }
        if (statAllocationPanel == null) { /* Debug.LogError("UIManager: statAllocationPanel が設定されていません"); */ }
        if (openButton == null) { /* Debug.LogError("UIManager: openButton が設定されていません"); */ }
        if (closeButton == null) { /* Debug.LogError("UIManager: closeButton が設定されていません"); */ }
        if (playerStats == null) { /* Debug.LogError("UIManager: playerStats が設定されていません"); */ }
    }

    // ステータス割り振りパネルの初期化
    private void InitStatAllocationPanel()
    {
        if (tempStatPoints == null)
            tempStatPoints = new int[6];
        
        for (int i = 0; i < tempStatPoints.Length; i++)
        {
            tempStatPoints[i] = 0;
        }
    }

    private void Update()
    {
        // フリーズ対策：UI更新頻度を下げる
        updateTimer += Time.unscaledDeltaTime;
        if (updateTimer < UPDATE_INTERVAL)
            return;
        
        updateTimer = 0f;
        
        // 安全なnullチェック（警告抑制）
        if (playerStats == null || playerStats.stats == null)
        {
            return; // 警告を出さずに静かに終了
        }

        // HP/EXPなどのUIを定期的に更新
        try
        {
            UpdateTopUI();
            UpdateStatusTexts();
            UpdateStatPointButtons();
        }
        catch (System.Exception)
        {
            // 例外が発生した場合も静かに処理
            return;
        }
    }

    // ステータスパネルを開いたときの処理
    public void OnOpenButton()
    {
        if (playerStats == null)
        {
            // Debug.LogError("UIManager: OnOpenButton で playerStats が null - Inspectorで設定してください");
            return;
        }
        if (playerStats.stats == null)
        {
            // Debug.LogError("UIManager: OnOpenButton で stats が null");
            return;
        }
        
        // 統一パネル開閉システムを使用
        SetUIState(true);
        
        UpdateStatusTexts();
        UpdateStatPointButtons();
        UpdateCurrentEquipDisplay();
        RefreshStatusPatchSlots();
        if (openMaterialPanelButton != null)
            openMaterialPanelButton.gameObject.SetActive(false);
    }

    // ステータスパネルを閉じたときの処理
    public void OnCloseButton()
    {
        Debug.Log("UIManager: 統合閉じるボタン機能 - 直感的な動作版");
        
        try
        {
            // アーティファクトパネルが開いている場合
            if (artifactPanel != null && artifactPanel.activeSelf)
            {
                CloseArtifactPanel();
                return;
            }
            
            // ステータス割り振りパネルが開いている場合
            if (statAllocationPanel != null && statAllocationPanel.activeSelf)
            {
                ReturnToStatusPanel();
                return;
            }
            
            // スキル選択パネルが開いている場合
            if (skillSelectionPanel != null && skillSelectionPanel.activeSelf)
            {
                CloseSkillSelectionPanel();
                return;
            }
            
            // パッチ選択パネルが開いている場合
            if (patchSelectionPanel != null && patchSelectionPanel.activeSelf)
            {
                ClosePatchSelectionPanel();
                return;
            }
            
            // 武器選択パネルが開いている場合
            if (weaponSelectionPanel != null && weaponSelectionPanel.activeSelf)
            {
                CloseWeaponSelectionPanel();
                return;
            }
            
            // マテリアル詳細パネルが開いている場合（優先）
            if (cachedMaterialUIManager != null && cachedMaterialUIManager.itemDetailPanel != null && cachedMaterialUIManager.itemDetailPanel.activeSelf)
            {
                cachedMaterialUIManager.CloseDetailPanel();
                return;
            }
            
            // マテリアルパネルが開いている場合
            if (cachedMaterialUIManager != null && cachedMaterialUIManager.materialPanel != null && cachedMaterialUIManager.materialPanel.activeSelf)
            {
                cachedMaterialUIManager.CloseMaterialPanel();
                return;
            }
            
            // 通常のステータスパネルが開いている場合
            if (statusPanel != null && statusPanel.activeSelf)
            {
                Debug.Log("UIManager: ステータスパネルを閉じました");
                SetUIState(false);
                return;
            }
            
            Debug.LogWarning("UIManager: 閉じるべきパネルが見つかりませんでした");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: OnCloseButton エラー - {e.Message}");
        }
    }

    // UI表示の状態を設定
    private void SetUIState(bool open)
    {
        Debug.Log($"UIManager: SetUIState({open}) - パネル開閉システム統合版");
        
        // パッチパネルが開いている場合は処理を中断
        if (isPatchPanelOpen)
        {
            Debug.Log("パッチパネルが開いているためステータスパネルを閉じました");
            return;
        }

        // パッチパネルが閉じている場合のみ処理を行う
        isPanelOpen = open;
        Time.timeScale = open ? 0f : 1f;

        if (statusPanel != null)
        {
            statusPanel.SetActive(open);
        }

        if (openButton != null)
        {
            openButton.SetActive(!open);
        }

        if (closeButton != null)
        {
            closeButton.SetActive(open);
        }

        if (playerControlObject != null)
        {
            var movement = playerControlObject.GetComponent<CharacterMovement>();
            if (movement != null)
            {
                movement.EnableMovement(!open);
            }
        }
        
        // 統一パネル開閉システムを呼び出し
        if (open)
        {
            OnPanelOpened();
        }
        else
        {
            OnPanelClosed();
        }
    }

    // 現在装備中の武器の強化値を取得
    private WeaponEnhanceValues GetCurrentWeaponEnhanceValues()
    {
        // デフォルト値（強化なし）
        var defaultValues = new WeaponEnhanceValues(0, 0, 0, 0, 0, 0);
        
        if (playerStats == null)
            return defaultValues;
            
        var weaponManager = playerStats.GetComponent<WeaponManager>();
        if (weaponManager == null || weaponManager.currentWeapon == null || weaponManager.currentWeapon.weaponItem == null)
            return defaultValues;
            
        var currentWeaponItem = weaponManager.currentWeapon.weaponItem;
        
        // WeaponEnhanceProcessorから強化レベルを取得
        var enhanceProcessor = FindObjectOfType<WeaponEnhanceProcessor>();
        if (enhanceProcessor == null)
            return defaultValues;
            
        int enhanceLevel = enhanceProcessor.GetWeaponEnhanceLevel(currentWeaponItem);
        if (enhanceLevel <= 0)
            return defaultValues;
            
        // 武器の強化値 × 強化レベルで計算
        return new WeaponEnhanceValues(
            currentWeaponItem.enhanceAttack * enhanceLevel,
            currentWeaponItem.enhanceDefense * enhanceLevel,
            currentWeaponItem.enhanceMagicAttack * enhanceLevel,
            currentWeaponItem.enhanceMagicDefense * enhanceLevel,
            currentWeaponItem.enhanceSpeed * enhanceLevel,
            currentWeaponItem.enhanceFate * enhanceLevel
        );
    }

    // ステータス値表示の更新（武器強化値込みの総計）
    private void UpdateStatusTexts()
    {
        if (playerStats == null || playerStats.stats == null)
        {
            // Debug.LogError("UIManager: playerStats または stats が null です");
            return;
        }
        var stats = playerStats.stats;
        try
        {
            if (levelText != null) levelText.text = $"{playerStats.Level}";
            if (statPointsText != null) statPointsText.text = $"ステータスポイント: {playerStats.StatPoints}";
            if (statPointsAllocationText != null) statPointsAllocationText.text = $"ステータスポイント: {remainingPoints}";
            if (hpText != null) hpText.text = $"HP: {playerStats.CurrentHP} / {Mathf.RoundToInt(stats.TotalHP)}";
            
            // 武器強化値込みの総計値を表示
            var weaponEnhanceValues = GetCurrentWeaponEnhanceValues();
            
            if (attackText != null) attackText.text = $"ATK: {stats.TotalAttack + weaponEnhanceValues.attack:F1}";
            if (defenseText != null) defenseText.text = $"DEF: {stats.TotalDefense + weaponEnhanceValues.defense:F1}";
            if (magicAttackText != null) magicAttackText.text = $"MATK: {stats.TotalMagicAttack + weaponEnhanceValues.magicAttack:F1}";
            if (magicDefenseText != null) magicDefenseText.text = $"MDEF: {stats.TotalMagicDefense + weaponEnhanceValues.magicDefense:F1}";
            if (speedText != null) speedText.text = $"SPD: {stats.Speedpoint + weaponEnhanceValues.speed:F1}";
            if (fateText != null) fateText.text = $"FATE: {stats.Fatepoint + weaponEnhanceValues.fate:F1}";
            
            UpdateStatAllocationTexts();
        }
        catch (System.Exception e)
        {
            // Debug.LogError($"UIManager: ステータス値表示の更新にエラーが発生しました: {e.Message}");
        }
    }

    // ステータス割り振りの表示（基本値 + ステータスポイント分のみ）
    private void UpdateStatAllocationTexts()
    {
        if (statValueTexts == null || statValueTexts.Length < 6 || playerStats == null || playerStats.stats == null)
            return;

        var stats = playerStats.stats;
        
        // 基本値 + ステータスポイント分のみを表示（装備品・アーティファクトを除く）
        statValueTexts[0].text = $"ATK: {stats.baseAttack + stats.additionalAttack + tempStatPoints[0]:F1}";
        statValueTexts[1].text = $"DEF: {stats.baseDefense + stats.additionalDefense + tempStatPoints[1]:F1}";
        statValueTexts[2].text = $"MATK: {stats.baseMagicAttack + stats.additionalMagicAttack + tempStatPoints[2]:F1}";
        statValueTexts[3].text = $"MDEF: {stats.baseMagicDefense + stats.additionalMagicDefense + tempStatPoints[3]:F1}";
        statValueTexts[4].text = $"SPD: {stats.baseSpeed + stats.additionalSpeed + tempStatPoints[4]:F1}";
        statValueTexts[5].text = $"FATE: {stats.baseFate + stats.additionalFate + tempStatPoints[5]:F1}";
    }

    // ステータス割り振りボタンの更新
    private void UpdateStatPointButtons()
    {
        // 基本のステータス割り振りボタン
        if (statIncreaseButtons != null && statIncreaseButtons.Length > 0)
        {
            bool canAllocate = playerStats.StatPoints > 0;
            foreach (var button in statIncreaseButtons)
            {
                if (button != null)
                    button.interactable = canAllocate;
            }
        }

        // ステータス割り振りのボタンの更新
        UpdateStatAllocationButtons();
    }

    // ステータス割り振りのボタンの更新
    private void UpdateStatAllocationButtons()
    {
        if (statDecreaseButtons == null || statIncreaseButtons == null || 
            statDecreaseButtons.Length < 6 || statIncreaseButtons.Length < 6)
            return;

        // 増加ボタンはステータスポイントがある場合のみ表示
        for (int i = 0; i < 6; i++)
        {
            statIncreaseButtons[i].interactable = remainingPoints > 0;
        }

        // 減少ボタンは一時的なステータス割り振りがある場合のみ表示
        for (int i = 0; i < 6; i++)
        {
            statDecreaseButtons[i].interactable = tempStatPoints[i] > 0;
        }

        // 割り振り確定ボタンはステータス割り振りがある場合のみ表示
        if (confirmStatAllocationButton != null)
        {
            bool anyPointsAllocated = false;
            for (int i = 0; i < 6; i++)
            {
                if (tempStatPoints[i] > 0)
                {
                    anyPointsAllocated = true;
                    break;
                }
            }
            confirmStatAllocationButton.interactable = anyPointsAllocated;
        }
    }

    // ステータス割り振りのボタンを押したときの処理
    public void OnAllocateStatPointButton(int statTypeIndex)
    {
        if (playerStats.StatPoints <= 0)
            return;

        StatType statType = (StatType)statTypeIndex;
        if (playerStats.AllocateStatPoint(statType))
        {
            UpdateStatusTexts();
            UpdateStatPointButtons();
        }
    }

    // ステータス割り振りパネルを開く
    public void ShowStatAllocationPanel()
    {
        // 統一パネル開閉システム
        OnPanelOpened();
        
        // 一時的なステータスポイントを初期化
        InitStatAllocationPanel();
        remainingPoints = playerStats.StatPoints;
        statusPanel.SetActive(false);
        statAllocationPanel.SetActive(true);
        UpdateStatAllocationTexts();
        UpdateStatAllocationButtons();
    }

    // ステータスパネルに戻る
    public void ReturnToStatusPanel()
    {
        Debug.Log("UIManager: ステータス割り振りパネルからメインパネルに戻る");
        
        // 一時的なステータスポイントを初期化
        InitStatAllocationPanel();
        
        if (statAllocationPanel != null)
            statAllocationPanel.SetActive(false);
        if (statusPanel != null)
            statusPanel.SetActive(true);
        
        // 統一パネル開閉システム（閉じる処理）
        OnPanelClosed();
    }

    
    // ステータス割り振りの値を増加
    public void IncreaseStatPoint(int statIndex)
    {
        if (remainingPoints <= 0 || statIndex < 0 || statIndex >= 6)
            return;
            
        tempStatPoints[statIndex]++;
        remainingPoints--;
        
        UpdateStatAllocationTexts();
        UpdateStatAllocationButtons();
    }
    
    // ステータス割り振りの値を減少
    public void DecreaseStatPoint(int statIndex)
    {
        if (tempStatPoints[statIndex] <= 0 || statIndex < 0 || statIndex >= 6)
            return;
            
        tempStatPoints[statIndex]--;
        remainingPoints++;
        
        UpdateStatAllocationTexts();
        UpdateStatAllocationButtons();
    }
    
    // ステータス割り振りの確定
    public void ConfirmStatAllocation()
    {
        // ステータス割り振りを反映
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < tempStatPoints[i]; j++)
            {
                playerStats.AllocateStatPoint((StatType)(i + 1)); // HPのためにC#の配列+1
            }
        }
        
        // ステータスパネルに戻る
        ReturnToStatusPanel();
        
        // 基本のステータス表示を更新
        UpdateStatusTexts();
        UpdateStatPointButtons();
    }

    public void ShowArtifactPanel()
    {
        Debug.Log("UIManager: アーティファクトパネル開く - Phase 2B軽量表示テスト");
        
        try
        {
            // 統一パネル開閉システム
            OnPanelOpened();
            
            if (playerStats != null) playerStats.ApplyAllEffects();
            if (statusPanel != null) statusPanel.SetActive(false);
            if (statAllocationPanel != null) statAllocationPanel.SetActive(false);
            
            if (closeArtifactPanelButton != null)
                closeArtifactPanelButton.gameObject.SetActive(true);
            if (artifactPanel != null) artifactPanel.SetActive(true);
            if (playerControlObject != null) playerControlObject.SetActive(false);
            
            var movement = playerControlObject != null ? playerControlObject.GetComponent<CharacterMovement>() : null;
            if (movement != null) movement.EnableMovement(false);
            
            Time.timeScale = 0f;
            
            // Phase 2B: 軽量なアーティファクト情報表示
            RefreshArtifactDisplayLightweight();
            
            Debug.Log("UIManager: アーティファクトパネル開く完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: ShowArtifactPanel エラー - {e.Message}");
            // エラー時は安全にパネルを閉じる
            CloseArtifactPanel();
        }
    }

    // Phase 2B軽量版：アーティファクト表示を軽量化してより良いUIに
    private void RefreshArtifactDisplayLightweight()
    {
        Debug.Log("Unity 6フリーズ対策: 軽量アーティファクト表示更新");
        
        if (artifactInventory == null)
        {
            Debug.LogWarning("UIManager: ArtifactInventory が null です");
            return;
        }

        try
        {
            // アーティファクト一覧を取得
            var allArtifacts = artifactInventory.GetAllOwnedArtifacts();
            
            // 軽量版表示内容を作成
            if (artifactListText != null)
            {
                var displayContent = new System.Text.StringBuilder();
                displayContent.AppendLine("【所持アーティファクト一覧】");
                
                if (allArtifacts.Count == 0)
                {
                    displayContent.AppendLine("アーティファクトを所持していません");
                }
                else
                {
                    // アーティファクトを直接表示（カテゴリー分けなし）
                    foreach (var artifact in allArtifacts)
                    {
                        int count = artifactInventory.GetArtifactCount(artifact);
                        var effectText = artifact.isMultiplier ? "×" : "+";
                        displayContent.AppendLine($"  • {artifact.artifactName} [{count}個]");
                        displayContent.AppendLine($"    効果: {artifact.affectedStat} {effectText}{artifact.effectValue}");
                    }
                }
                
                // 消費アイテム一覧も軽量表示
                var consumables = artifactInventory.GetAllOwnedConsumables();
                if (consumables.Count > 0)
                {
                    displayContent.AppendLine("\n【所持消費アイテム一覧】");
                    foreach (var item in consumables)
                    {
                        int count = artifactInventory.GetConsumableCount(item);
                        displayContent.AppendLine($"  • {item.ItemName} [{count}個]");
                        displayContent.AppendLine($"    効果: {item.Description}");
                    }
                }
                
                artifactListText.text = displayContent.ToString();
            }
            
            // 総数表示
            if (artifactCountText != null)
            {
                var consumables = artifactInventory.GetAllOwnedConsumables();
                artifactCountText.text = $"AF: {allArtifacts.Count}種類 | 消費: {consumables.Count}種類";
            }
            
            Debug.Log($"軽量AF表示更新完了: AF={allArtifacts.Count}, 消費={artifactInventory.GetAllOwnedConsumables().Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: 軽量AF表示更新エラー - {e.Message}");
            if (artifactListText != null)
            {
                artifactListText.text = "アーティファクト表示エラー";
            }
        }
    }

    // アーティファクトパネルを閉じる
    public void CloseArtifactPanel()
    {
        Debug.Log("UIManager: アーティファクトパネル閉じる - Phase 2A復活テスト");
        
        try
        {
            // 統一パネル開閉システム
            OnPanelClosed();
            
            if (artifactPanel != null) artifactPanel.SetActive(false);
            
            if (closeArtifactPanelButton != null)
                closeArtifactPanelButton.gameObject.SetActive(false);
            if (playerControlObject != null) playerControlObject.SetActive(true);
            
            var movement = playerControlObject != null ? playerControlObject.GetComponent<CharacterMovement>() : null;
            if (movement != null) movement.EnableMovement(true);
            
            Time.timeScale = 1f;
            
            Debug.Log("UIManager: アーティファクトパネル閉じる完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: CloseArtifactPanel エラー - {e.Message}");
            // 緊急復旧
            Time.timeScale = 1f;
            if (playerControlObject != null) playerControlObject.SetActive(true);
        }
    }

    // 武器選択パネルを表示
    public void ShowWeaponSelectionPanel()
    {
        Debug.Log("🚀 [UIManager] ShowWeaponSelectionPanel開始");
        
        try
        {
            // 統一パネル開閉システム
            Debug.Log("📝 [UIManager] OnPanelOpened呼び出し");
            OnPanelOpened();
            
            // ステータスパネルは重ねて表示したままにするため、非表示にしない
            if (weaponSelectionPanel != null)
            {
                Debug.Log("✅ [UIManager] weaponSelectionPanelをアクティブ化");
                weaponSelectionPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("❌ [UIManager] weaponSelectionPanelがnullです！");
            }
            
            // 武器リストを安全に更新
            Debug.Log("🔄 [UIManager] PopulateWeaponListSafely呼び出し");
            PopulateWeaponListSafely();
            
            Debug.Log("🎉 [UIManager] ShowWeaponSelectionPanel完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"💥 [UIManager] 武器選択パネル表示エラー - {e.Message}");
        }
    }
    
    // 武器選択パネルを閉じる

    // 従来の動作：武器選択パネルを閉じてステータスパネルを開く
    public void CloseWeaponSelectionPanel()
    {
        try
        {
            if (weaponSelectionPanel != null)
                weaponSelectionPanel.SetActive(false);
            
            // 統一パネル開閉システム
            OnPanelClosed();
            
            // ステータスパネルが開いている場合は、開く系ボタンを再度隠す
            if (statusPanel != null && statusPanel.activeInHierarchy)
            {
                HideOpenPanelButtons();
                Debug.Log("UIManager: ステータスパネル表示中のため、開く系ボタンを再度隠しました");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: 武器選択パネル閉じるエラー - {e.Message}");
        }
    }
    
    // 武器リストを安全に更新
    private void PopulateWeaponListSafely()
    {
        if (weaponListContent == null || weaponItemDisplayPrefab == null)
        {
            Debug.LogError("UIManager: 武器リスト更新 - 必要な参照が不足");
            return;
        }
        
        try
        {
            // 既存の武器アイテムを安全にクリア
            ClearWeaponDisplaysSafely();
            
            // 武器リストを取得
            List<WeaponItem> weaponsToDisplay = new List<WeaponItem>();
            
            if (allWeapons != null && allWeapons.Count > 0)
            {
                weaponsToDisplay.AddRange(allWeapons);
            }
            
            // ArtifactInventoryから所持武器を取得
            if (artifactInventory != null)
            {
                var ownedWeapons = artifactInventory.GetAllOwnedWeapons();
                foreach (var weapon in ownedWeapons)
                {
                    if (weapon != null && !weaponsToDisplay.Contains(weapon))
                    {
                        weaponsToDisplay.Add(weapon);
                    }
                }
            }
            
            // 武器アイテムを1つずつ安全に生成
            foreach (var weapon in weaponsToDisplay)
            {
                if (weapon != null)
                {
                    CreateWeaponDisplaySafely(weapon);
                }
            }
            
            // 装備状態を更新
            UpdateEquipIcons();
            UpdateWeaponHighlights();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: 武器リスト更新エラー - {e.Message}");
        }
    }
    
    // 武器表示を安全に作成
    private void CreateWeaponDisplaySafely(WeaponItem weapon)
    {
        try
        {
            GameObject weaponObj = Instantiate(weaponItemDisplayPrefab, weaponListContent);
            if (weaponObj != null)
            {
                WeaponItemDisplay display = weaponObj.GetComponent<WeaponItemDisplay>();
                if (display != null)
                {
                    display.Setup(weapon, this, false); // 初期状態は非選択
                    
                    // 強化値表示を更新
                    display.RefreshEnhancementDisplay();
                    
                    weaponDisplays.Add(display);
                }
                else
                {
                    Debug.LogWarning($"UIManager: WeaponItemDisplayコンポーネントが見つかりません: {weapon.weaponName}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: 武器表示作成エラー ({weapon.weaponName}) - {e.Message}");
        }
    }
    
    // 武器表示を安全にクリア
    private void ClearWeaponDisplaysSafely()
    {
        try
        {
            foreach (var display in weaponDisplays)
            {
                if (display != null && display.gameObject != null)
                {
                    Destroy(display.gameObject);
                }
            }
            weaponDisplays.Clear();
            
            Debug.Log("UIManager: 武器表示クリア完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: 武器表示クリアエラー - {e.Message}");
        }
    }

    // A ステータス値表示の更新時に呼ばれる UpdateEquipIcons()
    private void UpdateEquipIcons()
    {
        // Debug.Log("UpdateEquipIcons が呼ばれました");

        if (playerStats == null) return;
        var weaponManager = playerStats.GetComponent<WeaponManager>();
        if (weaponManager == null) return;

        // 武器装備の表示
        if (weaponManager.currentWeapon != null && weaponEquipIcon != null)
        {
            weaponEquipIcon.sprite = weaponManager.currentWeapon.weaponItem.icon;  // ここではインスペクターで設定したものを使用
            // Debug.Log("現在の武器: " + weaponManager.currentWeapon.weaponItem.weaponName);  // ここではインスペクターで設定したものを使用
        }
    }
    private void UpdateWeaponHighlights()
    {
        var weaponManager = playerStats.GetComponent<WeaponManager>();

        foreach (var display in weaponDisplays)
        {
            if (display != null && display.weaponData != null && weaponManager != null && weaponManager.currentWeapon != null)
            {
                bool isSelected = display.weaponData == weaponManager.currentWeapon.weaponItem;
                display.SetSelected(isSelected);
            }
        }
    }

    // C ステータス割り振りボタンからの処理、タップしたら
    public void OnClickWeaponEquipButton()
    {
        Debug.Log("🔥 [UIManager] 武器装備ボタンがクリックされました！");
        ShowWeaponSelectionPanel();
    }

    public void OnWeaponSelected(WeaponItem selectedWeapon)
    {
        // Debug.Log("[UIManager] OnWeaponSelectedが呼ばれました: " + selectedWeapon.weaponName);
        var weaponManager = playerStats.GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.EquipWeapon(selectedWeapon);
            // Debug.Log("[UIManager] EquipWeaponが呼ばれました");
        }
        playerStats.ApplyAllEffects();
        // Debug.Log("[UIManager] ApplyAllEffectsが呼ばれました");
        if (weaponManager != null && weaponManager.currentWeapon != null)
        {
            // Debug.Log("[UIManager] 現在の武器: " + weaponManager.currentWeapon.weaponItem.weaponName);
            equippedWeaponIcon.sprite = weaponManager.currentWeapon.weaponItem.icon;
            equippedWeaponName.text = weaponManager.currentWeapon.weaponItem.weaponName;
            equippedWeaponDescription.text = GenerateCurrentWeaponEffectText(weaponManager.currentWeapon.weaponItem);
            
            // 強化値表示
            UpdateCurrentWeaponEnhancementDisplay(weaponManager.currentWeapon.weaponItem);
            
            // ステータスパネル用武器強化値表示
            UpdateStatusPanelWeaponEnhancement(weaponManager.currentWeapon.weaponItem);
        }
        else
        {
            // Debug.LogWarning("[UIManager] currentWeaponがnullです");
        }
        UpdateStatusTexts();
        UpdateEquipIcons();
        UpdateWeaponHighlights();
        
        // 全ての武器表示の強化値を更新
        RefreshAllWeaponEnhancementDisplays();
    }

    // パッチリスト生成（必ずArtifactInventory.Instanceを参照）
    private void PopulatePatchList()
    {
        // 安全なplayerStats参照
        if (playerStats == null)
            return;
            
        var patches = playerStats.GetAllOwnedPatches();
        if (patches == null)
            return;

        // Debug.Log("PopulatePatchList: パッチリスト生成開始");
        // NonePatchが含まれていなければ追加
        if (nonePatch != null && !patches.Contains(nonePatch))
            patches.Insert(0, nonePatch);
        // Debug.Log($"所持パッチ数: {patches.Count}");
        if (patchDisplays.Count < patches.Count)
        {
            for (int i = patchDisplays.Count; i < patches.Count; i++)
            {
                GameObject itemGO = Instantiate(patchItemDisplayPrefab, patchListContent);
                PatchItemDisplay display = itemGO.GetComponent<PatchItemDisplay>();
                patchDisplays.Add(display);
            }
        }
        for (int i = 0; i < patchDisplays.Count; i++)
        {
            if (i < patches.Count)
            {
                patchDisplays[i].gameObject.SetActive(true);
                patchDisplays[i].Setup(patches[i], this);
                // Debug.Log($"パッチ[{i}] {patches[i]?.name ?? "null"} を表示");
            }
            else
            {
                patchDisplays[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnPatchSelected(EnhancePatch patch)
    {
        var weaponManager = playerStats.GetComponent<WeaponManager>();
        if (weaponManager == null) return;

        // NonePatchは多重装備可、それ以外は多重装備不可
        if (patch != nonePatch && System.Array.Exists(weaponManager.enhancePatches, p => p == patch))
        {
            // Debug.LogWarning("同じパッチは複数スロットに装備できません");
            return;
        }

        weaponManager.enhancePatches[selectedPatchSlotIndex] = patch;
        playerStats.ApplyAllEffects();
        RefreshStatusPatchSlots();
        UpdateStatusTexts();
        UpdateEquipIcons();
        ClosePatchSelectionPanel();
    }


    // 従来の動作：パッチ選択パネルを閉じてステータスパネルを開く
    public void ClosePatchSelectionPanel()
    {
        try
        {
            if (patchSelectionPanel != null)
                patchSelectionPanel.SetActive(false);
                
            isPatchPanelOpen = false;
            
            // ステータスパネルに戻る
            if (statusPanel != null)
                statusPanel.SetActive(true);
            
            // 統一パネル開閉システム
            OnPanelClosed();
            
            // 特殊処理：パッチパネルからステータスパネルに戻った場合もAFボタンを非表示にする
            HideOpenPanelButtons();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: パッチ選択パネル閉じるエラー - {e.Message}");
        }
    }

    // HP・EXP表示の更新
    private void UpdateTopUI()
    {
        if (playerStats == null || playerStats.stats == null)
        {
            // Debug.LogWarning("UpdateTopUI: playerStatsまたはstatsがnull");
            return;
        }
        var stats = playerStats.stats;
        if (hpBar != null && stats.TotalHP > 0)
            hpBar.value = Mathf.Clamp01(playerStats.CurrentHP / (float)stats.TotalHP);
        if (hpValueText != null)
            hpValueText.text = $"{playerStats.CurrentHP} / {Mathf.RoundToInt(stats.TotalHP)}";
        if (xpBar != null && playerStats.RequiredEXP > 0)
            xpBar.value = Mathf.Clamp01(playerStats.CurrentEXP / (float)playerStats.RequiredEXP);
        if (xpText != null)
            xpText.text = $"XP: {playerStats.CurrentEXP} / {playerStats.RequiredEXP}";
        if (levelText != null)
            levelText.text = $"{playerStats.Level}";
    }

    // 現在の装備表示
    private void UpdateCurrentEquipDisplay()
    {
        // 安全なplayerStats参照
        if (playerStats == null)
            return;
            
        var weaponManager = playerStats.GetComponent<WeaponManager>();
        if (weaponManager != null && weaponManager.currentWeapon != null && weaponManager.currentWeapon.weaponItem != null)
        {
            if (equippedWeaponIcon != null)
                equippedWeaponIcon.sprite = weaponManager.currentWeapon.weaponItem.icon;
            if (equippedWeaponName != null)
                equippedWeaponName.text = weaponManager.currentWeapon.weaponItem.weaponName;
            if (equippedWeaponDescription != null)
                equippedWeaponDescription.text = GenerateCurrentWeaponEffectText(weaponManager.currentWeapon.weaponItem);
            
            // 強化値表示
            UpdateCurrentWeaponEnhancementDisplay(weaponManager.currentWeapon.weaponItem);
            
            // ステータスパネル用武器強化値表示
            UpdateStatusPanelWeaponEnhancement(weaponManager.currentWeapon.weaponItem);
        }
        else
        {
            // 武器が装備されていない場合は強化値を非表示
            if (statusPanelWeaponEnhancementText != null)
                statusPanelWeaponEnhancementText.gameObject.SetActive(false);
        }
    }
    
    // ステータスパネル用武器強化値表示の更新
    private void UpdateStatusPanelWeaponEnhancement(WeaponItem weaponItem)
    {
        if (statusPanelWeaponEnhancementText == null || weaponItem == null)
            return;
            
        // WeaponEnhanceProcessorから強化レベルを取得
        var enhanceProcessor = FindObjectOfType<WeaponEnhanceProcessor>();
        if (enhanceProcessor == null)
        {
            statusPanelWeaponEnhancementText.gameObject.SetActive(false);
            return;
        }
        
        int enhanceLevel = enhanceProcessor.GetWeaponEnhanceLevel(weaponItem);
        
        if (enhanceLevel > 0)
        {
            statusPanelWeaponEnhancementText.text = $"+{enhanceLevel}";
            statusPanelWeaponEnhancementText.color = new Color(0.412f, 0.690f, 1.0f, 1.0f); // #69B0FF
            statusPanelWeaponEnhancementText.gameObject.SetActive(true);
        }
        else
        {
            statusPanelWeaponEnhancementText.gameObject.SetActive(false);
        }
    }

    private void InitializePatchDisplays()
    {
        if (patchDisplays.Count == 0)
        {
            foreach (var patch in playerStats.GetAllOwnedPatches())
            {
                GameObject itemGO = Instantiate(patchItemDisplayPrefab, patchListContent);
                PatchItemDisplay display = itemGO.GetComponent<PatchItemDisplay>();
                patchDisplays.Add(display);
            }
        }
    }
    private void RefreshPatchSlots()
    {
        Debug.Log("UIManager: RefreshPatchSlots - 完全版");
        
        try
        {
            // RefreshStatusPatchSlots()を呼び出す（統合）
            RefreshStatusPatchSlots();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: RefreshPatchSlots エラー - {e.Message}");
        }
    }

    // パッチ選択パネルを表示
    public void ShowPatchSelectionPanel()
    {
        try
        {
            // 統一パネル開閉システム
            OnPanelOpened();
            
            if (statusPanel != null)
                statusPanel.SetActive(false);
                
            if (patchSelectionPanel != null)
                patchSelectionPanel.SetActive(true);
                
            isPatchPanelOpen = true;
            
            // パッチリストを安全に更新
            PopulatePatchListSafely();
            
            // 現在装備中のパッチスロットを更新
            RefreshCurrentPatchSelectionSlots();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: パッチ選択パネル表示エラー - {e.Message}");
        }
    }
    
    // パッチリストを安全に更新
    private void PopulatePatchListSafely()
    {
        if (patchListContent == null || patchItemDisplayPrefab == null)
        {
            Debug.LogError("UIManager: パッチリスト更新 - 必要な参照が不足");
            return;
        }
        
        try
        {
            // 既存のパッチアイテムを安全にクリア
            ClearPatchDisplaysSafely();
            
            // パッチリストを取得
            List<EnhancePatch> patchesToDisplay = new List<EnhancePatch>();
            
            // ArtifactInventoryから所持パッチを取得
            if (artifactInventory != null)
            {
                var ownedPatches = artifactInventory.GetAllOwnedPatches();
                foreach (var patch in ownedPatches)
                {
                    if (patch != null)
                    {
                        patchesToDisplay.Add(patch);
                    }
                }
            }
            
            // 基本的なパッチを追加
            if (nonePatch != null && !patchesToDisplay.Contains(nonePatch))
            {
                patchesToDisplay.Add(nonePatch);
            }
            
            // パッチアイテムを1つずつ安全に生成
            foreach (var patch in patchesToDisplay)
            {
                if (patch != null)
                {
                    CreatePatchDisplaySafely(patch);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: パッチリスト更新エラー - {e.Message}");
        }
    }
    
    // パッチ表示を安全に作成
    private void CreatePatchDisplaySafely(EnhancePatch patch)
    {
        try
        {
            GameObject patchObj = Instantiate(patchItemDisplayPrefab, patchListContent);
            if (patchObj != null)
            {
                PatchItemDisplay display = patchObj.GetComponent<PatchItemDisplay>();
                if (display != null)
                {
                    display.Setup(patch, this);
                    patchDisplays.Add(display);
                }
                else
                {
                    Debug.LogWarning($"UIManager: PatchItemDisplayコンポーネントが見つかりません: {patch.patchName}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: パッチ表示作成エラー ({patch.patchName}) - {e.Message}");
        }
    }
    
    // パッチ表示を安全にクリア
    private void ClearPatchDisplaysSafely()
    {
        try
        {
            foreach (var display in patchDisplays)
            {
                if (display != null && display.gameObject != null)
                {
                    Destroy(display.gameObject);
                }
            }
            patchDisplays.Clear();
            
            Debug.Log("UIManager: パッチ表示クリア完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: パッチ表示クリアエラー - {e.Message}");
        }
    }

    // ここでパッチパネル用のXボタンの強調表示
    public void OnStatusPatchSlotClicked(int index)
    {
        selectedPatchSlotIndex = index;
        // 装備解除せず、必ずパッチ選択パネルを開く
        ShowPatchSelectionPanel();
    }

    // ステータスパネル用のXボタンの強調表示
    private void UpdateStatusPatchSlotHighlights()
    {
        for (int i = 0; i < patchSlotObjects.Count; i++)
        {
            var button = patchSlotObjects[i].GetComponent<PatchSlotButton>();
            if (button != null)
            {
                button.SetSelected(i == selectedPatchSlotIndex);
            }
        }
    }
    public void OnPatchSelectedForCurrentWeapon(EnhancePatch patch)
    {
        try
        {
            var weaponManager = playerStats?.GetComponent<WeaponManager>();
            if (weaponManager == null)
            {
                Debug.LogWarning("UIManager: WeaponManagerが見つかりません");
                return;
            }

            // NonePatchは多重装備可、それ以外は多重装備不可
            if (patch != nonePatch && System.Array.Exists(weaponManager.enhancePatches, p => p == patch))
            {
                Debug.LogWarning("UIManager: 同じパッチは複数スロットに装備できません");
                return;
            }

            // 有効なスロットインデックスかチェック
            if (selectedCurrentPatchSlotIndex >= 0 && selectedCurrentPatchSlotIndex < weaponManager.enhancePatches.Length)
            {
                // パッチを直接設定
                weaponManager.enhancePatches[selectedCurrentPatchSlotIndex] = patch;
                
                // 特殊効果パッチの場合、SpecialPatchEffectを適用
                if (patch != null && patch.isSpecialEffect)
                {
                    ApplySpecialPatchEffect(patch);
                }
                else if (patch == nonePatch)
                {
                    // パッチを外した場合、SpecialPatchEffectを無効化
                    DisableSpecialPatchEffect();
                }
                
                // PlayerStatsに効果を適用
                playerStats.ApplyAllEffects();
                
                // UI更新
                RefreshStatusPatchSlots();
                RefreshCurrentPatchSelectionSlots();
                UpdateStatusTexts();
            }
            else
            {
                Debug.LogWarning($"UIManager: 無効なパッチスロットインデックス: {selectedCurrentPatchSlotIndex}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: OnPatchSelectedForCurrentWeapon エラー - {e.Message}");
        }
    }
    
    /// <summary>
    /// 特殊効果パッチを適用
    /// </summary>
    private void ApplySpecialPatchEffect(EnhancePatch patch)
    {
        if (playerStats == null)
        {
            Debug.LogError("UIManager: PlayerStatsが見つかりません！");
            return;
        }
        
        // SpecialPatchEffectコンポーネントを取得または作成
        SpecialPatchEffect specialEffect = playerStats.GetComponent<SpecialPatchEffect>();
        if (specialEffect == null)
        {
            specialEffect = playerStats.gameObject.AddComponent<SpecialPatchEffect>();
            Debug.Log($"UIManager: SpecialPatchEffectをPlayerに追加しました");
        }
        
        // 特殊効果の設定を更新
        specialEffect.effectType = patch.specialEffectType;
        specialEffect.patchLevel = patch.patchLevel;
        specialEffect.affectedStatType = patch.AffectedStatType;
        specialEffect.targetStatType = patch.TargetStatType;
        
        Debug.Log($"UIManager: 特殊効果を適用 - effectType={patch.specialEffectType}, patchLevel={patch.patchLevel}");
        
        // 特殊効果を適用
        specialEffect.ApplySpecialEffect();
    }
    
    /// <summary>
    /// 特殊効果パッチを無効化
    /// </summary>
    private void DisableSpecialPatchEffect()
    {
        if (playerStats == null)
        {
            Debug.LogError("UIManager: PlayerStatsが見つかりません！");
            return;
        }
        
        // SpecialPatchEffectコンポーネントを取得
        SpecialPatchEffect specialEffect = playerStats.GetComponent<SpecialPatchEffect>();
        if (specialEffect != null)
        {
            // 効果タイプをNoneに設定
            specialEffect.effectType = SpecialEffectType.None;
            Debug.Log("UIManager: 特殊効果パッチを無効化しました");
        }
    }

    // --- スキルパネルの開閉 ---
    public void ShowSkillSelectionPanel()
    {
        try
        {
            // 統一パネル開閉システム
            OnPanelOpened();
            
            if (statusPanel != null)
                statusPanel.SetActive(false);
                
            if (skillSelectionPanel != null)
                skillSelectionPanel.SetActive(true);
            
            // スキルリストを安全に更新
            PopulateSkillListSafely();
            
            // 現在のスキルスロットを更新
            RefreshCurrentSkillSelectionSlots();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: スキル選択パネル表示エラー - {e.Message}");
        }
    }


    public void CloseSkillSelectionPanel()
    {
        try
        {
            if (skillSelectionPanel != null)
                skillSelectionPanel.SetActive(false);
            
            // 統一パネル開閉システム
            OnPanelClosed();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: スキル選択パネル閉じるエラー - {e.Message}");
        }
    }
    
    // スキルリストを安全に更新
    private void PopulateSkillListSafely()
    {
        if (skillListContent == null || skillItemDisplayPrefab == null)
        {
            Debug.LogError("UIManager: スキルリスト更新 - 必要な参照が不足");
            return;
        }
        
        try
        {
            // 既存のスキルアイテムを安全にクリア
            ClearSkillDisplaysSafely();
            
            // スキルリストを取得
            List<SkillData> skillsToDisplay = new List<SkillData>();
            
            if (allSkills != null && allSkills.Count > 0)
            {
                skillsToDisplay.AddRange(allSkills);
            }
            
            // ArtifactInventoryから所持スキルを取得
            if (artifactInventory != null)
            {
                var ownedSkills = artifactInventory.GetAllOwnedSkills();
                foreach (var skill in ownedSkills)
                {
                    if (skill != null && !skillsToDisplay.Contains(skill))
                    {
                        skillsToDisplay.Add(skill);
                    }
                }
            }
            
            // スキルアイテムを1つずつ安全に生成
            foreach (var skill in skillsToDisplay)
            {
                if (skill != null)
                {
                    CreateSkillDisplaySafely(skill);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: スキルリスト更新エラー - {e.Message}");
        }
    }
    
    // スキル表示を安全に作成
    private void CreateSkillDisplaySafely(SkillData skill)
    {
        try
        {
            GameObject skillObj = Instantiate(skillItemDisplayPrefab, skillListContent);
            if (skillObj != null)
            {
                SkillItemDisplay display = skillObj.GetComponent<SkillItemDisplay>();
                if (display != null)
                {
                    display.Setup(skill, this);
                    skillDisplays.Add(display);
                }
                else
                {
                    Debug.LogWarning($"UIManager: SkillItemDisplayコンポーネントが見つかりません: {skill.skillName}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: スキル表示作成エラー ({skill.skillName}) - {e.Message}");
        }
    }
    
    // スキル表示を安全にクリア
    private void ClearSkillDisplaysSafely()
    {
        try
        {
            foreach (var display in skillDisplays)
            {
                if (display != null && display.gameObject != null)
                {
                    Destroy(display.gameObject);
                }
            }
            skillDisplays.Clear();
            
            Debug.Log("UIManager: スキル表示クリア完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: スキル表示クリアエラー - {e.Message}");
        }
    }

    // --- カレントスキルスロット生成 ---
    private void RefreshCurrentSkillSelectionSlots()
    {
        foreach (var slotObj in currentSkillSlotObjects)
        {
            if (slotObj != null)
                Destroy(slotObj);
        }
        currentSkillSlotObjects.Clear();
        int totalSlots = 3;
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slot = Instantiate(currentSkillSlotPrefab, currentSkillSlotContainer);
            currentSkillSlotObjects.Add(slot);
            var btn = slot.GetComponent<CurrentSkillSlotUI>();
            btn?.Setup(i, OnCurrentSkillSlotClicked);
            btn?.SetSelected(false);
            btn?.SetSkillIcon(equippedSkills[i]?.icon);
        }
        UpdateCurrentSkillSlotHighlights();
    }

    private void OnCurrentSkillSlotClicked(int slotIndex)
    {
        selectedCurrentSkillSlotIndex = slotIndex;
        UpdateCurrentSkillSlotHighlights();
    }

    private void UpdateCurrentSkillSlotHighlights()
    {
        for (int i = 0; i < currentSkillSlotObjects.Count; i++)
        {
            var btn = currentSkillSlotObjects[i].GetComponent<CurrentSkillSlotUI>();
            if (btn != null)
                btn.SetSelected(i == selectedCurrentSkillSlotIndex);
        }
    }

    // --- スキル選択時 ---
    public void OnSkillSelected(SkillData selected)
    {
        if (selectedCurrentSkillSlotIndex >= 0 && selectedCurrentSkillSlotIndex < equippedSkills.Length)
        {
            // NoneSkillは何個でもOK、それ以外は多重装備不可
            if (selected.shape != SkillShape.NoneSkill && equippedSkills.Contains(selected))
            {
                // Debug.LogWarning("同じスキルは複数スロットに装備できません");
                return;
            }
            equippedSkills[selectedCurrentSkillSlotIndex] = selected;

            // SkillControllerにも反映（キャッシュ使用）
            if (cachedSkillController != null)
            {
                cachedSkillController.SetSkill(selectedCurrentSkillSlotIndex, selected);
            }

            // ActivateButtonにも即時反映（フリーズ対策：ボタン配列使用）
            if (skillActivateButtons != null)
            {
                foreach (var btnObj in skillActivateButtons)
                {
                    if (btnObj != null)
                    {
                        var btn = btnObj.GetComponent<SkillActivateButton>();
                        if (btn != null)
                        {
                            btn.UpdateIconAndCooldown();
                        }
                    }
                }
            }

            RefreshCurrentSkillSelectionSlots();
        }
        // 画面遷移はしない
    }

    /// <summary>
    /// オプションメニューを開く
    /// </summary>
    public void ShowOptionsMenu()
    {
        if (optionsMenu != null)
        {
            optionsMenu.OpenOptionsMenu();
            OnPanelOpened();
        }
        else
        {
            Debug.LogWarning("UIManager: OptionsMenuが見つかりません");
        }
    }

    // --- 素材パネルをUIManager経由で開く ---
    // 直感的な動作：マテリアルパネルのみを閉じる

    public void ShowMaterialPanel()
    {
        // キャッシュ使用（フリーズ対策）
        if (cachedMaterialUIManager != null && cachedMaterialUIManager.materialPanel != null)
        {
            // 統一パネル開閉システムを使用
            OnPanelOpened();
            
            // 他のUIボタンを個別に非表示
            if (openButton != null) openButton.SetActive(false);
            if (closeButton != null) closeButton.SetActive(false);
            if (showArtifactPanelButton != null) showArtifactPanelButton.gameObject.SetActive(false);
            if (playerControlObject != null) playerControlObject.SetActive(false);
            // マテリアルパネルを表示
            cachedMaterialUIManager.materialPanel.SetActive(true);
            cachedMaterialUIManager.RefreshMaterialList();
        }
    }

    // === パネル開閉システム ===
    /// <summary>
    /// パネルを開くときに共通して実行する処理（開く系ボタンを隠す）
    /// </summary>
    public void OnPanelOpened()
    {
        // Debug.Log("UIManager: パネルが開かれました - 開く系ボタンを隠します");
        HideOpenPanelButtons();
    }

    /// <summary>
    /// パネルを閉じるときに共通して実行する処理（開く系ボタンを表示）
    /// </summary>
    public void OnPanelClosed()
    {
        // Debug.Log("UIManager: パネルが閉じられました - 開く系ボタンを表示します");
        ShowOpenPanelButtons();
    }

    /// <summary>
    /// 開く系ボタンを全て隠す
    /// </summary>
    private void HideOpenPanelButtons()
    {
        if (openPanelButtons != null)
        {
            int hiddenCount = 0;
            foreach (var button in openPanelButtons)
            {
                if (button != null)
                {
                    button.SetActive(false);
                    hiddenCount++;
                }
            }
            // Debug.Log($"UIManager: {hiddenCount}個の開く系ボタンを隠しました");
        }
        else
        {
            // Debug.LogWarning("UIManager: openPanelButtons配列が設定されていません");
        }
    }

    /// <summary>
    /// 開く系ボタンを全て表示
    /// </summary>
    private void ShowOpenPanelButtons()
    {
        if (openPanelButtons != null)
        {
            int shownCount = 0;
            foreach (var button in openPanelButtons)
            {
                if (button != null)
                {
                    button.SetActive(true);
                    shownCount++;
                }
            }
            // Debug.Log($"UIManager: {shownCount}個の開く系ボタンを表示しました");
        }
        else
        {
            // Debug.LogWarning("UIManager: openPanelButtons配列が設定されていません");
        }
    }

    // 重い処理を非同期で実行
    private System.Collections.IEnumerator InitializeUIComponentsAsync()
    {
        Debug.Log("UIManager: 非同期UI初期化開始");
        
        // フレームを分けて処理
        yield return new WaitForEndOfFrame();
        
        // 武器リストの初期化
        try
        {
            if (allWeapons != null && allWeapons.Count > 0)
            {
                Debug.Log("UIManager: 武器リスト初期化完了");
            }
            else
            {
                Debug.LogWarning("UIManager: 武器リストが空です");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: 武器リスト初期化エラー - {e.Message}");
        }
        
        yield return new WaitForEndOfFrame();
        
        // スキルリストの初期化
        try
        {
            if (allSkills != null && allSkills.Count > 0)
            {
                Debug.Log("UIManager: スキルリスト初期化完了");
            }
            else
            {
                Debug.LogWarning("UIManager: スキルリストが空です");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: スキルリスト初期化エラー - {e.Message}");
        }
        
        yield return new WaitForEndOfFrame();
        
        // パッチリストの初期化
        try
        {
            InitializePatchDisplays();
            Debug.Log("UIManager: パッチリスト初期化完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: パッチリスト初期化エラー - {e.Message}");
        }
        
        yield return new WaitForEndOfFrame();
        
        // ArtifactInventoryイベント接続
        try
        {
            if (artifactInventory != null)
            {
                artifactInventory.OnInventoryChanged += OnInventoryChangedHandler;
                Debug.Log("UIManager: ArtifactInventoryイベント接続完了");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: ArtifactInventoryイベント接続エラー - {e.Message}");
        }
        
        yield return new WaitForEndOfFrame();
        
        Debug.Log("UIManager: 非同期UI初期化完了 - 全機能が安全に復活しました");
    }

    // LateUpdate も最適化
    private void LateUpdate()
    {
        // フリーズ対策：LateUpdateの頻度を制限
        if (updateTimer < UPDATE_INTERVAL)
            return;
            
        // 安全なnullチェック
        if (playerStats == null || !hasWarnedInLateUpdate)
        {
            if (!hasWarnedInLateUpdate)
            {
                Debug.Log("UIManager: LateUpdate - PlayerStats待機中");
                hasWarnedInLateUpdate = true;
            }
            return;
        }
        
        // 軽量なUI更新のみ実行
        try
        {
            // HP/EXPバーの更新など軽い処理のみ
            if (hpValueText != null)
            {
                hpValueText.text = $"{playerStats.CurrentHP} / {Mathf.RoundToInt(playerStats.stats.TotalHP)}";
            }
            
            if (xpBar != null && xpText != null)
            {
                float xpPercent = playerStats.GetEXPPercent();
                xpBar.value = xpPercent;
                xpText.text = $"EXP: {playerStats.CurrentEXP} / {playerStats.RequiredEXP}";
            }
        }
        catch (System.Exception)
        {
            // エラーが発生しても静かに処理
            return;
        }
    }
    
    // 削除された元の機能メソッドを復元
    public void OnCurrentPatchSlotClicked(int slotIndex)
    {
        Debug.Log($"UIManager: OnCurrentPatchSlotClicked - スロット{slotIndex}がクリックされました");
        
        try
        {
            selectedCurrentPatchSlotIndex = slotIndex;
            UpdateCurrentPatchSlotHighlights();
            
            Debug.Log($"UIManager: 現在のパッチスロット{slotIndex}を選択しました");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: OnCurrentPatchSlotClicked エラー - {e.Message}");
        }
    }
    
    private void UpdateCurrentPatchSlotHighlights()
    {
        Debug.Log("UIManager: UpdateCurrentPatchSlotHighlights - パッチスロットハイライト更新");
        
        try
        {
            if (currentPatchSlotObjects == null) return;
            
            for (int i = 0; i < currentPatchSlotObjects.Count; i++)
            {
                var button = currentPatchSlotObjects[i].GetComponent<CurrentPatchSlotButton>();
                if (button != null)
                {
                    button.SetSelected(i == selectedCurrentPatchSlotIndex);
                }
            }
            
            Debug.Log($"UIManager: パッチスロットハイライト更新完了 - 選択中: {selectedCurrentPatchSlotIndex}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: UpdateCurrentPatchSlotHighlights エラー - {e.Message}");
        }
    }
    
    // 消費アイテム使用機能の追加
    public void UseConsumableItem(ConsumableItem item)
    {
        Debug.Log($"UIManager: 消費アイテム使用 - {item.ItemName}");
        
        if (artifactInventory == null)
        {
            Debug.LogWarning("UIManager: ArtifactInventory が null です");
            return;
        }
        
        try
        {
            int currentCount = artifactInventory.GetConsumableCount(item);
            if (currentCount > 0)
            {
                // アイテム効果を適用
                ApplyConsumableEffect(item);
                
                // 消費アイテム数を減らす
                artifactInventory.RemoveItem(item, 1);
                
                // UI更新
                RefreshArtifactDisplayLightweight();
                
                Debug.Log($"UIManager: {item.ItemName} を使用しました");
            }
            else
            {
                Debug.LogWarning($"UIManager: {item.ItemName} を所持していません");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: 消費アイテム使用エラー - {e.Message}");
        }
    }
    
    private void ApplyConsumableEffect(ConsumableItem item)
    {
        // 消費アイテムの効果を適用
        if (playerStats == null)
        {
            Debug.LogWarning("UIManager: PlayerStats が null です");
            return;
        }
        
        // 消費アイテムの基本的な効果を適用
        if (item.itemName.Contains("HP") || item.itemName.Contains("体力"))
        {
            // HP回復系アイテム
            int healAmount = 50; // デフォルト回復量
            playerStats.Heal(healAmount);
            Debug.Log($"UIManager: {item.itemName}でHP {healAmount} 回復");
        }
        else if (item.itemName.Contains("ポーション"))
        {
            // ポーション系アイテム
            playerStats.Heal(30);
            Debug.Log($"UIManager: {item.itemName}でHP 30 回復");
        }
        else
        {
            // その他のアイテム
            Debug.Log($"UIManager: {item.itemName} を使用しました（効果不明）");
        }
    }

        private void RefreshCurrentPatchSelectionSlots()
    {
        Debug.Log("UIManager: RefreshCurrentPatchSelectionSlots - 完全版");
        
        if (currentPatchSlotContainer == null || currentPatchSlotPrefab == null)
        {
            Debug.LogError("UIManager: RefreshCurrentPatchSelectionSlots - 必要な参照が不足");
            return;
        }
        
        try
        {
            // WeaponManagerの取得
            var weaponManager = playerStats?.GetComponent<WeaponManager>();
            if (weaponManager == null)
            {
                Debug.LogWarning("UIManager: WeaponManagerが見つかりません");
                return;
            }
            
            // 現在装備中のパッチを取得
            var currentPatches = weaponManager.GetEnhancePatches();
            int maxSlots = weaponManager.GetMaxPatchSlots();
            
            Debug.Log($"UIManager: 現在のパッチスロット数: {maxSlots}, 現在のパッチ数: {currentPatches.Length}");
            
            // 必要なスロット数まで生成
            while (currentPatchSlotObjects.Count < maxSlots)
            {
                GameObject slot = Instantiate(currentPatchSlotPrefab, currentPatchSlotContainer);
                if (slot != null)
                {
                    slot.SetActive(true);
                    currentPatchSlotObjects.Add(slot);
                    
                    var button = slot.GetComponent<CurrentPatchSlotButton>();
                    if (button != null)
                    {
                        button.Setup(this, currentPatchSlotObjects.Count - 1);
                    }
                }
            }
            
            // 不要なスロットを非表示
            for (int i = 0; i < currentPatchSlotObjects.Count; i++)
            {
                if (i < maxSlots)
                {
                    currentPatchSlotObjects[i].SetActive(true);
                    
                    var button = currentPatchSlotObjects[i].GetComponent<CurrentPatchSlotButton>();
                    if (button != null)
                    {
                        // 現在装備中のパッチを表示
                        if (i < currentPatches.Length && currentPatches[i] != null)
                        {
                            button.SetPatchIcon(currentPatches[i].icon);
                            button.SetPatchFrame(currentPatches[i].rarity);
                        }
                        else
                        {
                            button.SetPatchIcon(null);
                            button.SetPatchFrame(EnhancePatch.PatchRarity.Common);
                        }
                    }
                }
                else
                {
                    currentPatchSlotObjects[i].SetActive(false);
                }
            }
            
            // 選択状態を更新
            UpdateCurrentPatchSlotHighlights();
            
            Debug.Log("UIManager: RefreshCurrentPatchSelectionSlots - 完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: RefreshCurrentPatchSelectionSlots エラー - {e.Message}");
        }
    }
    
    private void RefreshStatusPatchSlots()
    {
        Debug.Log("UIManager: RefreshStatusPatchSlots - 完全版");
        
        if (patchSlotContainer == null || patchSlotPrefab == null)
        {
            Debug.LogError("UIManager: RefreshStatusPatchSlots - 必要な参照が不足");
            return;
        }
        
        try
        {
            // WeaponManagerの取得
            var weaponManager = playerStats?.GetComponent<WeaponManager>();
            if (weaponManager == null)
            {
                Debug.LogWarning("UIManager: WeaponManagerが見つかりません");
                return;
            }
            
            // 現在装備中のパッチを取得
            var currentPatches = weaponManager.GetEnhancePatches();
            int maxSlots = weaponManager.GetMaxPatchSlots();
            
            Debug.Log($"UIManager: パッチスロット数: {maxSlots}, 現在のパッチ数: {currentPatches.Length}");
            
            // 必要なスロット数まで生成
            while (patchSlotObjects.Count < maxSlots)
            {
                GameObject slot = Instantiate(patchSlotPrefab, patchSlotContainer);
                if (slot != null)
                {
                    slot.SetActive(true);
                    patchSlotObjects.Add(slot);
                    
                    var button = slot.GetComponent<PatchSlotButton>();
                    if (button != null)
                    {
                        button.Setup(this, patchSlotObjects.Count - 1);
                    }
                }
            }
            
            // 不要なスロットを非表示
            for (int i = 0; i < patchSlotObjects.Count; i++)
            {
                if (i < maxSlots)
                {
                    patchSlotObjects[i].SetActive(true);
                    
                    var button = patchSlotObjects[i].GetComponent<PatchSlotButton>();
                    if (button != null)
                    {
                        // 現在装備中のパッチを表示
                        if (i < currentPatches.Length && currentPatches[i] != null)
                        {
                            button.SetPatchIcon(currentPatches[i].icon);
                            button.SetPatchFrame(currentPatches[i].rarity);
                        }
                        else
                        {
                            button.SetPatchIcon(null);
                            button.SetPatchFrame(EnhancePatch.PatchRarity.Common);
                        }
                    }
                }
                else
                {
                    patchSlotObjects[i].SetActive(false);
                }
            }
            
            // 選択状態を更新
            UpdateStatusPatchSlotHighlights();
            
            Debug.Log("UIManager: RefreshStatusPatchSlots - 完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: RefreshStatusPatchSlots エラー - {e.Message}");
        }
    }

    /// <summary>
    /// 全ての武器表示の強化値を更新
    /// </summary>
    private void RefreshAllWeaponEnhancementDisplays()
    {
        try
        {
            foreach (var display in weaponDisplays)
            {
                if (display != null)
                {
                    display.RefreshEnhancementDisplay();
                }
            }
            Debug.Log("UIManager: 全武器の強化値表示を更新しました");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager: 武器強化表示更新でエラー - {e.Message}");
        }
    }

    private string GenerateCurrentWeaponEffectText(WeaponItem weapon)
    {
        var effectParts = new System.Collections.Generic.List<string>();
        
        // WeaponEnhanceProcessorから強化レベルを取得
        var enhanceProcessor = FindObjectOfType<WeaponEnhanceProcessor>();
        int enhanceLevel = 0;
        
        if (enhanceProcessor != null && weapon != null)
        {
            enhanceLevel = enhanceProcessor.GetWeaponEnhanceLevel(weapon);
        }
        
        // 攻撃力（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusAttack > 0 || (weapon.enhanceAttack > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusAttack;
            float enhanceValue = weapon.enhanceAttack * enhanceLevel;
            string attackText = $"ATK +{baseValue:F0}";
            if (enhanceValue > 0)
                attackText += $"(+{enhanceValue:F0})";
            effectParts.Add(attackText);
        }
        
        // HP（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusHP > 0 || (weapon.enhanceHP > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusHP;
            float enhanceValue = weapon.enhanceHP * enhanceLevel;
            string hpText = $"HP +{baseValue:F0}";
            if (enhanceValue > 0)
                hpText += $"(+{enhanceValue:F0})";
            effectParts.Add(hpText);
        }
        
        // 防御力（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusDefense > 0 || (weapon.enhanceDefense > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusDefense;
            float enhanceValue = weapon.enhanceDefense * enhanceLevel;
            string defenseText = $"DEF +{baseValue:F0}";
            if (enhanceValue > 0)
                defenseText += $"(+{enhanceValue:F0})";
            effectParts.Add(defenseText);
        }
        
        // 魔法攻撃力（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusMagicAttack > 0 || (weapon.enhanceMagicAttack > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusMagicAttack;
            float enhanceValue = weapon.enhanceMagicAttack * enhanceLevel;
            string magicAttackText = $"MAT +{baseValue:F0}";
            if (enhanceValue > 0)
                magicAttackText += $"(+{enhanceValue:F0})";
            effectParts.Add(magicAttackText);
        }
        
        // 魔法防御力（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusMagicDefense > 0 || (weapon.enhanceMagicDefense > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusMagicDefense;
            float enhanceValue = weapon.enhanceMagicDefense * enhanceLevel;
            string magicDefenseText = $"MDF +{baseValue:F0}";
            if (enhanceValue > 0)
                magicDefenseText += $"(+{enhanceValue:F0})";
            effectParts.Add(magicDefenseText);
        }
        
        // 運（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusFate > 0 || (weapon.enhanceFate > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusFate;
            float enhanceValue = weapon.enhanceFate * enhanceLevel;
            string fateText = $"運 +{baseValue:F0}";
            if (enhanceValue > 0)
                fateText += $"(+{enhanceValue:F0})";
            effectParts.Add(fateText);
        }
        
        // 効果がない場合
        if (effectParts.Count == 0)
        {
            return "効果なし";
        }
        
        // 空白で結合（改行ではなく空白区切り）
        return string.Join(" ", effectParts);
    }

    private void UpdateCurrentWeaponEnhancementDisplay(WeaponItem weapon)
    {
        // 強化値表示の更新
        if (equippedWeaponEnhancementLevel != null)
        {
            var enhanceProcessor = FindObjectOfType<WeaponEnhanceProcessor>();
            int enhanceLevel = 0;
            
            if (enhanceProcessor != null && weapon != null)
            {
                enhanceLevel = enhanceProcessor.GetWeaponEnhanceLevel(weapon);
            }
            
            if (enhanceLevel > 0)
            {
                equippedWeaponEnhancementLevel.text = $"+{enhanceLevel}";
                equippedWeaponEnhancementLevel.gameObject.SetActive(true);
            }
            else
            {
                equippedWeaponEnhancementLevel.gameObject.SetActive(false);
            }
        }
    }
    
    // === ボタンレイアウト管理システム ===
    
    /// <summary>
    /// ボタン間のスペーシングを調整
    /// </summary>
    public void SetButtonSpacing(float spacing)
    {
        if (buttonLayoutGroup != null)
        {
            buttonLayoutGroup.spacing = spacing;
            Debug.Log($"[UIManager] ボタンスペーシングを{spacing}に設定しました");
        }
    }
    
    /// <summary>
    /// 新しいボタンをコンテナに追加
    /// </summary>
    public void AddButtonToContainer(GameObject button)
    {
        if (buttonContainer != null && button != null)
        {
            button.transform.SetParent(buttonContainer);
            button.transform.localScale = Vector3.one;
            Debug.Log($"[UIManager] ボタン「{button.name}」をコンテナに追加しました");
            
            // openPanelButtons配列も更新
            AddToOpenPanelButtons(button);
        }
    }
    
    /// <summary>
    /// openPanelButtons配列に新しいボタンを追加
    /// </summary>
    private void AddToOpenPanelButtons(GameObject button)
    {
        if (openPanelButtons != null)
        {
            var newArray = new GameObject[openPanelButtons.Length + 1];
            for (int i = 0; i < openPanelButtons.Length; i++)
            {
                newArray[i] = openPanelButtons[i];
            }
            newArray[openPanelButtons.Length] = button;
            openPanelButtons = newArray;
            
            Debug.Log($"[UIManager] openPanelButtons配列にボタンを追加。現在のボタン数: {openPanelButtons.Length}");
        }
    }
    
    /// <summary>
    /// レイアウトを強制的に再計算
    /// </summary>
    public void RefreshButtonLayout()
    {
        if (buttonLayoutGroup != null)
        {
            buttonLayoutGroup.SetLayoutHorizontal();
            buttonLayoutGroup.SetLayoutVertical();
            Debug.Log("[UIManager] ボタンレイアウトを再計算しました");
        }
    }
    
    /// <summary>
    /// コンテナ内のボタン数を取得
    /// </summary>
    public int GetButtonCount()
    {
        return buttonContainer != null ? buttonContainer.childCount : 0;
    }
  }