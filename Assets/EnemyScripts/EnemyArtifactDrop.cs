using UnityEngine;
using System.Linq;

public class EnemyArtifactDrop : MonoBehaviour
{
    public ArtifactItem artifactToDrop;
    [Range(0f, 1f)] public float dropChance = 1f;

    private ArtifactInventory inventory;

    private void Start()
    {
        Debug.Log("Unity 6フリーズ対策: EnemyArtifactDrop 敵ドロップ機能を復活");
        
        // フリーズ対策：Singletonのみ使用
        inventory = ArtifactInventory.Instance;
        if (inventory == null)
        {
            Debug.LogError("EnemyArtifactDrop: ArtifactInventory.Instance が null です");
        }
    }

    public void TryDropArtifact()
    {
        // Unity 6でのフリーズ対策：安全なドロップ処理
        Debug.Log("EnemyArtifactDrop: TryDropArtifact 呼び出し");
        
        // 毎回念のため再取得（フリーズ対策：Singletonのみ）
        if (inventory == null)
        {
            inventory = ArtifactInventory.Instance;
        }

        Debug.Log($"[EnemyArtifactDrop] TryDropArtifact: inventory={inventory?.GetInstanceID()} artifactToDrop={artifactToDrop?.artifactName}");

        // 特殊効果パッチによるAFドロップ率増加を適用
        float finalDropChance = dropChance;
        var playerStats = PlayerStats.Instance;
        if (playerStats != null)
        {
            var specialEffect = playerStats.GetComponent<SpecialPatchEffect>();
            if (specialEffect != null)
            {
                float dropRateBonus = specialEffect.GetAFDropRateBonus();
                if (dropRateBonus > 1f)
                {
                    finalDropChance = Mathf.Min(1f, dropChance * dropRateBonus);
                    Debug.Log($"特殊効果パッチ: AFドロップ率 {dropChance} → {finalDropChance} (倍率: {dropRateBonus})");
                }
            }
        }
        
        if (Random.value < finalDropChance && artifactToDrop != null && inventory != null)
        {
            Debug.Log($"[EnemyArtifactDrop] {artifactToDrop.artifactName} をドロップ！インベントリID={inventory.GetInstanceID()}");
            
            try
            {
                inventory.AddArtifact(artifactToDrop, 1);
                PlayerStats.Instance?.ApplyAllEffects();
                
                // ドロップ成功のログ
                var allArtifacts = inventory.GetAllOwnedArtifacts();
                Debug.Log($"[EnemyArtifactDrop] ドロップ成功！現在のAF数: {allArtifacts.Count}");
                
                // プレイヤーにドロップ通知（軽量版）
                NotifyPlayerOfDrop(artifactToDrop);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EnemyArtifactDrop] ドロップ処理エラー: {e.Message}");
            }
        }
        else if (inventory == null)
        {
            Debug.LogWarning("EnemyArtifactDrop: ArtifactInventoryが見つかりません。AFドロップ失敗。");
        }
        else if (artifactToDrop == null)
        {
            Debug.LogWarning("EnemyArtifactDrop: artifactToDropが設定されていません。");
        }
        else
        {
            Debug.Log($"[EnemyArtifactDrop] ドロップ判定失敗: dropChance={dropChance}, roll={Random.value}");
        }
    }
    
    private void NotifyPlayerOfDrop(ArtifactItem artifact)
    {
        // UIManagerの軽量ドロップ通知
        var uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            Debug.Log($"UI通知: {artifact.artifactName} を手に入れました！");
        }
    }
}

