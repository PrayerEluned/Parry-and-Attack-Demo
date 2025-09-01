using UnityEngine;

public class ArtifactDebugAdder : MonoBehaviour
{
    public ArtifactItem testArtifact; // �����ɒǉ�������AF���A�^�b�`�I

    private ArtifactInventory inventory;

    private void Start()
    {
        // Phase 3: デバッガー機能を慎重に復活
        Debug.Log("ArtifactDebugAdder: Phase 3復活 - デバッグ機能を有効化");
        
        try
        {
            // 遅延初期化でArtifactInventoryの参照を取得
            Invoke(nameof(InitializeDebugger), 0.5f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ArtifactDebugAdder: Phase 3復活エラー - {e.Message}");
        }
    }
    
    private void InitializeDebugger()
    {
        try
        {
            // ArtifactInventoryの参照取得（安全チェック付き）
            if (inventory == null)
            {
                inventory = GetComponent<ArtifactInventory>();
                if (inventory == null)
                {
                    inventory = Object.FindFirstObjectByType<ArtifactInventory>();
                    if (inventory == null)
                    {
                        Debug.LogWarning("ArtifactDebugAdder: ArtifactInventoryが見つかりません");
                        return;
                    }
                }
            }
            
            Debug.Log("ArtifactDebugAdder: 初期化完了 - F1キーでアーティファクト追加可能");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ArtifactDebugAdder: 初期化エラー - {e.Message}");
        }
    }

    void Update()
    {
        try
        {
            // 軽量なUpdate処理（安全チェック付き）
            if (inventory != null && Input.GetKeyDown(KeyCode.F1))
            {
                AddRandomArtifact();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ArtifactDebugAdder: Update エラー - {e.Message}");
        }
    }

    private void AddRandomArtifact()
    {
        if (testArtifact != null && inventory != null)
        {
            inventory.AddArtifact(testArtifact, 1);
            // Debug.Log($"アーティファクト追加: {testArtifact.artifactName}");
        }
        else
        {
            // Debug.LogWarning("testArtifact ܂ inventory  nullłI");
        }
    }
}
