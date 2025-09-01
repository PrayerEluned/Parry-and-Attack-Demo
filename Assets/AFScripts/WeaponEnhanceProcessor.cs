using UnityEngine;
using Game.Items;
using System.Collections.Generic;

[System.Serializable]
public class EnhancedWeaponData
{
    public string weaponID;
    public int enhancementLevel;
    
    public EnhancedWeaponData(string id, int level = 0)
    {
        weaponID = id;
        enhancementLevel = level;
    }
}

[System.Serializable]
public class SerializableEnhancedWeaponList
{
    public List<EnhancedWeaponData> weapons;
    
    public SerializableEnhancedWeaponList(List<EnhancedWeaponData> weaponList)
    {
        weapons = weaponList;
    }
}

public class WeaponEnhanceProcessor : MonoBehaviour
{
    [Header("強化武器データ保存")]
    [SerializeField] private List<EnhancedWeaponData> enhancedWeapons = new List<EnhancedWeaponData>();
    
    private ArtifactInventory inventory;
    private const string SAVE_KEY = "EnhancedWeaponsData";
    
    private void Start()
    {
        inventory = GetComponent<ArtifactInventory>();
        if (inventory == null)
            inventory = ArtifactInventory.Instance;
            
        // セーブ機能を無効化：常に初期化状態から開始
        enhancedWeapons = new List<EnhancedWeaponData>();
        Debug.Log("WeaponEnhanceProcessor: 初期化状態で開始しました");
        
        // // 強化データをロード（無効化）
        // LoadEnhancementData();
    }
    
    // セーブ・ロード機能を無効化（コメントアウト）
    /*
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveEnhancementData();
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SaveEnhancementData();
    }
    
    private void OnDestroy()
    {
        SaveEnhancementData();
    }
    */
    
