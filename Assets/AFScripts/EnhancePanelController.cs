using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
// WeaponEnhanceItemDisplayが別namespaceの場合はusingを追加

public class EnhancePanelController : MonoBehaviour
{
    [Header("武器リスト表示用")]
    [SerializeField] private Transform weaponListContent;
    [SerializeField] private GameObject weaponItemDisplayPrefab;
    [SerializeField] private ArtifactInventory artifactInventory; // 所持品管理

    [Header("強化詳細パネル")]
    [SerializeField] private WeaponEnhanceDetailPanel detailPanel; // 詳細パネルスクリプト

    private List<WeaponItem> ownedWeapons = new List<WeaponItem>();
    private List<WeaponEnhanceItemDisplay> weaponButtonDisplays = new List<WeaponEnhanceItemDisplay>();
    private int selectedWeaponIndex = 0;

    private void OnEnable()
    {
        Debug.Log("EnhancePanelController: パネルが有効化されました");
        // UIシステムの健全性を確認
        EnsureUISystemHealth();
        PopulateWeaponList();
    }
    
    /// <summary>
    /// UIシステムの健全性を確保
    /// </summary>
    private void EnsureUISystemHealth()
    {
        Debug.Log("EnhancePanelController: UIシステム健全性チェック開始");
        
        // 1. EventSystem確認
        var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("EventSystemが見つかりません。作成します。");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        else
        {
            Debug.Log($"✅ EventSystem見つかりました: {eventSystem.name}");
        }
        
        // 2. Canvas & GraphicRaycaster確認
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            var raycaster = parentCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogWarning($"Canvas '{parentCanvas.name}' にGraphicRaycasterがありません。追加します。");
                raycaster = parentCanvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            else
            {
                Debug.Log($"✅ GraphicRaycaster見つかりました: {raycaster.name}");
            }
            
            // Canvas設定確認
            Debug.Log($"Canvas設定: renderMode={parentCanvas.renderMode}, sortingOrder={parentCanvas.sortingOrder}");
        }
        
