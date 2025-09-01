using UnityEngine;

/// <summary>
/// InspectorでBasic/Villageモードを切り替えるシンプルなスクリプト
/// </summary>
public class LayerSystemSwitcher : MonoBehaviour
{
    [Header("モード設定")]
    [SerializeField] private LayerMode currentMode = LayerMode.Basic;
    
    [Header("レイヤー設定")]
    [SerializeField] private string basicLayer = "Layer 1";
    [SerializeField] private string villageLayer = "Terrain";
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebug = true;
    
    private CharacterMovement characterMovement;
    
    public enum LayerMode
    {
        Basic,
        Village
    }
    
    void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        if (characterMovement == null)
        {
            Debug.LogError("CharacterMovementコンポーネントが見つかりません");
            return;
        }
        
        // 初期モードを設定
        SetMode(currentMode);
    }
    
    /// <summary>
    /// Inspectorでモードを変更した時の処理
    /// </summary>
    void OnValidate()
    {
        if (Application.isPlaying && characterMovement != null)
        {
            SetMode(currentMode);
        }
    }
    
    /// <summary>
    /// モードを設定
    /// </summary>
    public void SetMode(LayerMode mode)
    {
        currentMode = mode;
        
        switch (mode)
        {
            case LayerMode.Basic:
                SetBasicMode();
                break;
            case LayerMode.Village:
                SetVillageMode();
                break;
        }
        
        if (showDebug)
        {
            Debug.Log($"モードを {mode} に切り替えました");
        }
    }
    
    /// <summary>
    /// Basicモードを設定
    /// </summary>
    private void SetBasicMode()
    {
        // プレイヤーをLayer 1に設定
        characterMovement.ChangeLayer(basicLayer);
        
        // Basicアセットの衝突設定を適用
        UpdateBasicCollisionMatrix();
        
        if (showDebug)
        {
            Debug.Log("Basicモード: Layer 1, 2, 3の衝突設定を適用");
        }
    }
    
    /// <summary>
    /// Villageモードを設定
    /// </summary>
    private void SetVillageMode()
    {
        // プレイヤーをTerrainレイヤーに設定
        characterMovement.ChangeLayer(villageLayer);
        
        // Villageアセットの衝突設定を適用
        UpdateVillageCollisionMatrix();
        
        if (showDebug)
        {
            Debug.Log("Villageモード: Terrain/Water Contactの衝突設定を適用");
        }
    }
    
    /// <summary>
    /// Basicモード用の衝突マトリックスを更新
    /// </summary>
    private void UpdateBasicCollisionMatrix()
    {
        int playerLayer = LayerMask.NameToLayer(basicLayer);
        
        // Layer 1, 2, 3の衝突設定
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            
            if (layerName == "Layer 1" || layerName == "Layer 2" || layerName == "Layer 3")
            {
                // 現在のレイヤーとのみ衝突
                bool shouldIgnore = (i != playerLayer);
                Physics2D.IgnoreLayerCollision(playerLayer, i, shouldIgnore);
            }
        }
    }
    
    /// <summary>
    /// Villageモード用の衝突マトリックスを更新
    /// </summary>
    private void UpdateVillageCollisionMatrix()
    {
        int playerLayer = LayerMask.NameToLayer(villageLayer);
        
        // TerrainとWater Contactの衝突設定
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            
            if (layerName == "Terrain" || layerName == "Water Contact")
            {
                // 現在のレイヤーとのみ衝突
                bool shouldIgnore = (i != playerLayer);
                Physics2D.IgnoreLayerCollision(playerLayer, i, shouldIgnore);
            }
        }
    }
    
    /// <summary>
    /// 現在のモードを取得
    /// </summary>
    public LayerMode GetCurrentMode()
    {
        return currentMode;
    }
    
    /// <summary>
    /// デバッグ情報を表示
    /// </summary>
    void OnGUI()
    {
        if (!showDebug) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 80));
        GUILayout.Label($"現在のモード: {currentMode}");
        GUILayout.Label($"プレイヤーレイヤー: {LayerMask.LayerToName(gameObject.layer)}");
        GUILayout.EndArea();
    }
} 