    /// <summary>
    /// 強化データをセーブ
    /// </summary>
    public void SaveEnhancementData()
    {
        try
        {
            string jsonData = JsonUtility.ToJson(new SerializableEnhancedWeaponList(enhancedWeapons));
            PlayerPrefs.SetString(SAVE_KEY, jsonData);
            PlayerPrefs.Save();
            Debug.Log($"強化データをセーブしました: {enhancedWeapons.Count}件");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"強化データセーブエラー: {e.Message}");
        }
    }
    
    /// <summary>
    /// 強化データをロード
    /// </summary>
    public void LoadEnhancementData()
    {
        try
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string jsonData = PlayerPrefs.GetString(SAVE_KEY);
                var loadedData = JsonUtility.FromJson<SerializableEnhancedWeaponList>(jsonData);
                if (loadedData != null && loadedData.weapons != null)
                {
                    enhancedWeapons = loadedData.weapons;
                    Debug.Log($"強化データをロードしました: {enhancedWeapons.Count}件");
                }
            }
            else
            {
                Debug.Log("強化データが見つかりません。新規開始します。");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"強化データロードエラー: {e.Message}");
            enhancedWeapons = new List<EnhancedWeaponData>();
        }
    }
    
    /// <summary>
    /// 武器を強化する
    /// </summary>
    public bool EnhanceWeapon(WeaponItem weapon)
    {
        if (weapon == null || inventory == null)
        {
            Debug.LogError("WeaponEnhanceProcessor: weapon または inventory が null です");
            return false;
        }
            
        // 素材の確認（事前チェック）
        if (!CanEnhanceWeapon(weapon))
        {
            Debug.LogWarning($"WeaponEnhanceProcessor: {weapon.weaponName} の強化に必要な素材が足りません");
            return false;
        }
        
        // 強化前の素材数を記録（巻き戻し用）
        var materialSnapshot = new List<(ConsumableItem material, int beforeCount)>();
        foreach (var req in weapon.enhancementMaterials)
        {
            int beforeCount = inventory.GetConsumableCount(req.material);
            materialSnapshot.Add((req.material, beforeCount));
            Debug.Log($"強化前の素材数: {req.material.ItemName} = {beforeCount}個");
        }
            
        // 素材を消費
        bool consumeSuccess = true;
        foreach (var req in weapon.enhancementMaterials)
        {
            Debug.Log($"素材消費開始: {req.material.ItemName} を {req.requiredAmount}個");
            
            if (!ConsumeMaterial(req.material, req.requiredAmount))
            {
                Debug.LogError($"素材消費に失敗: {req.material.ItemName}");
                consumeSuccess = false;
                break;
            }
        }
        
        // 素材消費に失敗した場合は巻き戻し
        if (!consumeSuccess)
        {
            Debug.LogError("素材消費に失敗したため、強化をキャンセルします");
            // TODO: 必要に応じて巻き戻し処理を実装
            return false;
        }
        
        // 消費後の素材数を確認
        foreach (var req in weapon.enhancementMaterials)
        {
            int afterCount = inventory.GetConsumableCount(req.material);
            Debug.Log($"強化後の素材数: {req.material.ItemName} = {afterCount}個");
        }
        
        // 強化レベルを上げる
        var enhancedData = enhancedWeapons.Find(data => data.weaponID == weapon.weaponID);
        if (enhancedData == null)
        {
            enhancedData = new EnhancedWeaponData(weapon.weaponID, 1);
            enhancedWeapons.Add(enhancedData);
            Debug.Log($"新規強化データ作成: {weapon.weaponName} レベル1");
        }
        else
        {
            enhancedData.enhancementLevel++;
            Debug.Log($"強化レベル更新: {weapon.weaponName} レベル{enhancedData.enhancementLevel}");
        }
        
        // // 強化データを自動セーブ（無効化）
        // SaveEnhancementData();
        
        Debug.Log($"🎉 武器 {weapon.weaponName} を強化しました！ 強化レベル: {enhancedData.enhancementLevel}");
        return true;
    }
    
    /// <summary>
    /// 武器が強化可能かチェック
    /// </summary>
    public bool CanEnhanceWeapon(WeaponItem weapon)
    {
        if (weapon == null || inventory == null)
            return false;
            
        foreach (var req in weapon.enhancementMaterials)
        {
            int owned = inventory.GetConsumableCount(req.material);
            if (owned < req.requiredAmount)
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 武器の現在の強化レベルを取得
    /// </summary>
    public int GetWeaponEnhanceLevel(WeaponItem weapon)
    {
        if (weapon == null)
            return 0;
            
        var enhancedData = enhancedWeapons.Find(data => data.weaponID == weapon.weaponID);
        return enhancedData?.enhancementLevel ?? 0;
    }
    
    /// <summary>
    /// 武器の強化後ステータスを取得
    /// </summary>
    public float GetEnhancedStat(WeaponItem weapon, string statType)
    {
        if (weapon == null)
            return 0f;
            
        int enhanceLevel = GetWeaponEnhanceLevel(weapon);
        
        switch (statType.ToLower())
        {
            case "hp":
                return weapon.bonusHP + (weapon.enhanceHP * enhanceLevel);
            case "attack":
                return weapon.bonusAttack + (weapon.enhanceAttack * enhanceLevel);
            case "magicattack":
                return weapon.bonusMagicAttack + (weapon.enhanceMagicAttack * enhanceLevel);
            case "defense":
                return weapon.bonusDefense + (weapon.enhanceDefense * enhanceLevel);
            case "magicdefense":
                return weapon.bonusMagicDefense + (weapon.enhanceMagicDefense * enhanceLevel);
            case "fate":
                return weapon.bonusFate + (weapon.enhanceFate * enhanceLevel);
            default:
                return 0f;
        }
    }
    
    /// <summary>
    /// 素材を消費する
    /// </summary>
    private bool ConsumeMaterial(ConsumableItem material, int amount)
    {
        if (inventory == null)
        {
            Debug.LogError("WeaponEnhanceProcessor: ArtifactInventoryが見つかりません！");
            return false;
        }
        
        // 事前に十分な数があるかチェック
        int currentCount = inventory.GetConsumableCount(material);
        if (currentCount < amount)
        {
            Debug.LogError($"素材 {material.ItemName} が不足しています。必要: {amount}, 所持: {currentCount}");
            return false;
        }
        
        // RemoveItemを使用して素材を消費
        for (int i = 0; i < amount; i++)
        {
            inventory.RemoveItem(material, 1);
        }
        
        // 消費後の確認
        int afterCount = inventory.GetConsumableCount(material);
        int expectedAfter = currentCount - amount;
        if (afterCount != expectedAfter)
        {
            Debug.LogWarning($"素材消費の結果が期待値と異なります。期待: {expectedAfter}, 実際: {afterCount}");
        }
        
        Debug.Log($"素材 {material.ItemName} を {amount} 個消費しました (残り: {afterCount}個)");
        return true;
    }
}