using UnityEngine;

[System.Serializable]
public class CharacterStats
{
    // �����x�[�X�X�e�[�^�X
    public float baseHP = 100;
    public float baseAttack = 10;
    public float baseDefense = 5;
    public float baseMagicAttack = 8;
    public float baseMagicDefense = 4;
    public float baseSpeed = 1.0f;
    public float baseFate = 0.01f; // 1%���A�h���b�v��

    // �X�e�[�^�X�|�C���g����U��p�̒ǉ��l
    public float additionalHP = 0;
    public float additionalAttack = 0;
    public float additionalDefense = 0;
    public float additionalMagicAttack = 0;
    public float additionalMagicDefense = 0;
    public float additionalSpeed = 0;
    public float additionalFate = 0;

    // ������o�t����̉��Z�l
    public float equipHP = 0;
    public float equipAttack = 0;
    public float equipDefense = 0;
    public float equipMagicAttack = 0;
    public float equipMagicDefense = 0;
    public float equipSpeed = 0;
    public float equipFate = 0;

    // アーティファクト・パッチ・武器由来の乗算値
    public float hpMultiplier = 1.0f;
    public float attackMultiplier = 1.0f;
    public float defenseMultiplier = 1.0f;
    public float magicAttackMultiplier = 1.0f;
    public float magicDefenseMultiplier = 1.0f;
    public float speedMultiplier = 1.0f;
    public float fateMultiplier = 1.0f;
    public float expGainMultiplier = 1.0f; // パッチ由来の経験値倍率

    //A[eBt@Ngʂ擾邽߂̎Q
    public ArtifactInventory artifactInventory;

    //XP[O萔
    private const float SpeedScale = 0.25f;
    private const float FateScale = 0.01f;

    // ステータス計算式を厳密に修正
    public float TotalHP => Mathf.Max(1f, (baseHP + additionalHP + equipHP) * Mathf.Max(0.01f, hpMultiplier));
    public float TotalAttack => (baseAttack + additionalAttack + equipAttack) * Mathf.Max(0.01f, attackMultiplier);
    public float TotalDefense => (baseDefense + additionalDefense + equipDefense) * Mathf.Max(0.01f, defenseMultiplier);
    public float TotalMagicAttack => (baseMagicAttack + additionalMagicAttack + equipMagicAttack) * Mathf.Max(0.01f, magicAttackMultiplier);
    public float TotalMagicDefense => (baseMagicDefense + additionalMagicDefense + equipMagicDefense) * Mathf.Max(0.01f, magicDefenseMultiplier);
    public float TotalSpeed => SpeedScale * (baseSpeed + additionalSpeed + equipSpeed) * Mathf.Max(0.01f, speedMultiplier);
    public float TotalFate => FateScale * (baseFate + additionalFate + equipFate) * Mathf.Max(0.01f, fateMultiplier);

    // 経験値倍率は最低0.0f（マイナス値防止）
    public float TotalExpGainMultiplier => Mathf.Max(0.0f, expGainMultiplier);

    public float Speedpoint => (baseSpeed + additionalSpeed + equipSpeed) * Mathf.Max(0.01f, speedMultiplier);
    public float Fatepoint => (baseFate + additionalFate + equipFate) * Mathf.Max(0.01f, fateMultiplier);

    // ړxvZ
    public float MoveSpeed
    {
        get
        {
            float ratio = Mathf.Min(Speedpoint, 1000) / 1000f;
            return 5.0f + (20.0f * ratio);
        }
    }

    // ꎞIȃ{[iX/fot
    public float temporaryHPBonus = 0;
    public float temporaryAttackBonus = 0;
    public float temporaryDefenseBonus = 0;
    public float temporaryMagicAttackBonus = 0;
    public float temporaryMagicDefenseBonus = 0;
    public float temporarySpeedBonus = 0;
    public float temporaryFateBonus = 0;

    //A[eBt@NgʂZ
    private float GetAFAdd(StatType stat)
    {
        return artifactInventory != null ? artifactInventory.GetTotalAdditive(stat) : 0f;
    }

    //A[eBt@NgʂZ
    private float GetAFMult(StatType stat)
    {
        return artifactInventory != null ? artifactInventory.GetTotalMultiplier(stat) : 1f;
    }
}
