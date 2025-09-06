using UnityEngine;
using Game.Items;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Game/Consumable")]
public class ConsumableItem : ScriptableObject, IPlayerItem
{
    [Header("基本情報")]
    public string itemID;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public int maxStack = 500; // 初期上限

    // IPlayerItem 実装
    public string ItemID => itemID;
    public string ItemName => itemName;
    public Sprite Icon => icon;
    public ItemType Type => ItemType.Consumable;
    public string Description => description;
} 