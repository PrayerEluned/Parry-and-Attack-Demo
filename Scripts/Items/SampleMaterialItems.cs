using UnityEngine;
using Game.Items;

[CreateAssetMenu(fileName = "NewMaterialItem", menuName = "Items/Material Item")]
public class MaterialItem : ConsumableItem
{
    [Header("素材アイテム設定")]
    [TextArea(3, 5)]
    public string materialDescription = "素材アイテムの説明";
    
    public MaterialType materialType = MaterialType.Common;
    
    private void OnEnable()
    {
        // 既存のConsumableItemを継承しているので、基本設定は不要
        if (string.IsNullOrEmpty(description))
        {
            description = materialDescription;
        }
    }
}

public enum MaterialType
{
    Common,    // 一般素材
    Rare,      // レア素材
    Epic,      // エピック素材
    Legendary  // レジェンダリー素材
} 