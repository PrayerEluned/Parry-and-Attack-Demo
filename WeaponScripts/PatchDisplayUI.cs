using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PatchDisplayUI : MonoBehaviour
{
    [Header("UI要素")]
    public Image patchIcon;
    public TextMeshProUGUI patchNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI enhanceCostText;
    
    [Header("強化ボタン")]
    public Button enhanceButton;
    public TextMeshProUGUI enhanceButtonText;
    
    [Header("効果タイプ表示")]
    public GameObject normalEffectPanel;
    public GameObject specialEffectPanel;
    public TextMeshProUGUI effectTypeText;
    
    private EnhancePatch currentPatch;
    private PatchEnhancementManager enhancementManager;
    
    void Start()
    {
        enhancementManager = FindObjectOfType<PatchEnhancementManager>();
        if (enhancementManager == null)
        {
            enhancementManager = gameObject.AddComponent<PatchEnhancementManager>();
        }
        
        if (enhanceButton != null)
        {
            enhanceButton.onClick.AddListener(OnEnhanceButtonClicked);
        }
    }
    
    // パッチの表示を更新
    public void UpdatePatchDisplay(EnhancePatch patch)
    {
        currentPatch = patch;
        
        if (patch == null)
        {
            ClearDisplay();
            return;
        }
        
        // 基本情報の表示
        if (patchIcon != null && patch.icon != null)
        {
            patchIcon.sprite = patch.icon;
        }
        
        if (patchNameText != null)
        {
            patchNameText.text = patch.patchName;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = patch.description;
        }
        
        // 効果の表示
        if (effectText != null)
        {
            effectText.text = patch.GetEffectDescription();
        }
        
        // レベル表示
        if (levelText != null)
        {
            levelText.text = $"レベル: {patch.patchLevel}/{patch.maxEnhanceLevel}";
        }
        
        // 効果タイプの表示
        UpdateEffectTypeDisplay(patch);
        
        // 強化ボタンの更新
        UpdateEnhanceButton(patch);
        
        // 強化コストの表示
        if (enhanceCostText != null && enhancementManager != null)
        {
            enhanceCostText.text = enhancementManager.GetEnhancementCost(patch);
        }
    }
    
    // 効果タイプの表示を更新
    private void UpdateEffectTypeDisplay(EnhancePatch patch)
    {
        if (normalEffectPanel != null)
        {
            normalEffectPanel.SetActive(!patch.isSpecialEffect);
        }
        
        if (specialEffectPanel != null)
        {
            specialEffectPanel.SetActive(patch.isSpecialEffect);
        }
        
        if (effectTypeText != null)
        {
            if (patch.isSpecialEffect)
            {
                effectTypeText.text = $"特殊効果: {patch.specialEffectType}";
            }
            else
            {
                effectTypeText.text = $"通常効果: {patch.effectType}";
            }
        }
    }
    
    // 強化ボタンの更新
    private void UpdateEnhanceButton(EnhancePatch patch)
    {
        if (enhanceButton == null || enhanceButtonText == null) return;
        
        bool canEnhance = false;
        string buttonText = "強化不可";
        
        if (patch != null)
        {
            if (enhancementManager != null)
            {
                canEnhance = enhancementManager.CanEnhancePatch(patch);
            }
            else
            {
                canEnhance = patch.canEnhance && patch.patchLevel < patch.maxEnhanceLevel;
            }
            
            if (canEnhance)
            {
                buttonText = $"強化 (Lv.{patch.patchLevel + 1})";
            }
            else if (!patch.canEnhance)
            {
                buttonText = "強化不可";
            }
            else if (patch.patchLevel >= patch.maxEnhanceLevel)
            {
                buttonText = "最大レベル";
            }
            else
            {
                buttonText = "素材不足";
            }
        }
        
        enhanceButton.interactable = canEnhance;
        enhanceButtonText.text = buttonText;
    }
    
    // 強化ボタンクリック時の処理
    private void OnEnhanceButtonClicked()
    {
        if (currentPatch == null || enhancementManager == null) return;
        
        bool success = enhancementManager.EnhancePatch(currentPatch);
        if (success)
        {
            // 表示を更新
            UpdatePatchDisplay(currentPatch);
            
            // 成功メッセージを表示
            Debug.Log($"パッチ {currentPatch.patchName} の強化に成功しました！");
        }
        else
        {
            // 失敗メッセージを表示
            Debug.LogWarning("パッチの強化に失敗しました。素材を確認してください。");
        }
    }
    
    // 表示をクリア
    private void ClearDisplay()
    {
        if (patchIcon != null) patchIcon.sprite = null;
        if (patchNameText != null) patchNameText.text = "";
        if (descriptionText != null) descriptionText.text = "";
        if (effectText != null) effectText.text = "";
        if (levelText != null) levelText.text = "";
        if (enhanceCostText != null) enhanceCostText.text = "";
        if (effectTypeText != null) effectTypeText.text = "";
        
        if (enhanceButton != null) enhanceButton.interactable = false;
        if (enhanceButtonText != null) enhanceButtonText.text = "強化不可";
    }
    
    // 特殊効果の詳細説明を取得
    public string GetSpecialEffectDescription(EnhancePatch patch)
    {
        if (patch == null || !patch.isSpecialEffect) return "";
        
        switch (patch.specialEffectType)
        {
            case SpecialEffectType.HPToAttack:
                return $"HPを{10 + 2 * patch.patchLevel}%減らして{patch.affectedStat}に{(10 + 2 * patch.patchLevel) * 2}追加";
                
            case SpecialEffectType.FixedStatsWithParry:
                return "攻撃、防御、魔法攻撃、魔法防御を10固定。パリィ時に敵の最大HPの1.5%ダメージ";
                
            case SpecialEffectType.CounterAttack:
                return $"防御中、敵に攻撃されると防御力の{10 + 2 * patch.patchLevel}%ダメージを返す";
                
            case SpecialEffectType.AutoHeal:
                return $"3秒おきに最大HPの{1 + 0.2f * patch.patchLevel}%回復";
                
            case SpecialEffectType.ExpBonus:
                return $"経験値獲得量 +{patch.patchLevel}%";
                
            case SpecialEffectType.EnemyHPReduce:
                return $"敵の体力を{patch.patchLevel}%減らす";
                
            case SpecialEffectType.AFDropRate:
                return "AFのドロップ率を1.5倍にする";
                
            case SpecialEffectType.MaterialDropRate:
                return "素材のドロップ量を2倍にする";
                
            default:
                return "効果なし";
        }
    }
} 