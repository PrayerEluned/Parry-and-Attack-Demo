using UnityEngine;

namespace Game.Items
{
    public enum ItemType
    {
        Artifact,
        Weapon,
        Patch,
        Skill,
        Consumable
    }

    public interface IPlayerItem
    {
        string ItemID { get; }
        string ItemName { get; }
        Sprite Icon { get; }
        ItemType Type { get; }
        string Description { get; }
    }
} 