using UnityEngine;
using Game.Items;

public enum SkillShape
{
    NoneSkill,         // 何もしない
    HorizontalSlash,   // 横斬り
    VerticalPierce,    // 縦突き
    RoundSpin          // 回転
}

[CreateAssetMenu(menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject, IPlayerItem
{
    public string skillName;         // スキル名
    public Sprite icon;              // スキル選択用アイコン
    public string description;       // スキルの説明文
    public GameObject skillPrefab;   // 実際のスキルPrefab（攻撃処理用）
    public float cooldown;           // クールタイム（秒）
    public AudioClip castSE;         // 発動時のSE
    public float width;              // 攻撃判定の幅
    public float height;             // 攻撃判定の高さ
    public bool useWeaponRange;      // 武器のレンジを使用するか
    public SkillShape shape;          // 攻撃形状（0:横斬り, 1:縦突き, 2:回転）
    // 何もしないスキル（NoneSkill）はskillPrefab=null、description="何も起きません"等で作成

    // IPlayerItem 実装
    public string ItemID => skillName; // 一意性が必要なら別途IDを追加
    public string ItemName => skillName;
    public Sprite Icon => icon;
    public ItemType Type => ItemType.Skill;
    public string Description => description;
} 