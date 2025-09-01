using UnityEngine;
using UnityEngine.UI;

public class EnemyUIController : MonoBehaviour
{
    public Slider hpSlider;
    public Slider attackSlider;

    private EnemyHealth health;

    private void Start()
    {
        Debug.Log("EnemyUIController: 敵システム解禁テスト - UI初期化を開始");
        
        try
        {
            // 基本的な敵UI初期化
            InitializeEnemyUI();
            Debug.Log("EnemyUIController: 敵UI初期化完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EnemyUIController: 初期化エラー - {e.Message}");
        }
    }
    
    private void InitializeEnemyUI()
    {
        // 敵UIの基本初期化
        if (transform != null)
        {
            Debug.Log("EnemyUIController: 敵UIコンポーネント確認完了");
        }
        
        // EnemyHealthコンポーネントを取得
        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
        }
        
        // HPスライダーの初期設定
        if (hpSlider != null && health != null)
        {
            hpSlider.maxValue = 1f; // 比率で管理
            hpSlider.value = health.CurrentHP / health.MaxHP;
            Debug.Log($"EnemyUIController: HPスライダー初期化 - CurrentHP: {health.CurrentHP}, MaxHP: {health.MaxHP}");
        }
    }

    private void Update()
    {
        // EnemyHealthが初期化されるまで待機
        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
            if (health != null)
            {
                // EnemyHealthが見つかったら初期化を再実行
                InitializeEnemyUI();
            }
            return;
        }

        // HP更新はEnemyHealthが管理するため、ここでは更新しない
        // 攻撃ゲージ更新（EnemyAttackSystemが管理）
        // attackSlider.value = health.CurrentAttackTimer / health.AttackInterval;
    }
    
    /// <summary>
    /// UI更新メソッド（外部から呼び出し可能）
    /// </summary>
    public void UpdateUI()
    {
        if (health == null) 
        {
            Debug.LogWarning("EnemyUIController: healthがnullです");
            return;
        }

        // HP更新
        if (hpSlider != null)
        {
            float hpRatio = health.CurrentHP / health.MaxHP;
            hpSlider.value = hpRatio;
            //Debug.Log($"EnemyUIController: HP更新 - CurrentHP: {health.CurrentHP}, MaxHP: {health.MaxHP}, Ratio: {hpRatio}");
        }
        else
        {
            Debug.LogWarning("EnemyUIController: hpSliderがnullです");
        }
    }
}
