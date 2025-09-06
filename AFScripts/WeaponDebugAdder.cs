using UnityEngine;
using System.Collections.Generic;

public class WeaponDebugAdder : MonoBehaviour
{
    [Header("デバッグ設定")]
    public ArtifactInventory artifactInventory;
    
    void Start()
    {
        if (artifactInventory == null)
        {
            artifactInventory = FindObjectOfType<ArtifactInventory>();
        }
        
        Debug.Log("WeaponDebugAdder: 開始時に武器を追加");
        AddTestWeapons();
            }
            
    void Update()
    {
        // F2キーで武器追加
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("F2: 武器追加実行");
            AddTestWeapons();
        }
        
        // F3キーで現在の所持武器数確認
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("F3: 所持武器確認");
            CheckCurrentWeapons();
        }
    }
    
    private void AddTestWeapons()
    {
        if (artifactInventory == null)
        {
            Debug.LogError("ArtifactInventoryが見つかりません");
            return;
        }
        
        // Resourcesから武器を検索して追加
        var allWeapons = Resources.FindObjectsOfTypeAll<WeaponItem>();
        Debug.Log($"検索された武器数: {allWeapons.Length}");
        
        int addedCount = 0;
        foreach (var weapon in allWeapons)
        {
            if (weapon != null)
            {
                artifactInventory.AddWeapon(weapon, 1);
                Debug.Log($"武器追加: {weapon.weaponName}");
                addedCount++;
            }
        }
        
        Debug.Log($"武器追加完了: {addedCount}個");
    }
    
    private void CheckCurrentWeapons()
    {
        if (artifactInventory == null)
        {
            Debug.LogError("ArtifactInventoryが見つかりません");
            return;
        }
        
        var ownedWeapons = artifactInventory.GetAllOwnedWeapons();
        Debug.Log($"現在の所持武器数: {ownedWeapons.Count}");
        
        foreach (var weapon in ownedWeapons)
        {
            if (weapon != null)
            {
                Debug.Log($"  - {weapon.weaponName}");
            }
        }
    }
}