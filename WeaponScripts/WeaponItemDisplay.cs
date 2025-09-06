using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponItemDisplay : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText; // 説明文用（新規追加）
    [SerializeField] private TMP_Text effectText;       // 効果文用（自動生成されるステータス）
    
    [Header("強化値表示用")]
    [SerializeField] private TMP_Text enhancementLevelText; // 強化レベル表示用（例: "+3"）
    [SerializeField] private GameObject enhancementIndicator; // 強化アイコンや背景（オプション）

    public WeaponItem weaponData;
    private UIManager uiManager;
    private Button button;

    public void Setup(WeaponItem weapon, UIManager manager, bool isSelected)
    {
        weaponData = weapon;
        uiManager = manager;

        // 武器の情報表示
        if (iconImage != null) iconImage.sprite = weapon.icon;
        if (nameText != null) nameText.text = weapon.weaponName;
        
        // 説明文は固定のテキストを表示
        if (descriptionText != null) 
        {
            descriptionText.text = weapon.description; // SOの説明文を使用
        }
        
        // 効果文は自動生成されるステータス表示（強化値込み）
        if (effectText != null) 
        {
            effectText.text = GenerateEffectText(weapon);
        }

        // 強化レベル表示を更新
        UpdateEnhancementDisplay();

        // 選択中ならハイライト（例えば色を変える）
        if (isSelected)
            GetComponent<Image>().color = new Color(0.35f, 0.5f, 0.35f);
        else
            GetComponent<Image>().color = new Color(0.231f, 0.231f, 0.231f);

        // ボタン登録
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners(); // 前のを削除
            button.onClick.AddListener(OnClickSelectWeapon);
        }
    }

    private void OnClickSelectWeapon()
    {
        if (uiManager != null)
        {
            uiManager.OnWeaponSelected(weaponData);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (GetComponent<Image>() != null)
        {
            if (isSelected)
                GetComponent<Image>().color = new Color(0.35f, 0.5f, 0.35f);
            else
                GetComponent<Image>().color = new Color(0.231f, 0.231f, 0.231f);
        }
    }
    
    /// <summary>
    /// 強化レベル表示を更新
    /// </summary>
    private void UpdateEnhancementDisplay()
    {
        Debug.Log($"[WeaponItemDisplay] UpdateEnhancementDisplay開始: {weaponData?.weaponName}");
        
        // WeaponEnhanceProcessorから強化レベルを取得
        var enhanceProcessor = FindObjectOfType<WeaponEnhanceProcessor>();
        int enhanceLevel = 0;
        
        if (enhanceProcessor != null && weaponData != null)
        {
            enhanceLevel = enhanceProcessor.GetWeaponEnhanceLevel(weaponData);
            Debug.Log($"  - 取得した強化レベル: {enhanceLevel}");
        }
        else
        {
            Debug.LogWarning($"  - enhanceProcessor: {enhanceProcessor != null}, weaponData: {weaponData != null}");
        }
        
        // 強化レベルテキスト表示
        if (enhancementLevelText != null)
        {
            if (enhanceLevel > 0)
            {
                enhancementLevelText.text = $"+{enhanceLevel}";
                enhancementLevelText.color = new Color(0.412f, 0.690f, 1.0f, 1.0f); // 強化済みは#69B0FF
                enhancementLevelText.gameObject.SetActive(true); // GameObject を表示
                Debug.Log($"  - 強化レベルテキスト表示: +{enhanceLevel}");
            }
            else
            {
                enhancementLevelText.text = "";
                enhancementLevelText.gameObject.SetActive(false); // GameObject を非表示
                Debug.Log($"  - 強化レベルテキスト非表示");
            }
        }
        else
        {
            Debug.LogWarning($"  - enhancementLevelText が null です（Prefabで設定されていません）");
        }
        
        // 強化インジケーター表示（オプション）
        if (enhancementIndicator != null)
        {
            // 安全性チェック：enhancementIndicatorが自分自身のGameObjectでないことを確認
            if (enhancementIndicator == gameObject)
            {
                Debug.LogError($"[重要] WeaponItemDisplay: enhancementIndicatorが自分自身のGameObjectに設定されています！無限ループを防ぐためスキップします。武器: {weaponData?.weaponName}");
                Debug.LogError("[解決方法] Prefabのインスペクターで enhancementIndicator を None または子オブジェクトに設定してください");
                // enhancementIndicatorの制御を完全にスキップ
            }
            else
            {
                enhancementIndicator.SetActive(enhanceLevel > 0);
                Debug.Log($"  - enhancementIndicator: {enhanceLevel > 0}");
            }
        }
        
        Debug.Log($"[WeaponItemDisplay] UpdateEnhancementDisplay完了: {weaponData?.weaponName}");
    }
    
    /// <summary>
    /// 強化表示を手動で更新（外部から呼び出し可能）
    /// </summary>
    public void RefreshEnhancementDisplay()
    {
        UpdateEnhancementDisplay();
        
        // 効果文も再生成
        if (effectText != null && weaponData != null)
        {
            effectText.text = GenerateEffectText(weaponData);
        }
    }

    /// <summary>
    /// 効果文を自動生成（ATK + 15(+6)形式）
    /// </summary>
    private string GenerateEffectText(WeaponItem weapon)
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
}
