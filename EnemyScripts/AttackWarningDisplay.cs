using UnityEngine;
using System.Collections.Generic;

public class AttackWarningDisplay : MonoBehaviour
{
    [Header("警告表示設定")]
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningAlpha = 0.5f;
    [SerializeField] private int warningLayer = 1; // 警告表示のレイヤー（TransparentFX）
    
    private List<GameObject> activeWarnings = new List<GameObject>();
    private Dictionary<GameObject, float> warningEndTimes = new Dictionary<GameObject, float>();
    
    /// <summary>
    /// 攻撃警告を表示（手動で削除されるまで表示し続ける）
    /// </summary>
    /// <param name="attackRange">攻撃範囲</param>
    /// <param name="duration">表示時間（現在は使用されません）</param>
    /// <param name="position">表示位置</param>
    public void ShowWarning(float attackRange, float duration, Vector3 position)
    {
        if (warningPrefab == null)
        {
            CreateDefaultWarning(attackRange, duration, position);
            return;
        }
        
        GameObject warning = Instantiate(warningPrefab, position, Quaternion.identity);
        warning.transform.localScale = Vector3.one * attackRange * 2f; // 直径として設定
        
        // レイヤーを設定
        warning.layer = warningLayer;
        
        // 警告の色を設定
        Renderer renderer = warning.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color color = warningColor;
            color.a = warningAlpha;
            renderer.material.color = color;
        }
        
        activeWarnings.Add(warning);
        // 警告は手動で削除されるまで表示し続ける（時間制限なし）
        // warningEndTimes[warning] = Time.time + duration;
    }
    
    /// <summary>
    /// デフォルトの警告表示を作成（円形スプライト）
    /// </summary>
    private void CreateDefaultWarning(float attackRange, float duration, Vector3 position)
    {
        GameObject warning = new GameObject("AttackWarning");
        warning.transform.position = position;
        
        // レイヤーを設定
        warning.layer = warningLayer;
        
        // SpriteRendererを追加
        SpriteRenderer spriteRenderer = warning.AddComponent<SpriteRenderer>();
        
        // 円形スプライトを作成（attackRangeは半径として使用）
        Sprite circleSprite = CreateCircleSprite(attackRange);
        spriteRenderer.sprite = circleSprite;
        
        // スケールを設定（半径に基づいてサイズ調整）
        float scale = attackRange / 50f; // スプライトのピボットサイズに基づいて調整
        warning.transform.localScale = Vector3.one * scale;
        
        // 色と透明度を設定
        Color color = warningColor;
        color.a = warningAlpha;
        spriteRenderer.color = color;
        
        // ソートレイヤーを設定（UIの上に表示）
        spriteRenderer.sortingOrder = 100;
        
        activeWarnings.Add(warning);
        // 警告は手動で削除されるまで表示し続ける（時間制限なし）
        // warningEndTimes[warning] = Time.time + duration;
    }
    
    /// <summary>
    /// 円形スプライトを作成
    /// </summary>
    private Sprite CreateCircleSprite(float radius)
    {
        int textureSize = 256;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float maxDistance = textureSize / 2f;
        
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                Vector2 pixelPos = new Vector2(x, y);
                float distance = Vector2.Distance(pixelPos, center);
                
                // 円の外側は透明
                if (distance > maxDistance)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
                else
                {
                    // 円の内側は白（色は後で設定）
                    texture.SetPixel(x, y, Color.white);
                }
            }
        }
        
        texture.Apply();
        
        // スプライトを作成
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100f);
        return sprite;
    }
    
    /// <summary>
    /// 全ての警告をクリア
    /// </summary>
    public void ClearAllWarnings()
    {
        foreach (GameObject warning in activeWarnings)
        {
            if (warning != null)
            {
                Destroy(warning);
            }
        }
        activeWarnings.Clear();
        warningEndTimes.Clear();
    }
    
    private void Update()
    {
        // 警告は手動で削除されるまで表示し続けるため、時間による自動削除は無効化
        // nullチェックのみ実行
        List<GameObject> warningsToRemove = new List<GameObject>();
        
        foreach (GameObject warning in activeWarnings)
        {
            if (warning == null)
            {
                warningsToRemove.Add(warning);
            }
        }
        
        foreach (GameObject warning in warningsToRemove)
        {
            activeWarnings.Remove(warning);
            warningEndTimes.Remove(warning);
        }
    }
    
    private void OnDestroy()
    {
        ClearAllWarnings();
    }
} 