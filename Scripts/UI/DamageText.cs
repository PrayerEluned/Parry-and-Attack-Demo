using UnityEngine;
using TMPro;
using System.Collections;

public class DamageText : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI damageText;
    public CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    public float moveUpDistance = 100f;
    public float moveDownDistance = 30f;
    public float totalDuration = 1.0f;
    public float upDuration = 0.3f; // 上がる時間
    public float downDuration = 0.3f; // 下がる時間
    

    
    [Header("Color Settings")]
    public Color normalDamageColor = Color.white;
    public Color criticalDamageColor = Color.red;
    public Color healColor = Color.green;
    
    private Vector3 originalPosition;
    private Coroutine animationCoroutine;
    
    private void Awake()
    {
        // コンポーネントの自動取得
        if (damageText == null)
            damageText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        // Canvas設定（ワールド座標モード）
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        
        // DamageTextLayerFixerを追加してSortingLayerを確実に設定
        var layerFixer = GetComponent<DamageTextLayerFixer>();
        if (layerFixer == null)
        {
            layerFixer = gameObject.AddComponent<DamageTextLayerFixer>();
        }
        layerFixer.FixSortingLayer();
        
        // UIレイヤーに設定
        gameObject.layer = LayerMask.NameToLayer("UI");
        
        originalPosition = transform.localPosition;
    }
    
    public void ShowDamage(int damage, bool isCritical = false, bool isHeal = false)
    {
        Debug.Log($"DamageText: ShowDamage called - Damage: {damage}, Critical: {isCritical}, Heal: {isHeal}");
        
        // 前のアニメーションを停止
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        // テキスト設定
        damageText.text = damage.ToString();
        
        // 色設定
        if (isHeal)
        {
            damageText.color = healColor;
            damageText.text = "+" + damage.ToString();
        }
        else if (isCritical)
        {
            damageText.color = criticalDamageColor;
        }
        else
        {
            damageText.color = normalDamageColor;
        }
        
        // 初期状態リセット
        // transform.localPosition = originalPosition; // この行を削除（ワールド座標で管理するため）
        canvasGroup.alpha = 1f;
        
        Debug.Log($"DamageText: Starting animation from position {transform.position}");
        
        // アニメーション開始
        animationCoroutine = StartCoroutine(AnimateDamage());
    }
    
    private IEnumerator AnimateDamage()
    {
        Debug.Log($"DamageText: AnimateDamage started");
        
        Vector3 startPosition = transform.position; // 現在のワールド座標から開始
        Vector3 upPosition = startPosition + Vector3.up * moveUpDistance;
        Vector3 finalPosition = upPosition + Vector3.down * moveDownDistance;
        
        Debug.Log($"DamageText: Animation positions - Start: {startPosition}, Up: {upPosition}, Final: {finalPosition}");
        
        float elapsedTime = 0f;
        
        // 上に上がる（イージング付き）
        Debug.Log($"DamageText: Starting upward movement for {upDuration} seconds");
        while (elapsedTime < upDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / upDuration;
            // Ease.OutQuad イージング
            float easedProgress = 1f - Mathf.Pow(1f - progress, 2f);
            transform.position = Vector3.Lerp(startPosition, upPosition, easedProgress);
            yield return null;
        }
        Debug.Log($"DamageText: Upward movement completed, position: {transform.position}");
        
        // 少し下がる（イージング付き）
        elapsedTime = 0f;
        while (elapsedTime < downDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / downDuration;
            // Ease.InQuad イージング
            float easedProgress = progress * progress;
            transform.position = Vector3.Lerp(upPosition, finalPosition, easedProgress);
            yield return null;
        }
        
        // 0.3秒後に消える
        yield return new WaitForSeconds(totalDuration - upDuration - downDuration);
        
        // フェードアウト（イージング付き）
        float fadeTime = 0f;
        float fadeDuration = 0.2f;
        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.deltaTime;
            float progress = fadeTime / fadeDuration;
            // Ease.InQuad イージング
            float easedProgress = progress * progress;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, easedProgress);
            yield return null;
        }
        
        // アニメーション完了
        Debug.Log($"DamageText: Animation completed, deactivating object");
        gameObject.SetActive(false);
    }
    
    public void SetPosition(Vector3 worldPosition)
    {
        // ワールド座標に直接設定
        transform.position = worldPosition;
        originalPosition = Vector3.zero; // ローカル座標は0から開始（Canvas内での相対位置）
        
        Debug.Log($"DamageText: SetPosition - WorldPos: {worldPosition}, FinalPos: {transform.position}, OriginalPos: {originalPosition}");
    }
    
    private void OnDestroy()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
    }
} 