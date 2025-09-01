using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("�������̕���iEquipWeaponData�^�j")]
    public EquipWeaponData currentWeapon;

    [Header("�������̋����p�b�`�i�ő�3�X���b�g�j")]
    public EnhancePatch[] enhancePatches = new EnhancePatch[3];

    private static WeaponManager instance;
    public static WeaponManager Instance => instance;

    public static EnhancePatch NonePatchRef;

    private void Awake()
    {
        // Unity 6フリーズ対策情報
        Debug.Log("WeaponManager: 軽量化モードで動作中");
        
        if (instance != null && instance != this)
        {
            // Debug.LogWarning("WeaponManager: 重複インスタンス検出。新しいインスタンスは即座に破棄します。");
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        // NonePatchで初期化（null参照防止）
        if (NonePatchRef != null)
        {
            for (int i = 0; i < enhancePatches.Length; i++)
            {
                enhancePatches[i] = NonePatchRef;
            }
        }
    }

    public void EquipWeapon(WeaponItem weaponItem)
    {
        Debug.Log($"[WeaponManager] 武器装備開始: {weaponItem.weaponName}");
        
        // インベントリに武器があるかチェック（安全性確認）
        var artifactInventory = FindObjectOfType<ArtifactInventory>();
        if (artifactInventory != null && artifactInventory.GetWeaponCount(weaponItem) == 0)
        {
            Debug.LogWarning($"[WeaponManager] 警告: 装備しようとした武器「{weaponItem.weaponName}」がインベントリにありません！");
            Debug.LogWarning("[WeaponManager] 初期化処理でインベントリに追加されているかご確認ください");
        }
        
        currentWeapon = new EquipWeaponData(weaponItem);
        
        // WeaponEnhanceProcessorから強化レベルを同期
        var enhanceProcessor = FindObjectOfType<WeaponEnhanceProcessor>();
        if (enhanceProcessor != null)
        {
            int enhanceLevel = enhanceProcessor.GetWeaponEnhanceLevel(weaponItem);
            currentWeapon.enhancementLevel = enhanceLevel;
            
            Debug.Log($"[WeaponManager] 武器装備完了: {weaponItem.weaponName}");
            Debug.Log($"  - 強化レベル: {enhanceLevel}");
            Debug.Log($"  - 基礎攻撃力: {weaponItem.bonusAttack}");
            Debug.Log($"  - 強化攻撃力: {weaponItem.enhanceAttack} × {enhanceLevel} = {weaponItem.enhanceAttack * enhanceLevel}");
            Debug.Log($"  - 総攻撃力: {currentWeapon.GetAddAttack()}");
        }
        else
        {
            Debug.LogWarning("[WeaponManager] WeaponEnhanceProcessorが見つかりません。強化レベルは0に設定されます。");
            currentWeapon.enhancementLevel = 0;
        }
    }

    // ---yCzpb`XbgAFx[XŌvZ ---
    public int GetMaxPatchSlots()
    {
        PlayerStats playerStats = GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            return Mathf.Clamp(playerStats.GetTotalPatchSlotCount(), 0, enhancePatches.Length);
        }
        else
        {
            return 1; // PlayerStatsȂ1g
        }
    }

    public float GetTotalHP()
    {
        if (currentWeapon == null || currentWeapon.weaponItem == null)
            return 0f;

        // EquipWeaponDataの強化値を含むHPを使用
        float baseHP = currentWeapon.GetAddHP();
        float additive = 0f;
        float multiplier = 1f;

        for (int i = 0; i < GetMaxPatchSlots(); i++)
        {
            var patch = enhancePatches[i];
            if (patch == null || patch == NonePatchRef || patch.affectedStat != "HP") continue;

            switch (patch.effectType)
            {
                case PatchEffectType.Additive:
                    additive += patch.GetEffectiveValue();
                    break;
                case PatchEffectType.Multiplicative:
                    multiplier *= patch.GetEffectiveValue();
                    break;
            }
        }

        return (baseHP + additive) * multiplier;
    }

    public float GetTotalAttack()
    {
        // EquipWeaponDataの強化値を含む攻撃力を使用
        float baseAttack = currentWeapon != null ? currentWeapon.GetAddAttack() : 0f;
        float additive = 0f;
        float multiplier = 1f;

        Debug.Log($"[WeaponManager] GetTotalAttack開始");
        Debug.Log($"  - 武器の強化済み攻撃力: {baseAttack}");
        if (currentWeapon != null)
        {
            Debug.Log($"  - 基礎攻撃力: {currentWeapon.weaponItem.bonusAttack}");
            Debug.Log($"  - 強化レベル: {currentWeapon.enhancementLevel}");
            Debug.Log($"  - 強化攻撃力: {currentWeapon.weaponItem.enhanceAttack} × {currentWeapon.enhancementLevel} = {currentWeapon.weaponItem.enhanceAttack * currentWeapon.enhancementLevel}");
        }

        for (int i = 0; i < GetMaxPatchSlots(); i++)
        {
            var patch = enhancePatches[i];
            if (patch == null || patch == NonePatchRef || patch.affectedStat != "Attack") continue;

            switch (patch.effectType)
            {
                case PatchEffectType.Additive:
                    additive += patch.GetEffectiveValue();
                    Debug.Log($"  - パッチ加算: +{patch.GetEffectiveValue()}");
                    break;
                case PatchEffectType.Multiplicative:
                    multiplier *= patch.GetEffectiveValue();
                    Debug.Log($"  - パッチ乗算: ×{patch.GetEffectiveValue()}");
                    break;
            }
        }

        float totalAttack = (baseAttack + additive) * multiplier;
        Debug.Log($"  - 最終攻撃力: ({baseAttack} + {additive}) × {multiplier} = {totalAttack}");
        return totalAttack;
    }

    public float GetTotalDefense()
    {
        // EquipWeaponDataの強化値を含む防御力を使用
        float baseDefense = currentWeapon != null ? currentWeapon.GetAddDefense() : 0f;
        float additive = 0f;
        float multiplier = 1f;

        for (int i = 0; i < GetMaxPatchSlots(); i++)
        {
            var patch = enhancePatches[i];
            if (patch == null || patch == NonePatchRef || patch.affectedStat != "Defense") continue;

            switch (patch.effectType)
            {
                case PatchEffectType.Additive:
                    additive += patch.GetEffectiveValue();
                    break;
                case PatchEffectType.Multiplicative:
                    multiplier *= patch.GetEffectiveValue();
                    break;
            }
        }

        return (baseDefense + additive) * multiplier;
    }

    public float GetTotalMagicAttack()
    {
        // EquipWeaponDataの強化値を含む魔法攻撃力を使用
        float baseMagicAttack = currentWeapon != null ? currentWeapon.GetAddMagicAttack() : 0f;
        float additive = 0f;
        float multiplier = 1f;

        for (int i = 0; i < GetMaxPatchSlots(); i++)
        {
            var patch = enhancePatches[i];
            if (patch == null || patch == NonePatchRef || patch.affectedStat != "MagicAttack") continue;

            switch (patch.effectType)
            {
                case PatchEffectType.Additive:
                    additive += patch.GetEffectiveValue();
                    break;
                case PatchEffectType.Multiplicative:
                    multiplier *= patch.GetEffectiveValue();
                    break;
            }
        }

        return (baseMagicAttack + additive) * multiplier;
    }

    public float GetTotalMagicDefense()
    {
        // EquipWeaponDataの強化値を含む魔法防御力を使用
        float baseMagicDefense = currentWeapon != null ? currentWeapon.GetAddMagicDefense() : 0f;
        float additive = 0f;
        float multiplier = 1f;

        for (int i = 0; i < GetMaxPatchSlots(); i++)
        {
            var patch = enhancePatches[i];
            if (patch == null || patch == NonePatchRef || patch.affectedStat != "MagicDefense") continue;

            switch (patch.effectType)
            {
                case PatchEffectType.Additive:
                    additive += patch.GetEffectiveValue();
                    break;
                case PatchEffectType.Multiplicative:
                    multiplier *= patch.GetEffectiveValue();
                    break;
            }
        }

        return (baseMagicDefense + additive) * multiplier;
    }

    public float GetTotalFate()
    {
        // EquipWeaponDataの強化値を含む運を使用
        float baseFate = currentWeapon != null ? currentWeapon.GetAddFate() : 0f;
        float additive = 0f;
        float multiplier = 1f;

        for (int i = 0; i < GetMaxPatchSlots(); i++)
        {
            var patch = enhancePatches[i];
            if (patch == null || patch == NonePatchRef || patch.affectedStat != "Fate") continue;

            switch (patch.effectType)
            {
                case PatchEffectType.Additive:
                    additive += patch.GetEffectiveValue();
                    break;
                case PatchEffectType.Multiplicative:
                    multiplier *= patch.GetEffectiveValue();
                    break;
            }
        }

        return (baseFate + additive) * multiplier;
    }

    public float GetTotalReachMultiplier()
    {
        float reach = 1f;
        for (int i = 0; i < GetMaxPatchSlots(); i++)
        {
            var patch = enhancePatches[i];
            if (patch == null || patch == NonePatchRef) continue;

            if (patch.effectType == PatchEffectType.ExtendReach)
                reach += patch.GetEffectiveValue();
        }
        return reach;
    }

    public bool HasLifesteal(out float rate)
    {
        rate = 0f;
        for (int i = 0; i < GetMaxPatchSlots(); i++)
        {
            var patch = enhancePatches[i];
            if (patch == null || patch == NonePatchRef) continue;

            if (patch.effectType == PatchEffectType.Lifesteal)
            {
                rate = Mathf.Max(rate, patch.GetEffectiveValue());
            }
        }
        return rate > 0f;
    }

    public EnhancePatch[] GetEnhancePatches()
    {
        return enhancePatches;
    }
}
