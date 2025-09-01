using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Image (Filled) や Slider の fillAmount を変えるだけの簡易 UI
/// </summary>
public class AttackGaugeUI : MonoBehaviour
{
    [SerializeField] private Image fillImg;

    /// <param name="ratio">0〜1</param>
    public void SetFill(float ratio)
    {
        fillImg.fillAmount = Mathf.Clamp01(ratio);
    }
}
