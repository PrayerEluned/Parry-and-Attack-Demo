using UnityEngine;
using TMPro;

/// <summary>
/// プレイヤーの現在のレイヤーとソーティングレイヤーをUIに表示する
/// </summary>
public class PlayerLayerStatus : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private TextMeshProUGUI layerText;
    [SerializeField] private TextMeshProUGUI sortingLayerText;
    [SerializeField] private TextMeshProUGUI positionText;

    [Header("ターゲット設定")]
    [SerializeField] private CharacterMovement playerController;

    private SpriteRenderer playerSpriteRenderer;

    private void Start()
    {
        if (playerController == null)
        {
            // シーンから自動的にプレイヤーを探す
            playerController = FindObjectOfType<CharacterMovement>();
        }

        if (playerController != null)
        {
            playerSpriteRenderer = playerController.GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        if (playerController == null)
        {
            if (layerText) layerText.text = "プレイヤーが見つかりません";
            return;
        }

        // レイヤー情報を表示
        if (layerText)
        {
            layerText.text = $"Layer: {LayerMask.LayerToName(playerController.gameObject.layer)}";
        }

        // ソーティングレイヤー情報を表示
        if (sortingLayerText && playerSpriteRenderer != null)
        {
            sortingLayerText.text = $"Sorting Layer: {playerSpriteRenderer.sortingLayerName} (Order: {playerSpriteRenderer.sortingOrder})";
        }

        // 座標情報を表示
        if (positionText)
        {
            Vector3 pos = playerController.transform.position;
            positionText.text = $"Position: ({pos.x:F1}, {pos.y:F1})";
        }
    }
} 