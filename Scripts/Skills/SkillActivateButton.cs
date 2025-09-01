using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillActivateButton : MonoBehaviour
{
    [SerializeField] private int slotIndex; // 0,1,2
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI cooldownText; // クールタイム表示用

    // === 外部からのアクセス用 ===
    public int SlotIndex => slotIndex;
    
    // === キャッシュ化された参照（フリーズ対策） ===
    private static SkillController cachedSkillController;
    
    private Color normalColor = Color.white;
    private Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f); // 暗くする色

    private SkillController skillController;

    private void Awake()
    {
        Debug.Log($"SkillActivateButton: スキルシステム解禁 - 初期化を開始 (slot={slotIndex})");
        
        try
        {
            // 基本的なコンポーネント取得
            skillController = Object.FindFirstObjectByType<SkillController>();
            if (skillController == null)
            {
                Debug.LogWarning("SkillActivateButton: SkillControllerが見つかりません");
            }
            
            // ボタンクリックイベントの設定
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
            
            Debug.Log($"SkillActivateButton: 初期化完了 (slot={slotIndex})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SkillActivateButton: 初期化エラー - {e.Message}");
        }
    }

    private void Update()
    {
        try
        {
            // 軽量なUpdate処理
            if (skillController != null)
            {
                // スキルの使用可能状態をチェック（クールダウンが0なら使用可能）
                float remainCooldown = skillController.GetRemainCooldown(slotIndex);
                bool canUseSkill = remainCooldown <= 0f;
                
                // 安全なボタンアクセス
                if (button != null)
                {
                    button.interactable = canUseSkill;
                }
                
                // アイコンとクールダウン表示を更新
                UpdateIconAndCooldown();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SkillActivateButton: Update エラー - {e.Message}");
        }
    }

    public void UpdateIconAndCooldown()
    {
        try
        {
            if (skillController != null && skillController.slots != null && 
                slotIndex >= 0 && slotIndex < skillController.slots.Length)
            {
                var skill = skillController.slots[slotIndex];
                
                // アイコンの更新
                if (iconImage != null)
                {
                    iconImage.sprite = skill ? skill.icon : null;
                }

                float remain = skillController.GetRemainCooldown(slotIndex);

                if (remain > 0.01f)
                {
                    if (iconImage != null)
                        iconImage.color = cooldownColor;
                    if (cooldownText != null)
                        cooldownText.text = Mathf.CeilToInt(remain).ToString();
                }
                else
                {
                    if (iconImage != null)
                        iconImage.color = normalColor;
                    if (cooldownText != null)
                        cooldownText.text = "";
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SkillActivateButton: UpdateIconAndCooldown エラー - {e.Message}");
        }
    }

    private void OnClick()
    {
        try
        {
            Debug.Log($"SkillActivateButton: クリック - slot={slotIndex}");
            if (skillController != null)
                skillController.TryUseSkill(slotIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SkillActivateButton: OnClick エラー - {e.Message}");
        }
    }
} 