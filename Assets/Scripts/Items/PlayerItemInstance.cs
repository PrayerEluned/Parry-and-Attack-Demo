using UnityEngine;
using Game.Items;

namespace Game.Items
{
    /// <summary>
    /// プレイヤーの所持アイテムインスタンス（SO参照＋所持数）
    /// </summary>
    [System.Serializable]
    public class PlayerItemInstance
    {
        [Header("アイテム定義")]
        public ScriptableObject itemSO; // IPlayerItemを実装したSO
        
        [Header("所持情報")]
        public int count = 1;
        
        // SOからIPlayerItemにキャスト
        public IPlayerItem Item => itemSO as IPlayerItem;
        
        // 便利プロパティ
        public string ItemName => Item?.ItemName ?? "不明";
        public Sprite Icon => Item?.Icon;
        public string Description => Item?.Description ?? "説明がありません";
        public ItemType Type => Item?.Type ?? ItemType.Consumable;
        
        // 最大所持数チェック
        public bool CanAddMore
        {
            get
            {
                var maxStack = GetMaxStackCount();
                return count < maxStack;
            }
        }
        
        /// <summary>
        /// SOの種類に応じて最大所持数を取得
        /// </summary>
        public int GetMaxStackCount()
        {
            switch (itemSO)
            {
                case ConsumableItem consumable:
                    return consumable.maxStack;
                case ArtifactItem artifact:
                    return artifact.maxStackCount;
                case EnhancePatch patch:
                    return patch.maxStack;
                default:
                    return 999; // デフォルト値
            }
        }
        
        /// <summary>
        /// 所持数を追加（最大値でクランプ）
        /// </summary>
        public void AddCount(int amount)
        {
            int prev = count;
            count = Mathf.Min(count + amount, GetMaxStackCount());
            Debug.Log($"[PlayerItemInstance] AddCount: {ItemName} {prev} -> {count} (追加: {amount})");
        }
        
        /// <summary>
        /// 所持数を減らす
        /// </summary>
        public bool RemoveCount(int amount)
        {
            if (count >= amount)
            {
                int prev = count;
                count -= amount;
                Debug.Log($"[PlayerItemInstance] RemoveCount: {ItemName} {prev} -> {count} (減少: {amount})");
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 表示用の所持数テキスト
        /// </summary>
        public string GetCountText()
        {
            var maxStack = GetMaxStackCount();
            if (count >= maxStack)
                return "MAX";
            else
                return $"x{count}";
        }
        
        // コンストラクタ
        public PlayerItemInstance(ScriptableObject so, int initialCount = 1)
        {
            itemSO = so;
            count = Mathf.Max(1, initialCount);
        }
    }
}