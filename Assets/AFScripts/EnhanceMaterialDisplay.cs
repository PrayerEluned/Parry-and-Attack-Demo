using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnhanceMaterialDisplay : MonoBehaviour
{
    [SerializeField] private Image materialIcon;
    [SerializeField] private TMP_Text materialNameText;
    [SerializeField] private TMP_Text requiredAmountText;
    [SerializeField] private TMP_Text ownedAmountText;

    public void Setup(Sprite icon, string name, int required, int owned)
    {
        if (materialIcon != null) materialIcon.sprite = icon;
        if (materialNameText != null) materialNameText.text = name;
        if (requiredAmountText != null) requiredAmountText.text = $"× {required}";
        if (ownedAmountText != null) ownedAmountText.text = $"× {owned}";
    }
}
