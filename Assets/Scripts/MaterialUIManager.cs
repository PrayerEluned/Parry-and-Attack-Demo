using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Game.Items;
using System;

/// <summary>
/// 素材アイテムUI管理クラス
/// 既存のArtifactInventoryとConsumableItemを使用
/// </summary>
public class MaterialUIManager : MonoBehaviour
{
    [Header("== 素材アイテムUI ==")]
    [Header("パネル参照")]
    public GameObject materialPanel;
    public GameObject itemDetailPanel;
    public Button openMaterialPanelButton;
    public Button closeMaterialPanelButton;
    public Button closeDetailPanelButton;
    
    [Header("アイテム一覧")]
    public Transform itemGridParent;
    public GameObject itemButtonPrefab;
    
    [Header("詳細パネル")]
    public Image detailIcon;
    public TMP_Text detailName;
    public TMP_Text detailCount;
    public TMP_Text detailDescription;
    
    [Header("データ参照")]
    public ArtifactInventory artifactInventory;
    public UIManager uiManager; // UIManagerの参照を追加
    
    [Header("開くボタン（複数対応）")]
    public List<Button> openPanelButtons; // Inspectorで複数登録
    
    [Header("常時表示ボタン（Inspectorで登録）")]
    [HideInInspector] public List<Button> managedButtons;
    
    // === キャッシュ化された参照（フリーズ対策） ===
    private static MaterialUIManager cachedInstance;
    
    private List<GameObject> activeItemButtons = new List<GameObject>();
    
    private void Start()
    {
        Debug.Log("MaterialUIManager: マテリアルシステム解禁テスト - 初期化を開始");
        
        try
        {
            // 基本的なマテリアルUI初期化
            InitializeMaterialUI();
            InitializeUI();
            SetupEventListeners(); // ボタンイベントを自動設定
            Debug.Log("MaterialUIManager: 初期化完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MaterialUIManager: 初期化エラー - {e.Message}");
        }
    }
    
    private void InitializeMaterialUI()
    {
        // マテリアルUIの基本初期化
        Debug.Log("MaterialUIManager: マテリアルUI初期化");
        
        // 軽量な初期化処理
        if (transform != null)
        {
            Debug.Log("MaterialUIManager: Transform確認完了");
        }
    }
    
    void OnDestroy()
    {
        if (artifactInventory != null)
        {
            artifactInventory.OnInventoryChanged -= OnInventoryChanged;
        }
    }
    
