using UnityEngine;
using System.Collections.Generic;

public class EnhancePanelDebugger : MonoBehaviour
{
    [Header("デバッグ情報")]
    [SerializeField] private EnhancePanelController enhancePanelController;
    [SerializeField] private ArtifactInventory artifactInventory;
    
    private void Start()
    {
        Debug.Log("EnhancePanelDebugger: デバッグシステム開始");
    }
    
    private void Update()
    {
        // F3キーでデバッグ情報表示
        if (Input.GetKeyDown(KeyCode.F3))
        {
            DebugWeaponSystem();
        }
        
        // F4キーで強制的に武器を追加
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ForceAddTestWeapon();
        }
    }
    
    [ContextMenu("武器システムデバッグ")]
    public void DebugWeaponSystem()
    {
        Debug.Log("=== 武器システムデバッグ開始 ===");
        
        // ArtifactInventoryの確認
        if (artifactInventory == null)
        {
            artifactInventory = GetComponent<ArtifactInventory>();
            if (artifactInventory == null)
                artifactInventory = FindFirstObjectByType<ArtifactInventory>();
        }
        
        if (artifactInventory == null)
        {
            Debug.LogError("ArtifactInventoryが見つかりません");
            return;
        }
        
        Debug.Log($"ArtifactInventory見つかりました: {artifactInventory.name}");
        
        // 所持武器数を確認
        var ownedWeapons = artifactInventory.GetAllOwnedWeapons();
        Debug.Log($"現在の所持武器数: {ownedWeapons.Count}");
        
        if (ownedWeapons.Count == 0)
        {
            Debug.LogWarning("武器が0個です！武器を追加してください");
        }
        else
        {
            Debug.Log("所持武器一覧:");
            foreach (var weapon in ownedWeapons)
            {
                Debug.Log($"- {weapon.weaponName} (ID: {weapon.weaponID})");
            }
        }
        
        // EnhancePanelControllerの確認
        if (enhancePanelController == null)
        {
            enhancePanelController = FindFirstObjectByType<EnhancePanelController>();
        }
        
        if (enhancePanelController != null)
        {
            Debug.Log("EnhancePanelController見つかりました");
        }
        else
        {
            Debug.LogError("EnhancePanelControllerが見つかりません");
        }
        
        Debug.Log("=== 武器システムデバッグ終了 ===");
    }
    
    [ContextMenu("テスト武器を強制追加")]
    public void ForceAddTestWeapon()
    {
        Debug.Log("=== テスト武器を強制追加 ===");
        
        if (artifactInventory == null)
        {
            artifactInventory = GetComponent<ArtifactInventory>();
            if (artifactInventory == null)
                artifactInventory = FindFirstObjectByType<ArtifactInventory>();
        }
        
        if (artifactInventory == null)
        {
            Debug.LogError("ArtifactInventoryが見つかりません");
            return;
        }
        
        // ScriptableObjectの武器を作成
        var testWeapon = ScriptableObject.CreateInstance<WeaponItem>();
        testWeapon.weaponID = "test_sword_001";
        testWeapon.weaponName = "テスト用剣";
        testWeapon.description = "デバッグ用のテスト武器";
        testWeapon.bonusAttack = 10f;
        testWeapon.bonusHP = 5f;
        testWeapon.enhanceAttack = 2f;
        testWeapon.enhanceHP = 1f;
        
        // インベントリに追加
        artifactInventory.AddWeapon(testWeapon, 1);
        Debug.Log($"テスト武器を追加しました: {testWeapon.weaponName}");
        
        // 追加確認
        var weapons = artifactInventory.GetAllOwnedWeapons();
        Debug.Log($"追加後の武器数: {weapons.Count}");
    }
}