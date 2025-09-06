using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIDebugHelper : MonoBehaviour
{
    [ContextMenu("UIシステム全体チェック")]
    public void CheckUISystem()
    {
        Debug.Log("=== UIシステム全体チェック開始 ===");
        
        // 1. EventSystem確認
        CheckEventSystem();
        
        // 2. すべてのCanvas確認
        CheckAllCanvases();
        
        // 3. 強化パネル専用チェック
        CheckEnhancePanelUI();
        
        Debug.Log("=== UIシステム全体チェック完了 ===");
    }
    
    [ContextMenu("EventSystem確認・修正")]
    public void CheckEventSystem()
    {
        Debug.Log("--- EventSystem確認 ---");
        
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("❌ EventSystemが見つかりません！新しく作成します。");
            CreateEventSystem();
        }
        else
        {
            Debug.Log("✅ EventSystem存在: " + eventSystem.name);
            Debug.Log("  現在選択中オブジェクト: " + eventSystem.currentSelectedGameObject);
            
            // StandaloneInputModule確認
            var inputModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (inputModule == null)
            {
                Debug.LogWarning("⚠️ StandaloneInputModuleがありません。追加します。");
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
            else
            {
                Debug.Log("✅ StandaloneInputModule存在");
            }
        }
    }
    
    [ContextMenu("全Canvas確認・修正")]
    public void CheckAllCanvases()
    {
        Debug.Log("--- Canvas確認 ---");
        
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        if (canvases.Length == 0)
        {
            Debug.LogError("❌ Canvasが見つかりません！");
            return;
        }
        
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            Debug.Log($"Canvas {i}: {canvas.name}");
            Debug.Log($"  Render Mode: {canvas.renderMode}");
            Debug.Log($"  Sorting Order: {canvas.sortingOrder}");
            Debug.Log($"  Active: {canvas.gameObject.activeInHierarchy}");
            
            // GraphicRaycaster確認
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogWarning($"⚠️ Canvas '{canvas.name}' にGraphicRaycasterがありません。追加します。");
                raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
            else
            {
                Debug.Log($"✅ GraphicRaycaster存在: Enabled={raycaster.enabled}");
            }
            
            // CanvasGroupチェック
            CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                Debug.Log($"  CanvasGroup: Alpha={canvasGroup.alpha}, Interactable={canvasGroup.interactable}, BlockRaycasts={canvasGroup.blocksRaycasts}");
                if (!canvasGroup.interactable)
                {
                    Debug.LogWarning($"⚠️ Canvas '{canvas.name}' のCanvasGroupがInteractable=falseです！");
                }
                if (!canvasGroup.blocksRaycasts)
                {
                    Debug.LogWarning($"⚠️ Canvas '{canvas.name}' のCanvasGroupがBlockRaycasts=falseです！");
                }
            }
        }
    }
    
    [ContextMenu("強化パネル専用チェック")]
    public void CheckEnhancePanelUI()
    {
        Debug.Log("--- 強化パネル専用チェック ---");
        
        // EnhancePanelControllerを探す
        EnhancePanelController enhanceController = FindFirstObjectByType<EnhancePanelController>();
        if (enhanceController == null)
        {
            Debug.LogError("❌ EnhancePanelControllerが見つかりません！");
            return;
        }
        
        Debug.Log($"✅ EnhancePanelController発見: {enhanceController.name}");
        
        // Contentオブジェクトを確認
        Transform contentTransform = enhanceController.transform.Find("Scroll View/Viewport/Content");
        if (contentTransform == null)
        {
            Debug.LogError("❌ Content Transformが見つかりません！");
            return;
        }
        
        Debug.Log($"✅ Content Transform発見: {contentTransform.name}");
        Debug.Log($"  子オブジェクト数: {contentTransform.childCount}");
        
        // 各子オブジェクト（武器ボタン）をチェック
        for (int i = 0; i < contentTransform.childCount; i++)
        {
            Transform child = contentTransform.GetChild(i);
            Debug.Log($"  子 {i}: {child.name} (Active: {child.gameObject.activeInHierarchy})");
            
            // Buttonコンポーネント確認
            Button button = child.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"    ⚠️ Buttonコンポーネントなし");
            }
            else
            {
                Debug.Log($"    ✅ Button: Enabled={button.enabled}, Interactable={button.interactable}");
                Debug.Log($"    リスナー数: {button.onClick.GetPersistentEventCount()}");
                
                // WeaponEnhanceItemDisplayも確認
                WeaponEnhanceItemDisplay itemDisplay = child.GetComponent<WeaponEnhanceItemDisplay>();
                if (itemDisplay != null)
                {
                    Debug.Log($"    ✅ WeaponEnhanceItemDisplay存在");
                }
            }
            
            // CanvasGroupチェック
            CanvasGroup childCanvasGroup = child.GetComponent<CanvasGroup>();
            if (childCanvasGroup != null)
            {
                Debug.Log($"    CanvasGroup: Alpha={childCanvasGroup.alpha}, Interactable={childCanvasGroup.interactable}");
            }
        }
    }
    
    [ContextMenu("ボタン強制修正")]
    public void ForceFixButtons()
    {
        Debug.Log("=== ボタン強制修正開始 ===");
        
        // 全てのButtonを取得
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        
        for (int i = 0; i < allButtons.Length; i++)
        {
            Button button = allButtons[i];
            
            // 基本設定を確実にする
            button.enabled = true;
            button.interactable = true;
            
            // 親のCanvasGroupも確認
            CanvasGroup parentGroup = button.GetComponentInParent<CanvasGroup>();
            if (parentGroup != null)
            {
                parentGroup.interactable = true;
                parentGroup.blocksRaycasts = true;
                parentGroup.alpha = Mathf.Max(parentGroup.alpha, 0.1f); // 最低限見える
            }
            
            Debug.Log($"ボタン修正: {button.name} (Parent: {button.transform.parent?.name})");
        }
        
        Debug.Log("=== ボタン強制修正完了 ===");
    }
    
    private void CreateEventSystem()
    {
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();
        Debug.Log("✅ 新しいEventSystemを作成しました");
    }
} 