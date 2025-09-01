using UnityEngine;
using Game.Items;
using System.Collections.Generic;

[System.Serializable]
public class EnhancementMaterialRequirement
{
    public ConsumableItem material;
    public int requiredAmount = 1;
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon")]
public class WeaponItem : ScriptableObject, IPlayerItem
{
    [Header("基本情報")]
    public string weaponID;
    public string weaponName;
    public Sprite icon;
    [TextArea] public string description;
    [TextArea] public string effectText;

    [Header("強化素材リスト（複数指定可）")]
    public List<EnhancementMaterialRequirement> enhancementMaterials = new List<EnhancementMaterialRequirement>();

    [Header("強化幅（1回強化ごとの上昇値）")]
    public float enhanceHP = 0f;
    public float enhanceAttack = 0f;
    public float enhanceMagicAttack = 0f;
    public float enhanceDefense = 0f;
    public float enhanceMagicDefense = 0f;
    public float enhanceFate = 0f;
    public float enhanceSpeed = 0f;

    [Header("ステータス補正（基礎値）")]
    public float bonusHP;
    public float bonusAttack;
    public float bonusMagicAttack;
    public float bonusDefense;
    public float bonusMagicDefense;
    public float bonusFate;

    [Header("攻撃特性")]
    public float attackRange = 1.0f;             // 攻撃範囲
    [Header("Attack Timing")]
    [Tooltip("基本インターバル (秒)")]
    public float attackInterval = 1f;

    [Tooltip("基本レンジ (m)")]
    public float range = 1.2f;
    public float motionSpeedMultiplier = 1.0f;   // 移動速度倍率

    // IPlayerItem 実装
    public string ItemID => weaponID;
    public string ItemName => weaponName;
    public Sprite Icon => icon;
    public ItemType Type => ItemType.Weapon;
    public string Description => description;
}