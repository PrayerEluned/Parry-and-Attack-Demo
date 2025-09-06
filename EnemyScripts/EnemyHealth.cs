using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyStats defaultStats; // デフォルト値（インスペクター用）
    [SerializeField] private GameObject uiPrefab;

    private EnemyStats stats; // 実際に使用するstats
    private float currentHP;
    public bool IsDead { get; private set; } = false;

    private EnemySpawner spawner;
    
    // UI管理
    private Slider hpSlider;

    public void Initialize(EnemyStats s, EnemySpawner originSpawner)
    {
        // 渡されたEnemyStatsを確実に使用
        stats = s;
        currentHP = stats.TotalHP;
        spawner = originSpawner;

        // UIの初期化を少し遅らせて、EnemyAttackSystemのUI作成後に実行
        StartCoroutine(DelayedSetupUI());
        
        Debug.Log($"EnemyHealth: 初期化完了 - CurrentHP: {currentHP}, MaxHP: {stats.TotalHP}");
    }
    
    private System.Collections.IEnumerator DelayedSetupUI()
    {
        // 1フレーム待ってからUI設定（EnemyAttackSystemのUI作成を待つ）
        yield return null;
        SetupUI();
    }

    private void SetupUI()
    {
        // 既存のUIを探す（EnemyAttackSystemが作成したものも含む）
        Slider[] existingSliders = GetComponentsInChildren<Slider>();
        bool foundHP = false;
        
        foreach (Slider slider in existingSliders)
        {
            if (slider.name.Contains("HP") && !foundHP)
            {
                hpSlider = slider;
                hpSlider.maxValue = stats.TotalHP;
                hpSlider.value = currentHP;
                foundHP = true;
                Debug.Log($"EnemyHealth: 既存のHPスライダーを使用 - CurrentHP: {currentHP}, MaxHP: {stats.TotalHP}");
            }
        }
        
        // 既存のUIが見つからない場合のみ新規作成
        if (!foundHP)
        {
            if (uiPrefab != null)
            {
                GameObject uiInstance = Instantiate(uiPrefab, transform);
                uiInstance.transform.localPosition = new Vector3(0, 0.5f, 0);

                Slider[] sliders = uiInstance.GetComponentsInChildren<Slider>();
                foreach (Slider slider in sliders)
                {
                    if (slider.name.Contains("HP") && !foundHP)
                    {
                        hpSlider = slider;
                        hpSlider.maxValue = stats.TotalHP;
                        hpSlider.value = currentHP;
                        foundHP = true;
                        Debug.Log($"EnemyHealth: 新しいHPスライダーを作成 - CurrentHP: {currentHP}, MaxHP: {stats.TotalHP}");
                    }
                }
            }
        }
        
        // UIが見つからない場合の警告
        if (hpSlider == null)
        {
            Debug.LogWarning("EnemyHealth: HPゲージ用のSliderが見つかりません。UI Prefabを設定してください。");
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        float previousHP = currentHP;
        currentHP -= damage;
        
        // HPスライダーを直接更新
        if (hpSlider != null)
        {
            hpSlider.value = currentHP; // 絶対値で更新
            Debug.Log($"EnemyHealth: HP更新 - Damage: {damage}, PreviousHP: {previousHP}, CurrentHP: {currentHP}, MaxHP: {stats.TotalHP}, SliderValue: {hpSlider.value}, SliderMaxValue: {hpSlider.maxValue}");
        }
        else
        {
            Debug.LogWarning("EnemyHealth: TakeDamage - hpSliderがnullです。UIの再初期化が必要かもしれません。");
            // UIの再初期化を試行
            ReinitializeUI();
            if (hpSlider != null)
            {
                hpSlider.value = currentHP;
                Debug.Log($"EnemyHealth: UI再初期化後にHPスライダーを更新 - CurrentHP: {currentHP}");
            }
        }
        
        if (currentHP <= 0)
        {
            Die();
        }
        
        // ダメージテキスト表示（敵の位置に表示）
        var damageTextComponent = GetComponent<DamageTextComponent>();
        if (damageTextComponent != null)
        {
            damageTextComponent.OnTakeDamage(Mathf.RoundToInt(damage));
        }
        else
        {
            // DamageTextComponentがない場合は直接マネージャーを使用
            // 敵の位置に少し上にオフセットして表示
            Vector3 enemyPosition = transform.position + Vector3.up * 0.8f;
            DamageTextManager.ShowDamageAt(enemyPosition, Mathf.RoundToInt(damage));
        }
    }

    private void Die()
    {
        IsDead = true;

        // 経験値をプレイヤーに与える
        GiveExperienceToPlayer();

        // 既存のArtifactドロップ
        var artifactDrop = GetComponent<EnemyArtifactDrop>();
        if (artifactDrop != null) artifactDrop.TryDropArtifact();

        // 新しいConsumableアイテムドロップ
        var itemDrop = GetComponent<EnemyItemDrop>();
        if (itemDrop != null) itemDrop.TryDropItems();

        if (spawner != null) spawner.NotifyEnemyDied();
        Destroy(gameObject);
    }

    /// <summary>
    /// プレイヤーに経験値を与える
    /// </summary>
    private void GiveExperienceToPlayer()
    {
        // EnemyItemDropがある場合はそちらで経験値を処理するため、ここでは何もしない
        var itemDrop = GetComponent<EnemyItemDrop>();
        if (itemDrop != null)
        {
            Debug.Log("EnemyHealth: EnemyItemDropが存在するため、経験値処理はそちらに委ねます");
            return;
        }
        
        // EnemyItemDropがない場合のみ、EnemyStatsの経験値を使用
        if (stats == null)
        {
            Debug.LogWarning("EnemyHealth: statsがnullのため経験値を与えられません");
            return;
        }

        int experienceReward = stats.GetExperienceReward();
        
        // プレイヤーのPlayerStatsを取得
        var playerStats = PlayerStats.Instance;
        if (playerStats != null)
        {
            playerStats.GainEXP(experienceReward);
            Debug.Log($"EnemyHealth: プレイヤーに経験値 {experienceReward} を与えました");
        }
        else
        {
            Debug.LogWarning("EnemyHealth: PlayerStats.Instanceが見つからないため経験値を与えられません");
        }
    }

    /// <summary>
    /// UIを再初期化（既存のUIを使用）
    /// </summary>
    public void ReinitializeUI()
    {
        if (stats == null) return;
        
        // 既存のUIを探す
        Slider[] existingSliders = GetComponentsInChildren<Slider>();
        bool foundHP = false;
        
        foreach (Slider slider in existingSliders)
        {
            if (slider.name.Contains("HP") && !foundHP)
            {
                hpSlider = slider;
                hpSlider.maxValue = stats.TotalHP;
                hpSlider.value = currentHP;
                foundHP = true;
                Debug.Log($"EnemyHealth: UI再初期化 - HPスライダーを既存のものに設定 - CurrentHP: {currentHP}, MaxHP: {stats.TotalHP}");
            }
        }
        
        if (!foundHP)
        {
            Debug.LogWarning("EnemyHealth: ReinitializeUI - HPスライダーが見つかりません");
        }
    }
    
    // プロパティ
    public float CurrentHP => currentHP;
    public float MaxHP => stats.TotalHP;
    public EnemyStats Stats => stats; // statsへのアクセス用プロパティ
    public EnemyStats DefaultStats => defaultStats; // defaultStatsへのアクセス用プロパティ
} 