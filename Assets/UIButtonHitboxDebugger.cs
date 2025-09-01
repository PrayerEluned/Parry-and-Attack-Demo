using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class UIButtonHitboxDebugger : MonoBehaviour
{
    [Tooltip("��������{�^����S�Ď擾���܂�")]
    private Button[] buttons;

    [Tooltip("�q�b�g�{�b�N�X�̐F")]
    public Color boxColor = new Color(1, 0, 0, 0.2f);

    private Texture2D _texture;

    void OnEnable()
    {
        buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        // 1x1 eNX�`��
        _texture = Texture2D.whiteTexture;
    }

    void OnGUI()
    {
        if (buttons == null || _texture == null) return;

        foreach (var btn in buttons)
        {
            var rt = btn.GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            // 0:left-bottom, 2:right-top
            Vector2 bl = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            Vector2 tr = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

            // GUI���W�n�͍��オ(0,0)�Ȃ̂ŕϊ�
            var rect = new Rect(
                bl.x,
                Screen.height - tr.y,
                tr.x - bl.x,
                tr.y - bl.y
            );

            GUI.color = boxColor;
            GUI.DrawTexture(rect, _texture);
        }
    }
}