        // 3. パネル自体のCanvasGroup確認
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = Mathf.Max(canvasGroup.alpha, 0.9f);
            Debug.Log($"CanvasGroup設定: interactable={canvasGroup.interactable}, blocksRaycasts={canvasGroup.blocksRaycasts}, alpha={canvasGroup.alpha}");
        }
        
        // 4. ScrollView設定確認
        var scrollView = GetComponentInChildren<ScrollRect>();
        if (scrollView != null)
        {
            Debug.Log($"ScrollView見つかりました: {scrollView.name}");
            Debug.Log($"  - ScrollView enabled: {scrollView.enabled}");
            Debug.Log($"  - Viewport: {scrollView.viewport != null}");
            Debug.Log($"  - Content: {scrollView.content != null}");
            
            if (scrollView.viewport != null)
            {
                var viewportImage = scrollView.viewport.GetComponent<Image>();
                if (viewportImage != null)
                {
                    Debug.Log($"  - Viewport raycastTarget: {viewportImage.raycastTarget}");
                }
            }
        }
        
        // 5. Content設定確認
        if (weaponListContent != null)
        {
            Debug.Log($"weaponListContent設定:");
            Debug.Log($"  - Active: {weaponListContent.gameObject.activeInHierarchy}");
            Debug.Log($"  - RectTransform: {weaponListContent.GetComponent<RectTransform>() != null}");
            
            var layoutGroup = weaponListContent.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                Debug.Log($"  - VerticalLayoutGroup: spacing={layoutGroup.spacing}, childControlHeight={layoutGroup.childControlHeight}");
            }
            
            var contentSizeFitter = weaponListContent.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                Debug.Log($"  - ContentSizeFitter: verticalFit={contentSizeFitter.verticalFit}");
            }
        }
        
        Debug.Log("EnhancePanelController: UIシステム健全性チェック完了");
    }

    /// <summary>
    /// 武器一覧を表示する
    /// </summary>
    private void PopulateWeaponList()
    {
        Debug.Log("=== EnhancePanelController: PopulateWeaponList開始 ===");
        
        // 1. 基本参照確認
        Debug.Log($"artifactInventory: {artifactInventory != null}");
        Debug.Log($"weaponItemDisplayPrefab: {weaponItemDisplayPrefab != null}");
        Debug.Log($"weaponListContent: {weaponListContent != null}");
        
        if (artifactInventory == null)
        {
            Debug.LogError("❌ ArtifactInventory が null です");
            return;
        }
        
        if (weaponItemDisplayPrefab == null)
        {
            Debug.LogError("❌ weaponItemDisplayPrefab が null です");
            return;
        }
        
        if (weaponListContent == null)
        {
            Debug.LogError("❌ weaponListContent が null です");
            return;
        }
        
        // 2. 既存のアイテムをクリア
        Debug.Log($"既存子オブジェクト数: {weaponListContent.childCount}");
        foreach (Transform child in weaponListContent)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        
        // 3. 武器データ取得
        var ownedWeapons = artifactInventory.GetAllOwnedWeapons();
        Debug.Log($"✅ 所持武器数: {ownedWeapons.Count}");
        
        if (ownedWeapons.Count == 0)
        {
            Debug.LogWarning("⚠️ 所持武器が0個です。F2キーでテスト武器を追加してください。");
            return;
        }
        
        // 4. プレハブの詳細確認
        var prefabButton = weaponItemDisplayPrefab.GetComponent<Button>();
        var prefabScript = weaponItemDisplayPrefab.GetComponent<WeaponEnhanceItemDisplay>();
        Debug.Log($"プレハブにButton: {prefabButton != null}");
        Debug.Log($"プレハブにWeaponEnhanceItemDisplay: {prefabScript != null}");
        
        // 追加：プレハブの詳細情報を出力
        Debug.Log($"プレハブ名: {weaponItemDisplayPrefab.name}");
        var allComponents = weaponItemDisplayPrefab.GetComponents<Component>();
        Debug.Log($"プレハブの全コンポーネント:");
        foreach (var comp in allComponents)
        {
            Debug.Log($"  - {comp.GetType().Name}");
        }
        
        if (prefabButton == null)
        {
            Debug.LogError("❌ プレハブにButtonコンポーネントがありません！");
            return;
        }
        
        if (prefabScript == null)
        {
            Debug.LogError("❌ プレハブにWeaponEnhanceItemDisplayスクリプトがありません！");
            Debug.LogError("プレハブのスクリプトをWeaponEnhanceItemDisplayに変更してください。");
            return;
        }
        
        // 5. 武器ボタン生成
        weaponButtonDisplays.Clear();
        selectedWeaponIndex = 0;
        
        int successCount = 0;
        foreach (var weapon in ownedWeapons)
        {
            if (weapon == null) 
            {
                Debug.LogWarning("⚠️ null武器をスキップ");
                continue;
            }
            
            Debug.Log($"🔧 武器ボタン生成開始: {weapon.weaponName}");
            
            // プレハブを生成
            GameObject weaponDisplayObj = Instantiate(weaponItemDisplayPrefab, weaponListContent);
            
            // 確実にアクティブにする
            weaponDisplayObj.SetActive(true);
            weaponDisplayObj.name = $"WeaponButton_{weapon.weaponName}";
            
            Debug.Log($"生成したオブジェクト: {weaponDisplayObj.name}, Active: {weaponDisplayObj.activeInHierarchy}");
            
            // コンポーネント確認
            var displayScript = weaponDisplayObj.GetComponent<WeaponEnhanceItemDisplay>();
            var button = weaponDisplayObj.GetComponent<Button>();
            var image = weaponDisplayObj.GetComponent<Image>();
            
            Debug.Log($"  - WeaponEnhanceItemDisplay: {displayScript != null}");
            Debug.Log($"  - Button: {button != null}");
            Debug.Log($"  - Image: {image != null}");
            
            // 追加：生成されたオブジェクトの全コンポーネントを確認
            Debug.Log($"生成されたオブジェクト '{weaponDisplayObj.name}' の全コンポーネント:");
            var generatedComponents = weaponDisplayObj.GetComponents<Component>();
            foreach (var comp in generatedComponents)
            {
                Debug.Log($"    - {comp.GetType().Name}");
            }
            
            if (displayScript != null)
            {
                Debug.Log($"🎯 Setup実行: {weapon.weaponName}");
                
                // 武器データを設定
                displayScript.Setup(weapon, OnWeaponSelected);
                
                // 強化表示も更新
                displayScript.RefreshEnhancementDisplay();
                
                // ボタン状態確認
                if (button != null)
                {
                    Debug.Log($"  - Button enabled: {button.enabled}");
                    Debug.Log($"  - Button interactable: {button.interactable}");
                    
                    // 強制的に有効化
                    button.enabled = true;
                    button.interactable = true;
                    
                    if (image != null)
                    {
                        image.raycastTarget = true;
                        Debug.Log($"  - Image raycastTarget: {image.raycastTarget}");
                    }
                }
                
                weaponButtonDisplays.Add(displayScript);
                successCount++;
                Debug.Log($"✅ 武器ボタン生成完了: {weapon.weaponName}");
            }
            else
            {
                Debug.LogError($"❌ WeaponEnhanceItemDisplayコンポーネントが見つかりません: {weapon.weaponName}");
                Debug.LogError($"プレハブ '{weaponItemDisplayPrefab.name}' にWeaponEnhanceItemDisplayスクリプトが付いていません！");
                Debug.LogError("プレハブのスクリプトをWeaponEnhanceItemDisplayに変更してください。");
                Destroy(weaponDisplayObj);
            }
        }
        
        Debug.Log($"=== 武器ボタン生成完了。成功数: {successCount}/{ownedWeapons.Count} ===");
        UpdateWeaponSelectionHighlight();
        
        // 6. 最終確認
        Debug.Log($"Content子オブジェクト数: {weaponListContent.childCount}");
        for (int i = 0; i < weaponListContent.childCount; i++)
        {
            var child = weaponListContent.GetChild(i);
            var childButton = child.GetComponent<Button>();
            Debug.Log($"  子{i}: {child.name}, Button: {childButton != null}, Active: {child.gameObject.activeInHierarchy}");
        }
    }
    
    /// <summary>
    /// 武器が選択されたときの処理
    /// </summary>
    public void OnWeaponSelected(WeaponItem selectedWeapon)
    {
        Debug.Log($"EnhancePanelController: 武器が選択されました: {selectedWeapon.weaponName}");
        
        if (detailPanel != null)
        {
            detailPanel.Open(selectedWeapon);
            Debug.Log("詳細パネルを表示しました");
        }
        else
        {
            Debug.LogError("detailPanel が null です");
        }
    }

    private void Update()
    {
        // テスト用：F5キーで手動クリックテスト
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("F5キー押下 - ボタンテスト開始");
            TestButtonClick();
        }
        
        // テスト用：F6キーでパネルの状態確認
        if (Input.GetKeyDown(KeyCode.F6))
        {
            Debug.Log("F6キー押下 - パネル状態確認");
            CheckUIStatus();
        }
        
        // テスト用：F8キーで強化パネルを強制アクティブ化
        if (Input.GetKeyDown(KeyCode.F8))
        {
            Debug.Log("F8キー押下 - 強化パネル強制アクティブ化");
            gameObject.SetActive(true);
        }

        if (weaponButtonDisplays.Count > 0)
        {
            // 下キー
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedWeaponIndex = (selectedWeaponIndex + 1) % weaponButtonDisplays.Count;
                UpdateWeaponSelectionHighlight();
            }
            // 上キー
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedWeaponIndex = (selectedWeaponIndex - 1 + weaponButtonDisplays.Count) % weaponButtonDisplays.Count;
                UpdateWeaponSelectionHighlight();
            }
            // Enterキー
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                var selected = weaponButtonDisplays[selectedWeaponIndex];
                if (selected != null)
                {
                    Debug.Log($"[キーボード選択] {selected.name} を詳細表示");
                    selected.OnClickSelectWeaponByKeyboard();
                }
            }
        }
    }
    
    private void CheckUIStatus()
    {
        Debug.Log("=== UI状態確認 ===");
        Debug.Log($"EnhancePanel isActive: {gameObject.activeInHierarchy}");
        Debug.Log($"weaponListContent: {weaponListContent != null}");
        Debug.Log($"weaponItemDisplayPrefab: {weaponItemDisplayPrefab != null}");
        
        if (weaponListContent != null)
        {
            Debug.Log($"weaponListContent子オブジェクト数: {weaponListContent.childCount}");
            
            for (int i = 0; i < weaponListContent.childCount; i++)
            {
                var child = weaponListContent.GetChild(i);
                var button = child.GetComponent<Button>();
                Debug.Log($"  子オブジェクト{i}: {child.name}, Button: {button != null}, Active: {child.gameObject.activeInHierarchy}");
                if (button != null)
                {
                    Debug.Log($"    Button interactable: {button.interactable}");
                }
            }
        }
        
        // EventSystemの確認
        var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        Debug.Log($"EventSystem found: {eventSystem != null}");
        
        // GraphicRaycasterの確認
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            Debug.Log($"GraphicRaycaster found: {raycaster != null}");
        }
    }
    
    private void TestButtonClick()
    {
        if (ownedWeapons.Count > 0)
        {
            var testWeapon = ownedWeapons[0];
            Debug.Log($"手動テスト: {testWeapon.weaponName} の詳細を開きます");
            OnWeaponSelected(testWeapon);
        }
        else
        {
            Debug.Log("テスト用武器がありません");
        }
    }
    
    private void ForceAddTestWeapons()
    {
        if (artifactInventory == null)
        {
            Debug.LogError("ArtifactInventoryが見つかりません");
            return;
        }
        
        // Resourcesから武器を検索して追加
        var allWeapons = Resources.FindObjectsOfTypeAll<WeaponItem>();
        Debug.Log($"検索された武器数: {allWeapons.Length}");
        
        foreach (var weapon in allWeapons)
        {
            if (weapon != null)
            {
                artifactInventory.AddWeapon(weapon, 1);
                Debug.Log($"武器追加: {weapon.weaponName}");
            }
        }
        
        // リストを再更新
        PopulateWeaponList();
    }

    private void UpdateWeaponSelectionHighlight()
    {
        for (int i = 0; i < weaponButtonDisplays.Count; i++)
        {
            weaponButtonDisplays[i].SetHighlight(i == selectedWeaponIndex);
        }
    }
}