using UnityEngine;
using System.Collections.Generic;

public class DamageTextManager : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject damageTextPrefab;
    public int poolSize = 20;
    
    [Header("Display Settings")]
    [Tooltip("ダメージテキストの位置をランダムにずらすかどうか")]
    public bool useRandomOffset = false;
    public float randomOffsetX = 20f;
    public float randomOffsetY = 10f;
    
    private List<DamageText> damageTextPool;
    private int currentIndex = 0;
    
    private static DamageTextManager instance;
    public static DamageTextManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DamageTextManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DamageTextManager");
                    instance = go.AddComponent<DamageTextManager>();
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializePool()
    {
        damageTextPool = new List<DamageText>();
        
        for (int i = 0; i < poolSize; i++)
        {
            CreateDamageTextInPool();
        }
    }
    
    private void CreateDamageTextInPool()
    {
        if (damageTextPrefab == null)
        {
            Debug.LogError("DamageText prefab is not assigned!");
            return;
        }
        
        GameObject obj = Instantiate(damageTextPrefab, transform);
        DamageText damageText = obj.GetComponent<DamageText>();
        
        if (damageText == null)
        {
            Debug.LogError("DamageText component not found on prefab!");
            return;
        }
        
        // Canvas設定（ワールド座標モード）
        Canvas canvas = obj.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = obj.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        
        // DamageTextLayerFixerを追加してSortingLayerを確実に設定
        var layerFixer = obj.GetComponent<DamageTextLayerFixer>();
        if (layerFixer == null)
        {
            layerFixer = obj.AddComponent<DamageTextLayerFixer>();
        }
        layerFixer.FixSortingLayer();
        
        // UIレイヤーに設定
        obj.layer = LayerMask.NameToLayer("UI");
        
        obj.SetActive(false);
        damageTextPool.Add(damageText);
    }
    
    public void ShowDamage(Vector3 worldPosition, int damage, bool isCritical = false, bool isHeal = false)
    {
        Debug.Log($"DamageTextManager: ShowDamage called - Position: {worldPosition}, Damage: {damage}");
        
        if (damageTextPool.Count == 0)
        {
            Debug.LogWarning("No damage text objects in pool!");
            return;
        }
        
        // プールから利用可能なオブジェクトを取得
        DamageText damageText = GetAvailableDamageText();
        
        if (damageText != null)
        {
            // 位置設定
            Vector3 finalPosition = worldPosition;
            
            if (useRandomOffset)
            {
                // ランダムオフセット付き
                finalPosition = worldPosition + new Vector3(
                    Random.Range(-randomOffsetX, randomOffsetX),
                    Random.Range(-randomOffsetY, randomOffsetY),
                    0f
                );
            }
            
            damageText.SetPosition(finalPosition);
            damageText.gameObject.SetActive(true);
            damageText.ShowDamage(damage, isCritical, isHeal);
            Debug.Log($"DamageTextManager: Damage text activated at {finalPosition}");
        }
        else
        {
            Debug.LogError("DamageTextManager: Failed to get damage text from pool");
        }
    }
    
    private DamageText GetAvailableDamageText()
    {
        // 非アクティブなオブジェクトを探す
        for (int i = 0; i < damageTextPool.Count; i++)
        {
            int index = (currentIndex + i) % damageTextPool.Count;
            if (!damageTextPool[index].gameObject.activeInHierarchy)
            {
                currentIndex = (index + 1) % damageTextPool.Count;
                return damageTextPool[index];
            }
        }
        
        // すべてアクティブな場合は、最も古いものを再利用
        DamageText oldest = damageTextPool[currentIndex];
        currentIndex = (currentIndex + 1) % damageTextPool.Count;
        return oldest;
    }
    
    // 便利な静的メソッド
    public static void ShowDamageAt(Vector3 worldPosition, int damage, bool isCritical = false, bool isHeal = false)
    {
        Instance.ShowDamage(worldPosition, damage, isCritical, isHeal);
    }
    
    // プレイヤーやエネミーの位置からダメージを表示
    public static void ShowDamageOnTarget(Transform target, int damage, bool isCritical = false, bool isHeal = false)
    {
        // ターゲットの位置に少し上にオフセットして表示
        Vector3 position = target.position + Vector3.up * 0.8f; // オフセットを少し大きくする
        Instance.ShowDamage(position, damage, isCritical, isHeal);
        Debug.Log($"DamageTextManager: ShowDamageOnTarget - Target: {target.name}, Position: {position}, Damage: {damage}");
    }
} 