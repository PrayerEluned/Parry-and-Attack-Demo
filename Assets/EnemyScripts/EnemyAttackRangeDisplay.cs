using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;

/// <summary>
/// 敵の攻撃範囲を表示するコンポーネント（複数範囲円対応版）
/// </summary>
public class EnemyAttackRangeDisplay : MonoBehaviour
{
    [Header("表示設定")]
    [SerializeField] private Color rangeColor = new Color(1f, 0f, 0f, 0.3f); // 赤色、30%透明度
    [SerializeField] private float displayDuration = 1.0f; // 表示時間
    [SerializeField] private bool showDebugInfo = false;
    
    [Header("円の設定")]
    [SerializeField] private int sortingOrder = 2; // ソーティングオーダー
    [SerializeField] private string sortingLayerName = "Enemy"; // ソーティングレイヤー
    
    [Header("音效設定")]
    [SerializeField] private AudioClip rangeDisplaySound; // 範囲円表示時のSE
    [SerializeField] private float soundVolume = 0.5f; // SEの音量
    [SerializeField] private bool enableSound = true; // SEの有効/無効
    
    // 複数の範囲円を管理
    private Dictionary<int, RangeCircleData> activeRangeCircles = new Dictionary<int, RangeCircleData>();
    private int nextCircleId = 0;
    
    // AudioManager参照
    private AudioSystem.AudioManager audioManager;
    
    /// <summary>
    /// 範囲円のデータ
    /// </summary>
    private class RangeCircleData
    {
        public GameObject rangeObject;
        public SpriteRenderer rangeRenderer;
        public Coroutine displayCoroutine;
        public float radius;
        public float startTime;
        
        public RangeCircleData(GameObject obj, SpriteRenderer renderer, float r)
        {
            rangeObject = obj;
            rangeRenderer = renderer;
            radius = r;
            startTime = Time.time;
        }
    }
    
    private void Awake()
    {
        // AudioManagerの取得
        audioManager = AudioSystem.AudioManager.Instance;
    }
    
    /// <summary>
    /// 攻撃範囲を表示（複数同時表示対応）
    /// </summary>
    /// <param name="radius">攻撃範囲の半径</param>
    /// <param name="duration">表示時間（-1の場合はデフォルト時間を使用）</param>
    /// <returns>範囲円のID（後で非表示にする際に使用）</returns>
    public int ShowAttackRange(float radius, float duration = -1f)
    {
        int circleId = nextCircleId++;
        
        // 範囲円オブジェクトを作成
        GameObject rangeObject = CreateRangeCircleObject(circleId);
        if (rangeObject == null)
        {
            Debug.LogWarning("[EnemyAttackRangeDisplay] 範囲円オブジェクトの作成に失敗しました");
            return -1;
        }
        
        SpriteRenderer rangeRenderer = rangeObject.GetComponent<SpriteRenderer>();
        if (rangeRenderer == null)
        {
            Debug.LogWarning("[EnemyAttackRangeDisplay] SpriteRendererが見つかりません");
            return -1;
        }
        
        // スケールを設定（半径に応じて正確にスケール）
        rangeObject.transform.localScale = Vector3.one * (radius * 2f); // 直径に合わせてスケール
        
        // 表示時間を設定
        float displayTime = duration > 0f ? duration : displayDuration;
        
        // 範囲円データを作成
        RangeCircleData circleData = new RangeCircleData(rangeObject, rangeRenderer, radius);
        
        // 表示コルーチンを開始
        circleData.displayCoroutine = StartCoroutine(DisplayRangeCoroutine(circleId, displayTime));
        
        // アクティブな範囲円に追加
        activeRangeCircles[circleId] = circleData;
        
        // 範囲円表示時のSEを再生
        PlayRangeDisplaySound();
        
        if (showDebugInfo)
        {
            Debug.Log($"[EnemyAttackRangeDisplay] 範囲円{circleId}を表示開始。半径: {radius}, スケール: {radius * 2f}, 表示時間: {displayTime}, アクティブ数: {activeRangeCircles.Count}");
        }
        
        return circleId;
    }
    
