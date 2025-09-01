using UnityEngine;
using Game.Items;

[CreateAssetMenu(fileName = "NewArtifact", menuName = "Game/Artifact")]
public class ArtifactItem : ScriptableObject, IPlayerItem
{
    [Header("基本情報")]
    public string artifactID;              // 一意のID
    public string artifactName;            // 名前
    [TextArea]
    public string description;             // 説明
    public Sprite icon;                    // アイコン
    public int maxStackCount = 3;          // 最大所持数

    [Header("効果設定")]
    public StatType affectedStat;            // 対象ステータス
    public float effectValue = 1.0f;       // 効果値
    public bool isMultiplier = false;      // 乗算か加算か
    [Header("パッチスロット拡張AFかどうか")]
    public bool isPatchSlotExpandArtifact = false;

    [HideInInspector]
    public int currentStackSize = 0;       // 現在の所持数

    // IPlayerItem 実装
    public string ItemID => artifactID;
    public string ItemName => artifactName;
    public Sprite Icon => icon;
    public ItemType Type => ItemType.Artifact;
    public string Description => description;
}
