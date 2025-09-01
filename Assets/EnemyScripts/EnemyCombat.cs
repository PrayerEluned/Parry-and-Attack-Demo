using UnityEngine;
using UnityEngine.UI;

public class EnemyCombat : MonoBehaviour
{
    [SerializeField] public EnemyStats stats;
    [SerializeField] private GameObject uiPrefab;

    private float currentHP;
    private float attackTimer;
    public bool IsDead { get; private set; } = false;

    private EnemySpawner spawner;

    // UI�Q��
    private Slider hpSlider;
    private Slider attackSlider;

    public void Initialize(EnemyStats s, EnemySpawner originSpawner)
    {
        stats = s;
        currentHP = stats.TotalHP;
        spawner = originSpawner;

        SetupUI();
    }

    private void SetupUI()
    {
        if (uiPrefab != null)
        {
            GameObject uiInstance = Instantiate(uiPrefab, transform);
            uiInstance.transform.localPosition = new Vector3(0, 0.5f, 0);

            Slider[] sliders = uiInstance.GetComponentsInChildren<Slider>();
            foreach (Slider slider in sliders)
            {
                if (slider.name.Contains("HP")) hpSlider = slider;
                else if (slider.name.Contains("Attack")) attackSlider = slider;
            }

            if (hpSlider != null) hpSlider.maxValue = stats.TotalHP;
        }
    }

    public void TryAttack(PlayerStats playerStats, bool inAttackRange)
    {
        if (Time.timeScale == 0f)
        {
            // Debug.Log("TryAttack: ゲーム停止中なので攻撃無効");
            return;
        }
        if (IsDead || playerStats == null) return;

        if (inAttackRange)
        {
            attackTimer += Time.deltaTime;
        }

        if (attackTimer >= AttackInterval)
        {
            attackTimer = 0f;

            int damage = DamageCalculator.CalculatePhysicalDamage(
                stats.TotalAttack,
                playerStats.stats.TotalDefense
            );
            playerStats.TakeDamage(damage);
        }

        // UI�Q�[�W�X�V
        if (hpSlider != null) hpSlider.value = currentHP;
        if (attackSlider != null)
        {
            attackSlider.maxValue = AttackInterval;
            attackSlider.value = attackTimer;
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHP -= damage;
        if (currentHP <= 0)
        {
            Die();
        }
        
        // ダメージテキスト表示（敵の位置に表示）
        Debug.Log($"EnemyCombat: TakeDamage called - Damage: {damage}, Enemy Position: {transform.position}");
        
        var damageTextComponent = GetComponent<DamageTextComponent>();
        if (damageTextComponent != null)
        {
            Debug.Log($"EnemyCombat: Using DamageTextComponent");
            damageTextComponent.OnTakeDamage(Mathf.RoundToInt(damage));
        }
        else
        {
            Debug.Log($"EnemyCombat: Using DamageTextManager directly");
            // DamageTextComponentがない場合は直接マネージャーを使用
            // 敵の位置に少し上にオフセットして表示
            Vector3 enemyPosition = transform.position + Vector3.up * 0.8f; // オフセットを少し大きくする
            DamageTextManager.ShowDamageAt(enemyPosition, Mathf.RoundToInt(damage));
        }
    }

    private void Die()
    {
        IsDead = true;

        // 既存のArtifactドロップ
        var artifactDrop = GetComponent<EnemyArtifactDrop>();
        if (artifactDrop != null) artifactDrop.TryDropArtifact();

        // 新しいConsumableアイテムドロップ
        var itemDrop = GetComponent<EnemyItemDrop>();
        if (itemDrop != null) itemDrop.TryDropItems();

        if (spawner != null) spawner.NotifyEnemyDied();
        Destroy(gameObject);
    }

    public float AttackInterval => 5f / stats.TotalSpeed;

    // �O���A�N�Z�X�p
    public float CurrentHP => currentHP;
    public float MaxHP => stats.TotalHP;
    public float CurrentAttackTimer => attackTimer;
}