    /// <summary>
    /// 指定した範囲円を非表示
    /// </summary>
    /// <param name="circleId">範囲円のID</param>
    public void HideAttackRange(int circleId)
    {
        if (!activeRangeCircles.ContainsKey(circleId))
        {
            Debug.LogWarning($"[EnemyAttackRangeDisplay] 範囲円{circleId}が見つかりません");
            return;
        }
        
        RangeCircleData circleData = activeRangeCircles[circleId];
        
        // コルーチンを停止
        if (circleData.displayCoroutine != null)
        {
            StopCoroutine(circleData.displayCoroutine);
        }
        
        // オブジェクトを削除
        if (circleData.rangeObject != null)
        {
            Destroy(circleData.rangeObject);
        }
        
        // 辞書から削除
        activeRangeCircles.Remove(circleId);
        
        if (showDebugInfo)
        {
            Debug.Log($"[EnemyAttackRangeDisplay] 範囲円{circleId}を非表示にしました。残りアクティブ数: {activeRangeCircles.Count}");
        }
    }
    
    /// <summary>
    /// すべての範囲円を非表示
    /// </summary>
    public void HideAllAttackRanges()
    {
        List<int> circleIds = new List<int>(activeRangeCircles.Keys);
        foreach (int circleId in circleIds)
        {
            HideAttackRange(circleId);
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[EnemyAttackRangeDisplay] すべての範囲円を非表示にしました");
        }
    }
    
    /// <summary>
    /// 範囲円オブジェクトを作成
    /// </summary>
    private GameObject CreateRangeCircleObject(int circleId)
    {
        GameObject rangeObject = new GameObject($"AttackRangeCircle_{circleId}");
        rangeObject.transform.SetParent(transform);
        rangeObject.transform.localPosition = Vector3.zero;
        
        // SpriteRendererを追加
        SpriteRenderer rangeRenderer = rangeObject.AddComponent<SpriteRenderer>();
        rangeRenderer.sprite = CreateCircularSprite();
        rangeRenderer.color = rangeColor;
        rangeRenderer.sortingOrder = sortingOrder;
        rangeRenderer.sortingLayerName = sortingLayerName;
        
        return rangeObject;
    }
    
    /// <summary>
    /// 円形のスプライトを作成
    /// </summary>
    private Sprite CreateCircularSprite()
    {
        int textureSize = 128;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float radius = textureSize / 2f;
        
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance <= radius)
                {
                    // 円の内側は設定された色
                    texture.SetPixel(x, y, rangeColor);
                }
                else
                {
                    // 円の外側は透明
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        // スプライトを作成（1ユニットのサイズに正規化）
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 128f);
        
        if (showDebugInfo)
        {
            Debug.Log($"[EnemyAttackRangeDisplay] 円形スプライトを作成しました。サイズ: {textureSize}x{textureSize}, PixelsPerUnit: 128");
        }
        
