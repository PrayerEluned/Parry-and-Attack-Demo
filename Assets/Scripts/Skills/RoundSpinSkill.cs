using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoundSpinSkill : MonoBehaviour, ISkillBehaviour
{
    [SerializeField] private float duration = 0.5f;   // モーション持続時間
    [SerializeField] private float spinRadius = 1.0f; // 回転半径（Inspectorで調整）
    [SerializeField] private float iconAlpha = 0.8f;  // アイコン透明度
    [SerializeField] private float iconScale = 1.0f;  // アイコンのスケール
    [SerializeField] private float damageMultiplier = 1.0f; // ダメージ倍率
    private GameObject owner;
    private SkillData data;
    public float Duration => duration;
    private CainosWeaponIconDisplay weaponIconDisplay;

    public void Init(GameObject owner, SkillData data)
    {
        Debug.Log("[RoundSpin] Init: スキル発動");
        this.owner = owner;
        this.data = data;
        
        // CainosWeaponIconDisplayを取得
        weaponIconDisplay = owner.GetComponent<CainosWeaponIconDisplay>();
        if (weaponIconDisplay == null)
        {
            Debug.LogWarning("[RoundSpin] CainosWeaponIconDisplayが見つかりません。通常のSpriteRendererを使用します。");
            // フォールバック: 通常のSpriteRendererを使用
            var wm = owner.GetComponent<WeaponManager>();
            var item = wm.currentWeapon.weaponItem;
            var sr = GetComponent<SpriteRenderer>();
            if (sr && item != null) {
                sr.sprite = item.icon;
                sr.transform.localScale = Vector3.one * iconScale;
                sr.color = new Color(1, 1, 1, iconAlpha);
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
        
        StartCoroutine(SpinMotion());
    }

    private IEnumerator SpinMotion()
    {
        float elapsed = 0f;
        float radius = spinRadius;
        Vector3 playerCenter = owner.transform.position;
        var hitEnemies = new HashSet<EnemyHealth>();
        
        while (elapsed < duration)
        {
            float angle = 360f * (elapsed / duration);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 circlePos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
            Vector3 iconPos = playerCenter + circlePos;
            
            // CainosWeaponIconDisplayを使用してアイコンの位置と回転を更新
            if (weaponIconDisplay != null)
            {
                weaponIconDisplay.UpdateSkillIconPosition(iconPos);
                Vector3 dir = (playerCenter - iconPos).normalized;
                float spriteAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
                weaponIconDisplay.UpdateSkillIconRotation(spriteAngle);
            }
            else
            {
                // フォールバック: 通常のSpriteRendererを使用
                var sr = GetComponent<SpriteRenderer>();
                if (sr) {
                    sr.transform.position = iconPos;
                    Vector3 dir = (playerCenter - iconPos).normalized;
                    float spriteAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
                    sr.transform.localEulerAngles = new Vector3(0, 0, spriteAngle);
                }
            }
            
            DoSpinHitAtPosition(iconPos, iconScale * 0.5f, hitEnemies);
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
            if (sr) sr.transform.localEulerAngles = Vector3.zero;
        }
    }

    private void DoSpinHitAtPosition(Vector3 pos, float hitRadius, HashSet<EnemyHealth> hitEnemies)
    {
        var wm = owner.GetComponent<WeaponManager>();
        
        // BasicAttackControllerと同じ方法で敵を検出
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, hitRadius);
        foreach (var hit in hits)
        {
            // 敵かどうかを判定
            if (!IsEnemyObject(hit)) continue;
            
            var enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null || hitEnemies.Contains(enemy)) continue;
            
            float dmg = DamageCalculator.CalculatePhysicalDamage(wm.GetTotalAttack(), enemy.Stats.TotalDefense) * damageMultiplier;
            enemy.TakeDamage(dmg);
            
            Debug.Log($"[RoundSpin] 敵に {dmg} ダメージを与えました");
            
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
            Debug.LogWarning("RoundSpinSkill: 'Enemy'タグが定義されていません。EnemyHealthコンポーネントで敵を識別します。");
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(owner.transform.position, spinRadius);
    }
} 