using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static EnhancePatch;

public class PatchItemDisplay : MonoBehaviour
{
    [Header("UI�v�f")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private Image frameImage;
    [SerializeField] private Sprite commonFrame;
    [SerializeField] private Sprite uncommonFrame;
    [SerializeField] private Sprite rareFrame;
    [SerializeField] private Sprite epicFrame;
    [SerializeField] private Sprite legendaryFrame;

    [SerializeField] private Button button;

    private EnhancePatch patchData;
    private UIManager uiManager;

    public EnhancePatch PatchData => patchData;

    // Patchꗗ\ɌĂ΂��
    public void Setup(EnhancePatch patch, UIManager manager)
    {
        patchData = patch;
        uiManager = manager;

        if (patch == null)
        {
            if (iconImage != null) iconImage.sprite = null;
            if (nameText != null) nameText.text = "";
            if (effectText != null) effectText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            gameObject.SetActive(false);
            return;
        }

        if (iconImage != null) iconImage.sprite = patch.icon;
        if (nameText != null) nameText.text = patch.patchName;
        if (descriptionText != null) descriptionText.text = patch.description;
        if (effectText != null) effectText.text = $"強化値 : <color=#69B0FF>{patch.patchLevel}</color>   効果 : {patch.GetEffectDescription()}";
        if (frameImage != null)
            frameImage.sprite = GetFrameByRarity(patch.rarity);
        gameObject.SetActive(true);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    //{^ꂽƂ�
    public void OnClick()
    {
        if (uiManager != null && patchData != null)
        {
            uiManager.OnPatchSelectedForCurrentWeapon(patchData);
        }
    }

    // パッチ選択時のハイライト用
    public void SetSelected(bool isSelected)
    {
        if (frameImage != null)
        {
            if (isSelected)
            {
                frameImage.color = Color.yellow; // 選択時は黄色枠
            }
            else
            {
                frameImage.color = Color.white; // 未選択は白
            }
        }
    }

    private Sprite GetFrameByRarity(PatchRarity rarity)
    {
        switch (rarity)
        {
            case PatchRarity.Common: return commonFrame;
            case PatchRarity.Uncommon: return uncommonFrame;
            case PatchRarity.Rare: return rareFrame;
            case PatchRarity.Epic: return epicFrame;
            case PatchRarity.Legendary: return legendaryFrame;
            default: return commonFrame;
        }
    }
}