        return sprite;
    }
    
    /// <summary>
    /// 範囲円表示のコルーチン
    /// </summary>
    private IEnumerator DisplayRangeCoroutine(int circleId, float duration)
    {
        if (!activeRangeCircles.ContainsKey(circleId)) yield break;
        
        RangeCircleData circleData = activeRangeCircles[circleId];
        
        // 表示開始
        circleData.rangeObject.SetActive(true);
        
        // フェードイン効果（オプション）
        Color startColor = rangeColor;
        startColor.a = 0f;
        Color endColor = rangeColor;
        
        float fadeTime = 0.1f; // フェードイン時間
        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            circleData.rangeRenderer.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        // 指定時間表示
        yield return new WaitForSeconds(duration - fadeTime);
        
        // フェードアウト効果
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            circleData.rangeRenderer.color = Color.Lerp(endColor, startColor, t);
            yield return null;
        }
        
        // 非表示
        HideAttackRange(circleId);
        
        if (showDebugInfo)
        {
            Debug.Log($"[EnemyAttackRangeDisplay] 範囲円{circleId}の表示を終了しました");
        }
    }
    
    /// <summary>
    /// 範囲円表示時のSEを再生
    /// </summary>
    private void PlayRangeDisplaySound()
    {
        if (!enableSound || audioManager == null || rangeDisplaySound == null)
        {
            return;
        }
        
        try
        {
            audioManager.PlaySE(rangeDisplaySound);
            
            if (showDebugInfo)
            {
                Debug.Log($"[EnemyAttackRangeDisplay] 範囲円表示SEを再生: {rangeDisplaySound.name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[EnemyAttackRangeDisplay] SE再生エラー: {e.Message}");
        }
    }
    
    /// <summary>
    /// デバッグ用Gizmos（Sceneビューで範囲を確認）
    /// </summary>
    private void OnDrawGizmos()
    {
        if (showDebugInfo && activeRangeCircles.Count > 0)
        {
            foreach (var kvp in activeRangeCircles)
            {
                RangeCircleData circleData = kvp.Value;
                if (circleData.rangeObject != null && circleData.rangeObject.activeSelf)
                {
                    // 各範囲円を青い線で表示
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(circleData.rangeObject.transform.position, circleData.radius);
                    
                    // 中心点を赤い点で表示
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(circleData.rangeObject.transform.position, 0.1f);
                }
            }
        }
    }
    
    /// <summary>
    /// 現在表示中の範囲円の数を取得（デバッグ用）
    /// </summary>
    public int GetActiveRangeCircleCount()
    {
        return activeRangeCircles.Count;
    }
    
    /// <summary>
    /// SEの有効/無効を設定
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        enableSound = enabled;
        if (showDebugInfo)
        {
            Debug.Log($"[EnemyAttackRangeDisplay] SE有効設定: {enabled}");
        }
    }
    
    /// <summary>
    /// SEの音量を設定
    /// </summary>
    public void SetSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);
        
        if (showDebugInfo)
        {
            Debug.Log($"[EnemyAttackRangeDisplay] SE音量設定: {soundVolume}");
        }
    }
    
    /// <summary>
    /// SEのAudioClipを設定
    /// </summary>
    public void SetRangeDisplaySound(AudioClip sound)
    {
        rangeDisplaySound = sound;
        if (showDebugInfo)
        {
            Debug.Log($"[EnemyAttackRangeDisplay] SE設定: {(sound != null ? sound.name : "null")}");
        }
    }
    
    /// <summary>
    /// 範囲の色を設定
    /// </summary>
    public void SetRangeColor(Color color)
    {
        rangeColor = color;
        // 既存の範囲円にも適用
        foreach (var kvp in activeRangeCircles)
        {
            if (kvp.Value.rangeRenderer != null)
            {
                kvp.Value.rangeRenderer.color = color;
            }
        }
    }
    
    /// <summary>
    /// 表示時間を設定
    /// </summary>
    public void SetDisplayDuration(float duration)
    {
        displayDuration = duration;
    }
    
    /// <summary>
    /// ソーティングオーダーを設定
    /// </summary>
    public void SetSortingOrder(int order)
    {
        sortingOrder = order;
        // 既存の範囲円にも適用
        foreach (var kvp in activeRangeCircles)
        {
            if (kvp.Value.rangeRenderer != null)
            {
                kvp.Value.rangeRenderer.sortingOrder = order;
            }
        }
    }
    
    /// <summary>
    /// ソーティングレイヤーを設定
    /// </summary>
    public void SetSortingLayer(string layerName)
    {
        sortingLayerName = layerName;
        // 既存の範囲円にも適用
        foreach (var kvp in activeRangeCircles)
        {
            if (kvp.Value.rangeRenderer != null)
            {
                kvp.Value.rangeRenderer.sortingLayerName = layerName;
            }
        }
    }
    
    private void OnDestroy()
    {
        // コルーチンを停止
        foreach (var kvp in activeRangeCircles)
        {
            if (kvp.Value.displayCoroutine != null)
            {
                StopCoroutine(kvp.Value.displayCoroutine);
            }
        }
    }
} 