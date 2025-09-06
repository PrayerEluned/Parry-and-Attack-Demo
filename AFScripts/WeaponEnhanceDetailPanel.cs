using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WeaponEnhanceDetailPanel : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text statBeforeText;
    [SerializeField] private TMP_Text statAfterText;
    [SerializeField] private Transform materialListContent;
    [SerializeField] private GameObject materialDisplayPrefab;
    [SerializeField] private Button enhanceButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text notEnoughMaterialText;
    [SerializeField] private ArtifactInventory artifactInventory;
    [SerializeField] private WeaponManager weaponManager; // 武器管理
    [SerializeField] private WeaponEnhanceProcessor enhanceProcessor; // 強化処理

    private WeaponItem currentWeapon;
    private bool canEnhance = false;

    public void Open(WeaponItem weapon)
    {
        currentWeapon = weapon;
        
        // 強化プロセッサーの参照を確保
        if (enhanceProcessor == null)
            enhanceProcessor = FindObjectOfType<WeaponEnhanceProcessor>();
        
        // UI更新
        if (weaponIcon != null) weaponIcon.sprite = weapon.icon;
        if (weaponNameText != null) weaponNameText.text = weapon.weaponName;
        
        // 現在の強化レベルを取得
        int currentEnhanceLevel = enhanceProcessor != null ? enhanceProcessor.GetWeaponEnhanceLevel(weapon) : 0;
        
        // 能力値表示（基礎値 + 強化値表示）
        if (statBeforeText != null)
        {
            statBeforeText.text = GenerateStatText(weapon, currentEnhanceLevel, false);
        }
        
        if (statAfterText != null)
        {
            statAfterText.text = GenerateStatText(weapon, currentEnhanceLevel + 1, true);
        }
        
        // 必要素材リスト更新
        PopulateMaterialList();
        
        // 強化可能性チェック
        CheckCanEnhance();
        
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
        
        if (enhanceButton != null)
            enhanceButton.onClick.AddListener(TryEnhance);
    }

    void PopulateMaterialList()
    {
        Debug.Log("=== PopulateMaterialList開始 ===");
        
        // null チェック
        if (materialListContent == null)
        {
            Debug.LogError("WeaponEnhanceDetailPanel: materialListContentが設定されていません！");
            return;
        }
        
        if (materialDisplayPrefab == null)
        {
            Debug.LogError("WeaponEnhanceDetailPanel: materialDisplayPrefabが設定されていません！");
            return;
        }
        
        if (currentWeapon == null)
        {
            Debug.LogError("WeaponEnhanceDetailPanel: currentWeaponがnullです！");
            return;
        }
        
        if (currentWeapon.enhancementMaterials == null)
        {
            Debug.LogWarning("WeaponEnhanceDetailPanel: enhancementMaterialsがnullです。空のリストとして処理します");
            return;
        }
        
        Debug.Log($"必要素材数: {currentWeapon.enhancementMaterials.Count}");

        // 既存の素材表示を削除
        foreach (Transform child in materialListContent)
            Destroy(child.gameObject);

        // 必要素材を表示
        foreach (var req in currentWeapon.enhancementMaterials)
        {
            if (req == null)
            {
                Debug.LogWarning("WeaponEnhanceDetailPanel: null の素材要求をスキップします");
                continue;
            }
            
            if (req.material == null)
            {
                Debug.LogWarning("WeaponEnhanceDetailPanel: 素材がnullの要求をスキップします");
                continue;
            }
            
            Debug.Log($"素材表示作成中: {req.material.itemName}");
            
            var display = Instantiate(materialDisplayPrefab, materialListContent);
            var materialDisplay = display.GetComponent<EnhanceMaterialDisplay>();
            
            if (materialDisplay == null)
            {
                Debug.LogError($"WeaponEnhanceDetailPanel: EnhanceMaterialDisplayコンポーネントが見つかりません");
                continue;
            }
            
            int ownedAmount = artifactInventory != null ? artifactInventory.GetConsumableCount(req.material) : 0;
            materialDisplay.Setup(req.material.icon, req.material.itemName, req.requiredAmount, ownedAmount);
            
            Debug.Log($"素材表示完了: {req.material.itemName} (必要:{req.requiredAmount}, 所持:{ownedAmount})");
        }
        
        Debug.Log("=== PopulateMaterialList完了 ===");
    }

    void CheckCanEnhance()
    {
        // WeaponEnhanceProcessorを使用
        if (enhanceProcessor != null)
        {
            canEnhance = enhanceProcessor.CanEnhanceWeapon(currentWeapon);
        }
        else
        {
            canEnhance = false;
            Debug.LogError("WeaponEnhanceProcessor が見つかりません！");
        }
        
        // UI更新
        if (enhanceButton != null)
            enhanceButton.interactable = canEnhance;
        
        if (notEnoughMaterialText != null)
            notEnoughMaterialText.gameObject.SetActive(!canEnhance);
            
        Debug.Log($"強化可能性チェック: {canEnhance} (武器: {currentWeapon?.weaponName})");
    }

    void TryEnhance()
    {
        Debug.Log($"TryEnhance開始: canEnhance={canEnhance}, enhanceProcessor={enhanceProcessor != null}");
        
        if (!canEnhance) 
        {
            Debug.LogWarning("強化できません。素材が足りないか、条件を満たしていません。");
            return;
        }
        
        if (enhanceProcessor == null)
        {
            Debug.LogError("WeaponEnhanceProcessor が見つかりません！");
            return;
        }
        
        // WeaponEnhanceProcessorを使用して強化実行
        bool success = enhanceProcessor.EnhanceWeapon(currentWeapon);
        
        if (success)
        {
            Debug.Log($"🎉 {currentWeapon.weaponName} の強化が成功しました！");
            
            // WeaponManagerの現在装備武器も更新（同期）
            if (weaponManager != null && weaponManager.currentWeapon != null && 
                weaponManager.currentWeapon.weaponItem == currentWeapon)
            {
                int newLevel = enhanceProcessor.GetWeaponEnhanceLevel(currentWeapon);
                weaponManager.currentWeapon.enhancementLevel = newLevel;
                Debug.Log($"WeaponManagerの強化レベルも同期: {newLevel}");
            }
            
            // UIを再表示（強化後の値を反映）
            Open(currentWeapon);
            
            // 武器リストの強化表示も更新
            RefreshWeaponListEnhancementDisplay();
        }
        else
        {
            Debug.LogError("強化に失敗しました！");
        }
    }
    
    /// <summary>
    /// 武器リストの強化表示を更新
    /// </summary>
    private void RefreshWeaponListEnhancementDisplay()
    {
        // EnhancePanelControllerを探して武器リストの強化表示を更新
        var enhancePanelController = FindObjectOfType<EnhancePanelController>();
        if (enhancePanelController != null)
        {
            // 武器リスト内のすべてのWeaponEnhanceItemDisplayを更新
            var weaponDisplays = FindObjectsOfType<WeaponEnhanceItemDisplay>();
            foreach (var display in weaponDisplays)
            {
                display.RefreshEnhancementDisplay();
            }
            Debug.Log("武器リストの強化表示を更新しました");
        }
        else
        {
            Debug.LogWarning("EnhancePanelControllerが見つかりません");
        }
    }

    private string GenerateStatText(WeaponItem weapon, int enhanceLevel, bool isAfter)
    {
        var statParts = new System.Collections.Generic.List<string>();
        
        // 攻撃力（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusAttack > 0 || (weapon.enhanceAttack > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusAttack;
            if (isAfter && weapon.enhanceAttack > 0)
            {
                // After status：現在のステータス値 + (追加される強化値)
                float currentValue = baseValue + weapon.enhanceAttack * (enhanceLevel - 1);
                float additionalEnhance = weapon.enhanceAttack;
                statParts.Add($"ATK: {currentValue:F0} <color=green>(+{additionalEnhance:F0})</color>");
            }
            else
            {
                float totalValue = baseValue + weapon.enhanceAttack * enhanceLevel;
                statParts.Add($"ATK: {totalValue:F0}");
            }
        }

        // HP（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusHP > 0 || (weapon.enhanceHP > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusHP;
            if (isAfter && weapon.enhanceHP > 0)
            {
                // After status：現在のステータス値 + (追加される強化値)
                float currentValue = baseValue + weapon.enhanceHP * (enhanceLevel - 1);
                float additionalEnhance = weapon.enhanceHP;
                statParts.Add($"HP: {currentValue:F0} <color=green>(+{additionalEnhance:F0})</color>");
            }
            else
            {
                float totalValue = baseValue + weapon.enhanceHP * enhanceLevel;
                statParts.Add($"HP: {totalValue:F0}");
            }
        }

        // 防御力（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusDefense > 0 || (weapon.enhanceDefense > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusDefense;
            if (isAfter && weapon.enhanceDefense > 0)
            {
                // After status：現在のステータス値 + (追加される強化値)
                float currentValue = baseValue + weapon.enhanceDefense * (enhanceLevel - 1);
                float additionalEnhance = weapon.enhanceDefense;
                statParts.Add($"DEF: {currentValue:F0} <color=green>(+{additionalEnhance:F0})</color>");
            }
            else
            {
                float totalValue = baseValue + weapon.enhanceDefense * enhanceLevel;
                statParts.Add($"DEF: {totalValue:F0}");
            }
        }

        // 魔法攻撃力（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusMagicAttack > 0 || (weapon.enhanceMagicAttack > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusMagicAttack;
            if (isAfter && weapon.enhanceMagicAttack > 0)
            {
                // After status：現在のステータス値 + (追加される強化値)
                float currentValue = baseValue + weapon.enhanceMagicAttack * (enhanceLevel - 1);
                float additionalEnhance = weapon.enhanceMagicAttack;
                statParts.Add($"MAT: {currentValue:F0} <color=green>(+{additionalEnhance:F0})</color>");
            }
            else
            {
                float totalValue = baseValue + weapon.enhanceMagicAttack * enhanceLevel;
                statParts.Add($"MAT: {totalValue:F0}");
            }
        }

        // 魔法防御力（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusMagicDefense > 0 || (weapon.enhanceMagicDefense > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusMagicDefense;
            if (isAfter && weapon.enhanceMagicDefense > 0)
            {
                // After status：現在のステータス値 + (追加される強化値)
                float currentValue = baseValue + weapon.enhanceMagicDefense * (enhanceLevel - 1);
                float additionalEnhance = weapon.enhanceMagicDefense;
                statParts.Add($"MDF: {currentValue:F0} <color=green>(+{additionalEnhance:F0})</color>");
            }
            else
            {
                float totalValue = baseValue + weapon.enhanceMagicDefense * enhanceLevel;
                statParts.Add($"MDF: {totalValue:F0}");
            }
        }

        // 運（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusFate > 0 || (weapon.enhanceFate > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusFate;
            if (isAfter && weapon.enhanceFate > 0)
            {
                // After status：現在のステータス値 + (追加される強化値)
                float currentValue = baseValue + weapon.enhanceFate * (enhanceLevel - 1);
                float additionalEnhance = weapon.enhanceFate;
                statParts.Add($"運: {currentValue:F0} <color=green>(+{additionalEnhance:F0})</color>");
            }
            else
            {
                float totalValue = baseValue + weapon.enhanceFate * enhanceLevel;
                statParts.Add($"運: {totalValue:F0}");
            }
        }

        // 効果がない場合
        if (statParts.Count == 0)
        {
            return "効果なし";
        }

        // 空白で結合
        return string.Join(" ", statParts);
    }
}