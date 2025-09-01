using UnityEngine;

[System.Serializable]
public class EquipWeaponData
{
    public WeaponItem weaponItem;
    public int enhancementLevel;

    public EquipWeaponData(WeaponItem item)
    {
        weaponItem = item;
        enhancementLevel = 0;
    }

    public float GetAddHP() => weaponItem != null ? weaponItem.bonusHP + (weaponItem.enhanceHP * enhancementLevel) : 0f;
    public float GetAddAttack() => weaponItem != null ? weaponItem.bonusAttack + (weaponItem.enhanceAttack * enhancementLevel) : 0f;
    public float GetAddMagicAttack() => weaponItem != null ? weaponItem.bonusMagicAttack + (weaponItem.enhanceMagicAttack * enhancementLevel) : 0f;
    public float GetAddDefense() => weaponItem != null ? weaponItem.bonusDefense + (weaponItem.enhanceDefense * enhancementLevel) : 0f;
    public float GetAddMagicDefense() => weaponItem != null ? weaponItem.bonusMagicDefense + (weaponItem.enhanceMagicDefense * enhancementLevel) : 0f;
    public float GetAddFate() => weaponItem != null ? weaponItem.bonusFate + (weaponItem.enhanceFate * enhancementLevel) : 0f;
}
