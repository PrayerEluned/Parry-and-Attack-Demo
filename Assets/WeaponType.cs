using UnityEngine;
using System.Collections.Generic;

public enum WeaponType
{
    Sword,
    Axe,
    Spear,
    Bow,
    Staff,
    // ���̕���^�C�v��ǉ�
}

[System.Serializable]
public class WeaponSlotEffect
{
    public string effectName;
    public string description;

    // �X���b�g���ʂ̎��
    public bool isHealOnHit = false;          // �U���q�b�g���ɉ�
    public bool isStatBoost = false;          // �X�e�[�^�X����
    public bool isAttackSpeedBoost = false;   // �U�����x�㏸
    public bool isReachExtender = false;      // ���[�`����

    // ���ʂ̐��l
    public float healPercentage = 0f;         // �_���[�W�̉�%���񕜂��邩
    public float statBoostAmount = 0f;        // �X�e�[�^�X������
    public StatType boostedStat;              // ��������X�e�[�^�X
    public float attackSpeedReduction = 0f;   // �U���Ԋu�Z�k��
    public float reachExtensionAmount = 0f;   // ���[�`������
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class Weapon : ScriptableObject
{
    [Header("��{���")]
    public string weaponName;
    public Sprite weaponIcon;
    public GameObject weaponPrefab;
    public WeaponType weaponType;

    [Header("��{�X�e�[�^�X")]
    public float baseDamage = 10f;
    public float baseAttackSpeed = 1.0f;  // �U���Ԋu�i�b�j
    public float baseReach = 1.0f;        // ��{���[�`

    [Header("�X���b�g")]
    public int maxSlots = 3;              // �ő�X���b�g��
    public List<WeaponSlotEffect> equippedSlotEffects = new List<WeaponSlotEffect>();

    // ���݂̗L���ȃX���b�g���i�A�b�v�O���[�h�\�j
    public int activeSlots = 1;

    // �������̃{�[�i�X�X�e�[�^�X
    public float bonusAttack = 0;
    public float bonusDefense = 0;
    public float bonusMagicAttack = 0;
    public float bonusMagicDefense = 0;
    public float bonusSpeed = 0;
    public float bonusFate = 0;

    // �U�����[�V�������i�A�j���[�V�������Ȃǁj
    public string attackAnimationTrigger = "Attack";
    public float attackDuration = 0.5f;

    // �X���b�g���ʂ�K�p������̎��ۂ̒l���v�Z
    public float ActualDamage => baseDamage;

    public float ActualAttackSpeed
    {
        get
        {
            float speedModifier = 1.0f;
            foreach (var effect in equippedSlotEffects)
            {
                if (effect.isAttackSpeedBoost)
                {
                    speedModifier -= effect.attackSpeedReduction;
                }
            }
            // �ŏ��l��0.5�b�ɐ����i���܂�ɑ����Ȃ肷���Ȃ��悤�Ɂj
            return Mathf.Max(baseAttackSpeed * speedModifier, 0.5f);
        }
    }

    public float ActualReach
    {
        get
        {
            float reachModifier = 0f;
            foreach (var effect in equippedSlotEffects)
            {
                if (effect.isReachExtender)
                {
                    reachModifier += effect.reachExtensionAmount;
                }
            }
            return baseReach + reachModifier;
        }
    }

    // �X���b�g�Ɍ��ʂ��Z�b�g
    public bool EquipSlotEffect(WeaponSlotEffect effect)
    {
        if (equippedSlotEffects.Count >= activeSlots)
        {
            return false;
        }

        equippedSlotEffects.Add(effect);
        return true;
    }

    // �X���b�g������ʂ���菜��
    public bool RemoveSlotEffect(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSlotEffects.Count)
        {
            return false;
        }

        equippedSlotEffects.RemoveAt(slotIndex);
        return true;
    }
}