using UnityEngine;
using UnityEngine.UI;

public class PatchSlotButton : MonoBehaviour
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
    [SerializeField] private Sprite defaultIcon;

    private void Awake()
    {
        button = GetComponent<Button>();

        Transform iconTf = transform.Find("Icon");
        if (iconTf != null)
        {
            iconImage = iconTf.GetComponent<Image>();
        }

        if (button == null)
        {
            // Debug.LogError($"{name}: フレーム取得に失敗しました！！！");
        }
        else
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            // Debug.Log($"{name}: Awakeでフレーム取得とOnClick登録完了！！！");
        }

        if (frameImage == null)
        {
            // Debug.LogError($"{name}: フレーム画像取得に失敗しました！！！");
        }
        if (iconImage == null)
        {
            // Debug.LogError($"{name}: アイコン画像取得に失敗しました！！！");
        }
    }

    public void Setup(UIManager manager, int index, EnhancePatch patch = null)
    {
        // Debug.Log($"{name}: Setup呼び出し！スロット番号 {index}");

        uiManager = manager;
        slotIndex = index;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        // フレームとアイコンのセット
        if (patch != null)
        {
            SetPatchIcon(patch.icon);
            SetPatchFrame(patch.rarity);
        }
        else
        {
            SetPatchIcon(null);
            SetPatchFrame(EnhancePatch.PatchRarity.Common); // デフォルトは一番レアリティの低い枠
        }
    }

    private void OnClick()
    {
        // Debug.Log($"PatchSlotButton: OnClick呼び出し！Jn");

        if (uiManager == null)
        {
            // Debug.LogError("PatchSlotButton: uiManagerがnullです！");
        }
        else
        {
            // Debug.Log("PatchSlotButton: uiManagerはOK！装備中パッチのクリック処理実行！");
            uiManager.OnStatusPatchSlotClicked(slotIndex);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (frameImage != null)
            frameImage.color = Color.white;
    }

    public void SetPatchIcon(Sprite sprite)
    {
        if (iconImage != null)
        {
            if (sprite != null)
            {
                iconImage.sprite = sprite;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.sprite = defaultIcon;
                iconImage.color = Color.white;
            }
        }
    }

    public void SetPatchFrame(EnhancePatch.PatchRarity rarity)
    {
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