    void InitializeUI()
    {
        if (materialPanel != null)
            materialPanel.SetActive(false);
        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);
    }
    
    void SetupEventListeners()
    {
        // Inspectorで必ずOpenMaterialPanel(Button)に自分自身のButtonを渡す形で設定してください
        if (closeMaterialPanelButton != null)
            closeMaterialPanelButton.onClick.AddListener(CloseMaterialPanel);
        if (closeDetailPanelButton != null)
            closeDetailPanelButton.onClick.AddListener(CloseDetailPanel);
    }
    
    public void OpenMaterialPanel(Button pressedButton)
    {
        if (materialPanel == null || artifactInventory == null) return;
        
        // UIManagerの統一パネル開閉システムを呼ぶ
        if (uiManager != null)
        {
            uiManager.OnPanelOpened();
        }
        
        materialPanel.SetActive(true);
        itemDetailPanel?.SetActive(false);
        Time.timeScale = 0f;
        RefreshMaterialList();
    }
    
    public void RefreshMaterialList()
    {
        if (itemGridParent == null || itemButtonPrefab == null || artifactInventory == null) 
        {
            Debug.LogError($"MaterialUIManager: RefreshMaterialList - 必要な参照が不足: itemGridParent={itemGridParent}, itemButtonPrefab={itemButtonPrefab}, artifactInventory={artifactInventory}");
            return;
        }
        
        // ArtifactInventoryのインスタンス情報をデバッグ
        Debug.Log($"MaterialUIManager: 使用中のArtifactInventoryインスタンスID={artifactInventory.GetInstanceID()}");
        Debug.Log($"MaterialUIManager: ArtifactInventory.Instance={ArtifactInventory.Instance?.GetInstanceID()}");
        
        ClearItemButtons();
        var items = artifactInventory.GetAllOwnedConsumables();
        
        Debug.Log($"MaterialUIManager: GetAllOwnedConsumables()の結果: {items.Count}個");
        foreach (var item in items)
        {
            Debug.Log($"MaterialUIManager: 消費アイテム発見: {item.itemName} (ID: {item.itemID})");
            int count = artifactInventory.GetConsumableCount(item);
            Debug.Log($"MaterialUIManager: {item.itemName}の所持数: {count}");
            var buttonObj = Instantiate(itemButtonPrefab, itemGridParent);
            SetupConsumableButton(buttonObj, item, count);
            activeItemButtons.Add(buttonObj);
        }
        Debug.Log($"MaterialUIManager: {items.Count}個のConsumableアイテムを表示しました。");
    }
    
    void SetupConsumableButton(GameObject buttonObj, ConsumableItem item, int count)
    {
        var icon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        var countText = buttonObj.transform.Find("CountText")?.GetComponent<TMP_Text>();
        var button = buttonObj.GetComponent<Button>();
        if (icon != null && item.Icon != null)
            icon.sprite = item.Icon;
        if (countText != null)
        {
            countText.text = $"x{count}";
        }
        if (button != null)
            button.onClick.AddListener(() => ShowItemDetail(item));
    }
    
    public void ShowItemDetail(ConsumableItem item)
    {
        if (itemDetailPanel == null || item == null) return;
        
        // UIManagerの統一パネル開閉システムを呼ぶ
        if (uiManager != null)
        {
            uiManager.OnPanelOpened();
        }
        
        itemDetailPanel.SetActive(true);
        Time.timeScale = 0f;
        if (detailIcon != null)
            detailIcon.sprite = item.Icon;
        if (detailName != null)
            detailName.text = item.ItemName;
        if (detailCount != null)
            detailCount.text = $"x{artifactInventory.GetConsumableCount(item)}";
        if (detailDescription != null)
            detailDescription.text = item.Description;
        Debug.Log($"MaterialUIManager: {item.ItemName}の詳細を表示しました。");
    }
    
    public void CloseMaterialPanel()
    {
        Debug.Log("Unity 6フリーズ対策: MaterialUIManager パネルを閉じます");
        
        try
        {
            if (materialPanel != null)
            {
                materialPanel.SetActive(false);
                Debug.Log("MaterialUIManager: マテリアルパネルを閉じました");
            }
            
            // プレイヤーコントロールを復活
            var characterMovement = FindObjectOfType<CharacterMovement>();
            if (characterMovement != null)
            {
                characterMovement.EnableMovement(true);
                Debug.Log("MaterialUIManager: プレイヤーコントロールを復活しました");
            }
            
            // 時間スケールを復活
            Time.timeScale = 1f;
            
            // 他のUIボタンを復活
            var uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.OnPanelClosed();
            }
            
            Debug.Log("MaterialUIManager: 閉じる処理完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MaterialUIManager: 閉じる処理エラー - {e.Message}");
            
            // エラー時でも最低限の復旧
            Time.timeScale = 1f;
            if (materialPanel != null)
                materialPanel.SetActive(false);
        }
    }
    
    public void CloseDetailPanel()
    {
        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);
            
        // 他のパネルも閉じていれば時間再開とボタン表示
        if (materialPanel == null || !materialPanel.activeInHierarchy)
        {
            Time.timeScale = 1f;
            
            // UIManagerの統一パネル開閉システムを呼ぶ
            if (uiManager != null)
            {
                uiManager.OnPanelClosed();
            }
        }
        
        Debug.Log("MaterialUIManager: 詳細パネルを閉じました。");
    }
    
    void ClearItemButtons()
    {
        foreach (var button in activeItemButtons)
        {
            if (button != null)
                Destroy(button);
        }
        activeItemButtons.Clear();
    }
    
    public void OnInventoryChanged()
    {
        if (materialPanel != null && materialPanel.activeInHierarchy)
        {
            RefreshMaterialList();
        }
    }
    
    [ContextMenu("テスト用アイテム追加")]
    private void AddTestItems()
    {
        if (artifactInventory == null)
        {
            Debug.LogWarning("MaterialUIManager: ArtifactInventoryが見つかりません。");
            return;
        }
        
        Debug.Log("MaterialUIManager: テスト用アイテムを追加するには、ConsumableItemアセットを作成してArtifactInventory.AddConsumableを呼んでください。");
    }
    
    // 消費アイテム使用ボタンの処理
    public void OnUseConsumableItem(ConsumableItem item)
    {
        Debug.Log($"MaterialUIManager: 消費アイテム使用 - {item.ItemName}");
        
        if (artifactInventory == null)
        {
            Debug.LogWarning("MaterialUIManager: ArtifactInventory が null です");
            return;
        }
        
        try
        {
            int currentCount = artifactInventory.GetConsumableCount(item);
            if (currentCount > 0)
            {
                // UIManagerの消費アイテム使用機能を呼び出し
                var uiManager = FindObjectOfType<UIManager>();
                if (uiManager != null)
                {
                    uiManager.UseConsumableItem(item);
                    
                    // マテリアルパネルも更新
                    RefreshMaterialList();
                    
                    Debug.Log($"MaterialUIManager: {item.ItemName} 使用完了");
                }
                else
                {
                    Debug.LogWarning("MaterialUIManager: UIManager が見つかりません");
                }
            }
            else
            {
                Debug.LogWarning($"MaterialUIManager: {item.ItemName} を所持していません");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MaterialUIManager: 消費アイテム使用エラー - {e.Message}");
        }
    }
} 