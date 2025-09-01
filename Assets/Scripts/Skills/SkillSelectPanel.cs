using UnityEngine;
using System.Collections.Generic;

public class SkillSelectPanel : MonoBehaviour
{
    [SerializeField] private Transform contentParent; // ScrollViewのContent
    [SerializeField] private SkillItemDisplay itemPrefab;
    [SerializeField] private List<SkillData> skillList; // InspectorでSOをセット

    [SerializeField] private GameObject mainUI;
    [SerializeField] private GameObject thisPanel;
    [SerializeField] private GameObject openButton; // スキル選択パネルを開くボタン
    [SerializeField] private GameObject closeButton; // スキル選択パネルを閉じるボタン

    private int selectingSlotIndex = -1;
    [SerializeField] private List<SkillSlotUI> skillSlots;
    
    // === スキル反映のための参照 ===
    private SkillController skillController;
    private SkillActivateButton[] skillActivateButtons;

    private void Start()
    {
        Debug.Log("SkillSelectPanel: スキルシステム解禁 - 初期化を開始");
        
        try
        {
            // 基本的なスキル選択パネルの初期化
            InitializeSkillPanel();
            Debug.Log("SkillSelectPanel: 初期化完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SkillSelectPanel: 初期化エラー - {e.Message}");
        }
    }
    
    private void InitializeSkillPanel()
    {
        // スキル選択パネルの基本初期化
        if (contentParent != null)
        {
            Debug.Log("SkillSelectPanel: スキルリストコンテンツ初期化");
            
            // スキルリストを生成
            Populate();
            
            // スキル発動処理を渡す
            for (int i = 0; i < skillSlots.Count; i++)
            {
                skillSlots[i].Setup(i, OnSkillUse);
            }
        }
        
        // Unity 6対応：安全な参照取得
        try
        {
            // SkillControllerとSkillActivateButtonの参照を取得
            skillController = Object.FindFirstObjectByType<SkillController>();
            if (skillController != null)
            {
                Debug.Log("SkillSelectPanel: SkillController参照取得完了");
            }
            else
            {
                Debug.LogWarning("SkillSelectPanel: SkillControllerが見つかりません");
            }
            
            // SkillActivateButtonの参照を安全に取得
            skillActivateButtons = Object.FindObjectsByType<SkillActivateButton>(FindObjectsSortMode.None);
            Debug.Log($"SkillSelectPanel: SkillActivateButton {skillActivateButtons.Length}個検出");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SkillSelectPanel: 参照取得エラー - {e.Message}");
            // フォールバック処理
            skillController = null;
            skillActivateButtons = new SkillActivateButton[0];
        }
    }

    public void Populate()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var skill in skillList)
        {
            var item = Instantiate(itemPrefab, contentParent);
            item.Setup(skill, this);
        }
    }

    // スキル発動処理
    private void OnSkillUse(SkillData skill, int slotIndex)
    {
        Debug.Log($"スキル発動: {skill?.skillName} (slot {slotIndex})");
        // SkillController.Instance.UseSkill(slotIndex); などに置き換えてください
    }

    // スキル選択パネルを開く専用ボタン用
    public void OpenSkillSelectPanel(int slotIndex)
    {
        selectingSlotIndex = slotIndex;
        thisPanel.SetActive(true);
        if (mainUI) mainUI.SetActive(false);
        if (openButton) openButton.SetActive(false);
        if (closeButton) closeButton.SetActive(true);
        
        Debug.Log($"SkillSelectPanel: スキル選択パネル開始 - slotIndex={slotIndex}");
    }

    public void OnSkillSelected(SkillData selected)
    {
        Debug.Log($"SkillSelectPanel: スキル選択 - {selected?.skillName} (slot {selectingSlotIndex})");
        
        if (selectingSlotIndex >= 0 && selectingSlotIndex < skillSlots.Count)
        {
            // 1. SkillSlotUIを更新
            skillSlots[selectingSlotIndex].SetSkill(selected);
            
            // 2. SkillControllerのslotsを更新
            if (skillController != null)
            {
                skillController.SetSkill(selectingSlotIndex, selected);
                Debug.Log($"SkillSelectPanel: SkillController更新完了 - {selected?.skillName}");
            }
            else
            {
                Debug.LogWarning("SkillSelectPanel: SkillControllerがnullです");
            }
            
            // 3. 対応するSkillActivateButtonのアイコンを更新
            UpdateSkillActivateButton(selectingSlotIndex);
        }
        
        // パネルを閉じる
        thisPanel.SetActive(false);
        if (mainUI) mainUI.SetActive(true);
        if (openButton) openButton.SetActive(true);
        if (closeButton) closeButton.SetActive(false);
    }
    
    private void UpdateSkillActivateButton(int slotIndex)
    {
        if (skillActivateButtons != null)
        {
            foreach (var button in skillActivateButtons)
            {
                // slotIndexが一致するボタンを見つけて更新
                if (button != null && button.SlotIndex == slotIndex)
                {
                    button.UpdateIconAndCooldown();
                    Debug.Log($"SkillSelectPanel: SkillActivateButton更新完了 - slot={slotIndex}");
                    break;
                }
            }
        }
    }

    public void OnCloseButton()
    {
        thisPanel.SetActive(false);
        if (mainUI) mainUI.SetActive(true);
        if (openButton) openButton.SetActive(true);
        if (closeButton) closeButton.SetActive(false);
    }
} 