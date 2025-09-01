using UnityEngine;

/// <summary>
/// すべての Skill プレハブに実装させる共通窓口
/// </summary>
public interface ISkillBehaviour
{
    /// <summary>オーナーと設定データを渡して初期化</summary>
    void Init(GameObject owner, SkillData data);

    /// <summary>発動に要する合計秒数（SkillController が待つ）</summary>
    float Duration { get; }
}
