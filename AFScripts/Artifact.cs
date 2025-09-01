using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Artifact
{
    public string artifactID;         // ��ӂ�ID
    public string artifactName;       // �\����
    public string description;        // ������
    public Sprite icon;               // �A�C�R���摜
    public int maxStackSize = 5;      // �ő及����
    public int currentStackSize = 0;  // ���݂̏�����

    public StatType affectedStat;     // �e����^����X�e�[�^�X
    public bool isMultiplier;         // ���Z����Z��
    public float effectValue;         // ���ʒl�i���Z�l�܂��͏�Z�䗦�j
    [Header("������� (�I�v�V����)")]
    public bool isPatchSlotExpandArtifact = false;
    public Artifact() { }


    public Artifact(ArtifactItem item)
    {
        artifactID = item.artifactID;
        artifactName = item.artifactName;
        description = item.description;
        icon = item.icon;
        maxStackSize = item.maxStackCount;
        currentStackSize = 0;
        affectedStat = item.affectedStat;
        isMultiplier = item.isMultiplier;
        effectValue = item.effectValue;
    }


    // ���ʂ��v�Z����
    public float CalculateEffect()
    {
        if (isMultiplier)
        {
            // 乗算: effectValue^currentStackSize
            return Mathf.Pow(effectValue, currentStackSize);
        }
        else
        {
            // 加算
            return effectValue * currentStackSize;
        }
    }
}