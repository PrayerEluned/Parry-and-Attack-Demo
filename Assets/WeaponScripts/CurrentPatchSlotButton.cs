using UnityEngine;
using UnityEngine.UI;

public class CurrentPatchSlotButton : MonoBehaviour
{
    [HideInInspector] public int slotIndex;
    private UIManager uiManager;
    private Button button;
    [SerializeField] private Image frameImage;
    [SerializeField] private Sprite commonFrame;
    [SerializeField] private Sprite uncommonFrame;
    [SerializeField] private Sprite rareFrame;
    [SerializeField] private Sprite epicFrame;
    [SerializeField] private Sprite legendaryFrame;
    private Image iconImage;

    private bool isInitialized = false;

    private void EnsureInitialized()
    {
        if (isInitialized) return;

        button = GetComponent<Button>();
        if (frameImage != null)
        {
            frameImage.color = Color.white;
        }

        Transform iconTf = transform.Find("Icon");
        if (iconTf != null)
        {
            iconImage = iconTf.GetComponent<Image>();
            var rect = iconTf.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
            }
        }

        isInitialized = true;
    }

    public void Setup(UIManager manager, int index)
    {
        EnsureInitialized();

        uiManager = manager;
        slotIndex = index;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        if (frameImage != null)
        {
            frameImage.color = Color.white;
        }

        SetPatchIcon(null);
    }

    private void OnClick()
    {
        if (uiManager != null)
        {
            uiManager.OnCurrentPatchSlotClicked(slotIndex);
        }
    }

    public void SetSelected(bool isSelected)
    {
        EnsureInitialized();
        if (frameImage != null)
            frameImage.color = isSelected ? Color.yellow : Color.white;
    }

    public void SetPatchIcon(Sprite sprite)
    {
        EnsureInitialized();
        if (iconImage == null) return;
        iconImage.sprite = sprite;
        iconImage.color = sprite ? Color.white : new Color(1f, 1f, 1f, 0f);
    }

    public void SetPatchFrame(EnhancePatch.PatchRarity rarity)
    {
        EnsureInitialized();
        if (frameImage == null) return;
        switch (rarity)
        {
            case EnhancePatch.PatchRarity.Common: frameImage.sprite = commonFrame; break;
            case EnhancePatch.PatchRarity.Uncommon: frameImage.sprite = uncommonFrame; break;
            case EnhancePatch.PatchRarity.Rare: frameImage.sprite = rareFrame; break;
            case EnhancePatch.PatchRarity.Epic: frameImage.sprite = epicFrame; break;
            case EnhancePatch.PatchRarity.Legendary: frameImage.sprite = legendaryFrame; break;
            default: frameImage.sprite = commonFrame; break;
        }
    }
}
