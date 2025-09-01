using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VerticalPierceSkill : MonoBehaviour, ISkillBehaviour
{
    [SerializeField] private float range = 1.0f;      // 基準レンジ
    [SerializeField] private float duration = 0.5f;   // モーション持続時間
    [SerializeField] private float iconAlpha = 0.3f;  // アイコン透明度
    [SerializeField] private float iconScale = 1.0f;  // アイコンのスケール（Inspectorで調整）
    [SerializeField] private float offsetDistance = 0.5f; // キャラの向きにどれだけ離すか
    [SerializeField] private float damageMultiplier = 1.0f; // ダメージ倍率（Inspectorで調整）
    private GameObject owner;
    private SkillData data;
    public float Duration => duration;
    private Vector2 inputDir = Vector2.up;
    private CainosWeaponIconDisplay weaponIconDisplay;

    public void Init(GameObject owner, SkillData data, Vector2 inputDir)
    {
        Debug.Log("[VerticalPierce] Init: スキル発動");
        this.owner = owner;
        this.data = data;
        this.inputDir = inputDir.normalized;
        
        // CainosWeaponIconDisplayを取得
        weaponIconDisplay = owner.GetComponent<CainosWeaponIconDisplay>();
        if (weaponIconDisplay == null)
        {
            Debug.LogWarning("[VerticalPierce] CainosWeaponIconDisplayが見つかりません。通常のSpriteRendererを使用します。");
            // フォールバック: 通常のSpriteRendererを使用
            var wm = owner.GetComponent<WeaponManager>();
            var item = wm.currentWeapon.weaponItem;
            var sr = GetComponent<SpriteRenderer>();
            if (sr && item != null) {
                sr.sprite = item.icon;
                sr.transform.localScale = Vector3.one * iconScale;
                sr.color = new Color(1, 1, 1, iconAlpha);
                sr.transform.localEulerAngles = new Vector3(0, 0, 0f); // 画像はデフォルト回転
            }
        }
        else
        {
            // CainosWeaponIconDisplayを使用してスキルアイコンを設定
            var wm = owner.GetComponent<WeaponManager>();
            var item = wm.currentWeapon.weaponItem;
            if (item != null)
            {
                weaponIconDisplay.SetSkillIcon(item.icon, iconScale, iconAlpha);
            }
        }
        
        StartCoroutine(PierceMotion());
    }

    public void Init(GameObject owner, SkillData data)
    {
        var move = owner.GetComponent<CharacterMovement>();
        Vector2 facingDir = move != null && move.LastMoveDirection != Vector2.zero ? move.LastMoveDirection : owner.transform.up;
        Init(owner, data, facingDir);
    }

    private IEnumerator PierceMotion()
    {
        Debug.Log($"[VerticalPierce] Motion開始 range={range}");
        float elapsed = 0f;
        
        Vector3 start = owner.transform.position; // 始点はPlayer中心
        Vector3 end = owner.transform.position + (Vector3)inputDir * (range * 3f); // 終点はinputDir方向にrange*3
        Vector2 boxSize = new Vector2(iconScale, iconScale); // 判定はアイコンサイズ
        float iconAngle = Vector2.SignedAngle(Vector2.up, inputDir); // デフォルトup基準
        var hitEnemies = new HashSet<EnemyHealth>();
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 pos = Vector3.Lerp(start, end, t) + (Vector3)inputDir * (iconScale * 0.5f); // 長方形の辺がPlayer中心
            
            // CainosWeaponIconDisplayを使用してアイコンの位置と回転を更新
            if (weaponIconDisplay != null)
            {
                weaponIconDisplay.UpdateSkillIconPosition(pos);
                weaponIconDisplay.UpdateSkillIconRotation(iconAngle);
            }
            else
            {
                // フォールバック: 通常のSpriteRendererを使用
                var sr = GetComponent<SpriteRenderer>();
                if (sr) {
                    sr.transform.position = pos;
                    sr.transform.localEulerAngles = new Vector3(0, 0, 0f);
                }
            }
            
            // 攻撃判定（現在位置でOverlapBox）
            DoPierceHitAtPosition(pos, boxSize, iconAngle, hitEnemies);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // スキル終了時の処理
        if (weaponIconDisplay != null)
        {
            weaponIconDisplay.EndSkillMode();
        }
        else
        {
            // フォールバック: 通常のSpriteRendererを使用
            var sr = GetComponent<SpriteRenderer>();
            if (sr) {
                sr.transform.position = end + (Vector3)inputDir * (iconScale * 0.5f);
                sr.transform.localEulerAngles = new Vector3(0, 0, 0f);
            }
        }
        
        Debug.Log("[VerticalPierce] Motion終了");
    }

    private void DoPierceHitAtPosition(Vector3 pos, Vector2 size, float angle, HashSet<EnemyHealth> hitEnemies)
    {
        var wm = owner.GetComponent<WeaponManager>();
        
        // BasicAttackControllerと同じ方法で敵を検出
        Collider2D[] hits = Physics2D.OverlapBoxAll(pos, size, angle);
        foreach (var hit in hits)
        {
            // 敵かどうかを判定
            if (!IsEnemyObject(hit)) continue;
            
            var enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null || hitEnemies.Contains(enemy)) continue;
            
            float dmg = DamageCalculator.CalculatePhysicalDamage(wm.GetTotalAttack(), enemy.Stats.TotalDefense) * damageMultiplier;
            enemy.TakeDamage(dmg);
            
            Debug.Log($"[VerticalPierce] 敵に {dmg} ダメージを与えました");
            
            hitEnemies.Add(enemy);
        }
    }
    
    private bool IsEnemyObject(Collider2D target)
    {
        // BasicAttackControllerと同じ敵識別ロジック
        
        // 方法1: EnemyHealthコンポーネントが付いているかチェック
        if (target.GetComponent<EnemyHealth>() != null)
        {
            return true;
        }
        
        // 方法2: タグが存在する場合のみチェック
        try
        {
            if (target.CompareTag("Enemy"))
            {
                return true;
            }
        }
        catch (UnityException)
        {
            // タグが存在しない場合はスキップ
            Debug.LogWarning("VerticalPierceSkill: 'Enemy'タグが定義されていません。EnemyHealthコンポーネントで敵を識別します。");
        }
        
        // 方法3: オブジェクト名による識別（フォールバック）
        if (target.name.ToLower().Contains("enemy"))
        {
            return true;
        }
        
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (owner == null) return;
        Vector3 start = owner.transform.position;
        Vector3 end = owner.transform.position + (Vector3)inputDir * (range * 3f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(start, end);
        // 長方形の始点がPlayer中心になるように、Gizmosも修正
        Vector3 center = Vector3.Lerp(start, end, 0.5f) + (Vector3)inputDir * (iconScale * 0.5f);
        Gizmos.DrawWireCube(center, new Vector3(iconScale, range * 3f, 1f));
    }
} 