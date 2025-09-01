using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerStats : MonoBehaviour
{
    [Header("��{�X�e�[�^�X�f�[�^")]
    public CharacterStats stats = new CharacterStats();

    [Header("���݂̏��")]
    public int CurrentHP { get; private set; }
    public int CurrentEXP { get; private set; }
    public int RequiredEXP { get; private set; }
    public int Level { get; private set; } = 1;
    public int StatPoints { get; private set; } = 10;

    [Header("�o���l�ݒ�")]
    [SerializeField] private int baseRequiredEXP = 50;
    [SerializeField] private float expGrowthRate = 1.2f;

    [Header("���x���A�b�v�ݒ�")]
    [SerializeField] private int statPointsPerLevel = 5;
    [SerializeField] private float hpGrowthPerLevel = 1.1f;

    private WeaponManager weaponManager;

    private static PlayerStats instance;
    public static PlayerStats Instance => instance;

    private void Awake()
    {
        // Unity 6フリーズ対策情報
        Debug.Log("PlayerStats: 軽量化モードで動作中");
        
        if (instance != null && instance != this)
        {
            // Debug.LogWarning("PlayerStats: 重複インスタンス検出。新しいインスタンスは即座に破棄します。");
            // string path = gameObject.name;
            // Transform t = transform.parent;
            // while (t != null) {
            //     path = t.name + "/" + path;
            //     t = t.parent;
            // }
            // Debug.Log($"[PlayerStats] Awake: 階層: {path}");
            // Debug.Log($"[PlayerStats] Start() 実行 (InstanceID: {GetInstanceID()})");
            // Debug.Log("[PlayerStats] デフォルト武器（拳）を装備しました。");
            // Debug.LogError($"[PlayerStats] weaponManagerがnullです！ (GameObject: {gameObject.name})");
            // Debug.LogWarning("デフォルト武器（拳）が見つかりませんでした！ Resources/Weapons/Fist にありますか？");
            // Debug.Log($"[PlayerStats] 最大HP変動: {prevMaxHP}→{newMaxHP}、CurrentHP割合補正: {CurrentHP}");
            // Debug.Log($"[PlayerStats] ApplyAllEffects後: CurrentHP={CurrentHP}, TotalHP={stats.TotalHP}");
            // Debug.Log($"[PlayerStats] SPDデバッグ: Speedpoint={stats.Speedpoint}, TotalSpeed={stats.TotalSpeed}, speedMultiplier={stats.speedMultiplier}, baseSpeed={stats.baseSpeed}, equipSpeed={stats.equipSpeed}, ad");
            // Debug.Log($"xAbvĨ݂x: {Level}, ̕KvEXP: {RequiredEXP},gp\\Xe[^X|Cg: {StatPoints}");
            // Debug.LogWarning("Xe[^X|Cg܂");
            // Debug.LogError("ȃXe[^X^Cvł");
            // Debug.Log($"{statType}{amount}|CgU蕪܂Bc̃Xe[^X|Cg: {StatPoints}");
            // Debug.LogError("[PlayerStats] ArtifactInventoryがnullです！同じGameObjectにアタッチされていますか？");
            // Debug.Log($"[PlayerStats] artifactInventoryインスタンスID: {inventory.GetInstanceID()}");
            // Debug.Log($"[PlayerStats] 所持AF: {string.Join(", ", ownedArtifacts.Select(a => a != null ? a.artifactName : "null"))}");
            // Debug.Log($"[PlayerStats] AF効果 {st}: 加算={add}, 乗算={mul}");
            // Debug.Log("TakeDamage: ゲーム停止中なのでダメージ無効");
            // Debug.Log($"HP{amount}񕜂܂B݂HP: {CurrentHP}/{Mathf.RoundToInt(stats.TotalHP)}");
            // Debug.Log("vC[͓|ꂽI");
            // Debug.LogWarning($"[PlayerStats] OnDisable: オブジェクトが非アクティブになりました (InstanceID: {GetInstanceID()})。階層: {path}");
            // Debug.Log(System.Environment.StackTrace);
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        Debug.Log("PlayerStats: 基本機能復活テスト - PlayerStats初期化を開始");
        
        try
        {
            // 基本的なコンポーネント取得
            weaponManager = GetComponent<WeaponManager>();
            stats.artifactInventory = GetComponent<ArtifactInventory>();
            
            // 基本値の設定
            CurrentHP = Mathf.RoundToInt(stats.TotalHP);
            RequiredEXP = Mathf.RoundToInt(baseRequiredEXP);
            CurrentEXP = 0;
            
            // デフォルト武器「拳」をセット
            if (weaponManager != null && weaponManager.currentWeapon == null)
            {
                var defaultWeapon = Resources.Load<WeaponItem>("Weapons/こぶし");
                if (defaultWeapon != null)
                {
                    // インベントリに追加してから装備（整合性を保つため）
                    if (stats.artifactInventory != null)
                    {
                        // 既に持っているかチェック（重複防止）
                        if (stats.artifactInventory.GetWeaponCount(defaultWeapon) == 0)
                        {
                            stats.artifactInventory.AddItem(defaultWeapon);
                            Debug.Log("PlayerStats: デフォルト武器「こぶし」をインベントリに追加");
                        }
                        else
                        {
                            Debug.Log("PlayerStats: デフォルト武器「こぶし」は既にインベントリにあります");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("PlayerStats: ArtifactInventoryが見つかりませんが、武器装備は続行します");
                    }
                    
                    weaponManager.EquipWeapon(defaultWeapon);
                    Debug.Log("PlayerStats: デフォルト武器「こぶし」を装備");
                }
                else
                {
                    Debug.LogWarning("PlayerStats: デフォルト武器「こぶし」が見つかりません！Resources/Weapons/こぶし を確認してください");
                }
            }
            
            Debug.Log("PlayerStats: 基本機能復活完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerStats: 初期化エラー - {e.Message}");
        }
    }


    public void ApplyAllEffects()
    {
        // ステータス再計算前のHPを保存
        float prevMaxHP = stats.TotalHP;
        int prevCurrentHP = CurrentHP;

        // ステータスリセット
        stats.equipHP = 0;
        stats.equipAttack = 0;
        stats.equipDefense = 0;
        stats.equipMagicAttack = 0;
        stats.equipMagicDefense = 0;
        stats.equipSpeed = 0;
        stats.equipFate = 0;

        stats.hpMultiplier = 1f;
        stats.attackMultiplier = 1f;
        stats.defenseMultiplier = 1f;
        stats.magicAttackMultiplier = 1f;
        stats.magicDefenseMultiplier = 1f;
        stats.speedMultiplier = 1f;
        stats.fateMultiplier = 1f;

        // Artifact効果適用
        ApplyArtifactEffects();

        // 武器効果適用
        if (weaponManager == null) weaponManager = GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            stats.equipHP += weaponManager.GetTotalHP();
            stats.equipAttack += weaponManager.GetTotalAttack();
            stats.equipMagicAttack += weaponManager.GetTotalMagicAttack();
            stats.equipDefense += weaponManager.GetTotalDefense();
            stats.equipMagicDefense += weaponManager.GetTotalMagicDefense();
            stats.equipFate += weaponManager.GetTotalFate();
        }
        // HP再計算
        float newMaxHP = stats.TotalHP;
        if (Mathf.Abs(newMaxHP - prevMaxHP) > 0.01f && prevMaxHP > 0)
        {
            // 最大HPが変化した場合のみ割合補正
            float ratio = prevCurrentHP / (float)prevMaxHP;
            CurrentHP = Mathf.RoundToInt(newMaxHP * ratio);
            // Debug.Log($"[PlayerStats] 最大HP変動: {prevMaxHP}→{newMaxHP}、CurrentHP割合補正: {CurrentHP}");
        }
        else
        {
            // 変化なしなら現在のHPを維持（全回復を防ぐ）
            CurrentHP = prevCurrentHP;
        }
        // Debug.Log($"[PlayerStats] ApplyAllEffects後: CurrentHP={CurrentHP}, TotalHP={stats.TotalHP}");
        // Debug.Log($"[PlayerStats] SPDデバッグ: Speedpoint={stats.Speedpoint}, TotalSpeed={stats.TotalSpeed}, speedMultiplier={stats.speedMultiplier}, baseSpeed={stats.baseSpeed}, equipSpeed={stats.equipSpeed}, additionalSpeed={stats.additionalSpeed}");
    }


    public void GainEXP(int amount)
    {
        // 特殊効果パッチによる経験値倍率を適用
        var specialEffect = GetComponent<SpecialPatchEffect>();
        if (specialEffect != null)
        {
            float expBonus = specialEffect.GetExpBonus();
            if (expBonus > 0f)
            {
                amount = Mathf.RoundToInt(amount * (1f + expBonus / 100f));
                Debug.Log($"特殊効果パッチ: 経験値倍率 +{expBonus}% 適用。獲得経験値: {amount}");
            }
        }
        
        CurrentEXP += amount;
        
        // 無限ループ対策：レベルアップ回数制限
        int levelUpCount = 0;
        const int MAX_LEVEL_UP_PER_CALL = 10; // 一度に最大10レベルアップまで
        
        while (CurrentEXP >= RequiredEXP && levelUpCount < MAX_LEVEL_UP_PER_CALL)
        {
            CurrentEXP -= RequiredEXP;
            LevelUp();
            levelUpCount++;
        }
        
        // 上限に達した場合の警告
        if (levelUpCount >= MAX_LEVEL_UP_PER_CALL && CurrentEXP >= RequiredEXP)
        {
            Debug.LogWarning($"PlayerStats: レベルアップ回数制限に達しました。残りEXP: {CurrentEXP}");
        }
    }

    public float MoveSpeed => stats != null ? stats.MoveSpeed : 1.0f;

    private void LevelUp()
    {
        Level++;
        StatPoints += statPointsPerLevel;
        RequiredEXP = Mathf.RoundToInt(baseRequiredEXP * Mathf.Pow(expGrowthRate, Level - 1));
        stats.baseHP *= hpGrowthPerLevel;
        ApplyAllEffects();
        CurrentHP = Mathf.RoundToInt(stats.TotalHP);
        // Debug.Log($"xAbvĨ݂x: {Level}, ̕KvEXP: {RequiredEXP},gp\\Xe[^X|Cg: {StatPoints}");
    }

    public void UpdateHP()
    {
        // 現在のHPを維持（全回復を防ぐ）
        // CurrentHP = Mathf.RoundToInt(stats.TotalHP); // この行を削除
    }

    public bool AllocateStatPoint(StatType statType, int amount = 1)
    {
        if (StatPoints < amount)
        {
            // Debug.LogWarning("Xe[^X|Cg܂");
            return false;
        }

        switch (statType)
        {
            case StatType.HP:
                stats.additionalHP += 10 * amount;
                break;
            case StatType.Attack:
                stats.additionalAttack += 1 * amount;
                break;
            case StatType.Defense:
                stats.additionalDefense += 1 * amount;
                break;
            case StatType.MagicAttack:
                stats.additionalMagicAttack += 1 * amount;
                break;
            case StatType.MagicDefense:
                stats.additionalMagicDefense += 1 * amount;
                break;
            case StatType.Speed:
                stats.additionalSpeed += 1 * amount;
                break;
            case StatType.Fate:
                stats.additionalFate += 1 * amount;
                break;
            default:
                // Debug.LogError("ȃXe[^X^Cvł");
                return false;
        }

        StatPoints -= amount;
        ApplyAllEffects();
        // CurrentHP = Mathf.RoundToInt(stats.TotalHP); // この行を削除（体力全回復を防ぐ）
        // Debug.Log($"{statType}{amount}|CgU蕪܂Bc̃Xe[^X|Cg: {StatPoints}");
        return true;
    }

    public void ApplyArtifactEffects()
    {
        ArtifactInventory inventory = ArtifactInventory.Instance ?? GetComponent<ArtifactInventory>();
        if (inventory == null)
        {
            // Debug.LogError("[PlayerStats] ArtifactInventoryがnullです！同じGameObjectにアタッチされていますか？");
            return;
        }
        // Debug.Log($"[PlayerStats] artifactInventoryインスタンスID: {inventory.GetInstanceID()}");

        // ここで1回だけ取得
        var ownedArtifacts = inventory.GetAllOwnedArtifacts();
        // Debug.Log($"[PlayerStats] 所持AF: {string.Join(", ", ownedArtifacts.Select(a => a != null ? a.artifactName : "null"))}");

        foreach (StatType st in System.Enum.GetValues(typeof(StatType)))
        {
            float add = inventory.GetTotalAdditiveFromList(st, ownedArtifacts);
            float mul = inventory.GetTotalMultiplierFromList(st, ownedArtifacts);
            // Debug.Log($"[PlayerStats] AF効果 {st}: 加算={add}, 乗算={mul}");
        }
        stats.equipHP = inventory.GetTotalAdditiveFromList(StatType.HP, ownedArtifacts);
        stats.equipAttack = inventory.GetTotalAdditiveFromList(StatType.Attack, ownedArtifacts);
        stats.equipDefense = inventory.GetTotalAdditiveFromList(StatType.Defense, ownedArtifacts);
        stats.equipMagicAttack = inventory.GetTotalAdditiveFromList(StatType.MagicAttack, ownedArtifacts);
        stats.equipMagicDefense = inventory.GetTotalAdditiveFromList(StatType.MagicDefense, ownedArtifacts);
        stats.equipSpeed = inventory.GetTotalAdditiveFromList(StatType.Speed, ownedArtifacts);
        stats.equipFate = inventory.GetTotalAdditiveFromList(StatType.Fate, ownedArtifacts);
        stats.hpMultiplier = inventory.GetTotalMultiplierFromList(StatType.HP, ownedArtifacts);
        stats.attackMultiplier = inventory.GetTotalMultiplierFromList(StatType.Attack, ownedArtifacts);
        stats.defenseMultiplier = inventory.GetTotalMultiplierFromList(StatType.Defense, ownedArtifacts);
        stats.magicAttackMultiplier = inventory.GetTotalMultiplierFromList(StatType.MagicAttack, ownedArtifacts);
        stats.magicDefenseMultiplier = inventory.GetTotalMultiplierFromList(StatType.MagicDefense, ownedArtifacts);
        stats.speedMultiplier = inventory.GetTotalMultiplierFromList(StatType.Speed, ownedArtifacts);
        stats.fateMultiplier = inventory.GetTotalMultiplierFromList(StatType.Fate, ownedArtifacts);
    }

    public void TakeDamage(int damage, EnemyHealth attacker = null)
    {
        if (Time.timeScale == 0f)
        {
            // Debug.Log("TakeDamage: ゲーム停止中なのでダメージ無効");
            return;
        }
        
        float finalDamage = damage;
        
        // 防御システムによるダメージ処理
        var defenseController = GetComponent<DefenseController>();
        if (defenseController == null)
        {
            defenseController = GetComponentInChildren<DefenseController>();
        }
        if (defenseController == null)
        {
            defenseController = FindFirstObjectByType<DefenseController>();
        }
        
        if (defenseController != null)
        {
            finalDamage = defenseController.ProcessIncomingDamage(damage, attacker);
            Debug.Log($"PlayerStats: ダメージ処理 {damage} -> {finalDamage} (防御中: {defenseController.IsDefending}, attacker: {attacker})");
        }
        else
        {
            Debug.LogWarning("DefenseControllerが見つかりません！");
        }
        
        CurrentHP -= Mathf.RoundToInt(finalDamage);
        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Die();
        }
        
        // ダメージテキスト表示（ダメージを受けた時の位置に固定）
        Vector3 damagePosition = transform.position + Vector3.up * 0.8f; // オフセットを少し大きくする
        
        var damageTextComponent = GetComponent<DamageTextComponent>();
        if (damageTextComponent != null)
        {
            // DamageTextComponentの位置を一時的に設定
            damageTextComponent.damageTextOffset = Vector3.up * 0.8f; // オフセットを少し大きくする
            damageTextComponent.OnTakeDamage(Mathf.RoundToInt(finalDamage));
        }
        else
        {
            // DamageTextComponentがない場合は直接マネージャーを使用
            DamageTextManager.ShowDamageAt(damagePosition, Mathf.RoundToInt(finalDamage));
        }
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, Mathf.RoundToInt(stats.TotalHP));
        // Debug.Log($"HP{amount}񕜂܂B݂HP: {CurrentHP}/{Mathf.RoundToInt(stats.TotalHP)}");
    }

    private void Die()
    {
        // Debug.Log("vC[͓|ꂽI");
    }

    public float GetHPPercent()
    {
        return CurrentHP / stats.TotalHP;
    }

    public float GetEXPPercent()
    {
        return (float)CurrentEXP / RequiredEXP;
    }
    public List<EnhancePatch> GetAllOwnedPatches()
    {
        ArtifactInventory inventory = GetComponent<ArtifactInventory>();
        return inventory != null ? inventory.GetAllOwnedPatches() : new List<EnhancePatch>();
    }



    public int GetTotalPatchSlotCount()
    {
        ArtifactInventory inventory = GetComponent<ArtifactInventory>();
        if (inventory == null) return 3; // {3Xbg

        return 3 + inventory.GetPatchSlotExpandCount();
    }

    private void OnDisable()
    {
        // string path = gameObject.name;
        // Transform t = transform.parent;
        // while (t != null) {
        //     path = t.name + "/" + path;
        //     t = t.parent;
        // }
        // Debug.LogWarning($"[PlayerStats] OnDisable: オブジェクトが非アクティブになりました (InstanceID: {GetInstanceID()})。階層: {path}");
        // Debug.Log(System.Environment.StackTrace);
    }



}

public enum StatType
{
    HP,
    Attack,
    Defense,
    MagicAttack,
    MagicDefense,
    Speed,
    Fate,
    ExpGain // 経験値倍率
}
