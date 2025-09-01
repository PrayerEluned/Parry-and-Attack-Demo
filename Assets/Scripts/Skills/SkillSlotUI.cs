using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button button;
    private int slotIndex;
    private SkillData currentSkill;
    public System.Action<SkillData, int> OnSkillUse;

    public void SetSkill(SkillData skill)
    {
        currentSkill = skill;
        if (iconImage) iconImage.sprite = skill.icon;
        if (nameText) nameText.text = skill.skillName;
    }

    public SkillData GetSkill() => currentSkill;

    public void Setup(int index, System.Action<SkillData, int> onUse)
    {
        slotIndex = index;
        OnSkillUse = onUse;
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnSlotClicked);
        }
    }

    private void OnSlotClicked()
    {
        OnSkillUse?.Invoke(currentSkill, slotIndex);
    }
} 