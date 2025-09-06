using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillItemDisplay : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button selectButton;

    private SkillData skillData;
    private SkillSelectPanel parentPanel;
    private UIManager uiManager;

    public void Setup(SkillData data, SkillSelectPanel panel)
    {
        skillData = data;
        parentPanel = panel;
        uiManager = null;
        if (iconImage) iconImage.sprite = data.icon;
        if (nameText) nameText.text = data.skillName;
        if (descriptionText) descriptionText.text = data.description;
        if (selectButton)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelect);
        }
    }

    public void Setup(SkillData data, UIManager manager)
    {
        skillData = data;
        parentPanel = null;
        uiManager = manager;
        if (iconImage) iconImage.sprite = data.icon;
        if (nameText) nameText.text = data.skillName;
        if (descriptionText) descriptionText.text = data.description;
        if (selectButton)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSelect);
        }
    }

    private void OnSelect()
    {
        if (parentPanel != null)
            parentPanel.OnSkillSelected(skillData);
        else if (uiManager != null)
            uiManager.OnSkillSelected(skillData);
    }
} 